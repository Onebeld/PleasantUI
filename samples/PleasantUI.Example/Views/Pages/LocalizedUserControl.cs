using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages;

/// <summary>
/// Base class for all sub-page views that contain <c>{Localize}</c> bindings.
///
/// Problem: LocalizeKeyObservable subscriptions are dropped when a view is detached from
/// the visual tree. When the user navigates back to a page, the bindings are stale.
/// ReinitializeComponent rebuilds the visual tree so fresh LocalizeKeyObservable instances
/// are created that read the current language immediately.
///
/// Problem with the old approach: ReinitializeComponent was called 3-4 times per language
/// change (triple-reinit in constructor + two passes in OnLanguageChanged), causing
/// LocalizeKeyObservable instances to accumulate in Localizer.AliveObservables and the
/// subscriber count to explode (263+ after a few switches).
///
/// Fix: use a single pending-reinit flag so only ONE ReinitializeComponent call is
/// dispatched per language change, regardless of how many times OnLanguageChanged fires
/// or how many constructor-time reinit paths are triggered.
/// </summary>
public abstract class LocalizedUserControl : UserControl
{
    // Guards against multiple concurrent reinit dispatches for the same language change.
    private int _reinitPending;

    protected LocalizedUserControl()
    {
        var currentLang = Localizer.Instance.CurrentLanguage;
        Debug.WriteLine($"[{GetType().Name}] Constructor START - CurrentLanguage={currentLang}");

        Localizer.Instance.LocalizationChanged += OnLanguageChanged;

        // If this view is constructed AFTER a language change has already completed
        // (e.g., user navigates to this page after switching language), the {Localize}
        // bindings created by InitializeComponent() already read the correct language —
        // no reinit needed. The LocalizeKeyObservable subscriptions will handle future changes.
        //
        // However, if the view is constructed WHILE a language change is in progress
        // (rare race), schedule a single reinit to be safe.
        if (!string.IsNullOrEmpty(currentLang) && currentLang != "en")
        {
            ScheduleReinit(currentLang, "constructor-time");
        }

        Debug.WriteLine($"[{GetType().Name}] Constructor END");
    }

    private void OnLanguageChanged(string lang)
    {
        Debug.WriteLine($"[LocalizedUserControl.{GetType().Name}] OnLanguageChanged lang={lang}");
        ScheduleReinit(lang, "OnLanguageChanged");
    }

    /// <summary>
    /// Schedules exactly one ReinitializeComponent call at Background priority.
    /// Subsequent calls before the dispatch fires are no-ops (flag is already set).
    /// </summary>
    private void ScheduleReinit(string lang, string reason)
    {
        // Atomically set the flag. If it was already 1, a reinit is already queued — skip.
        if (Interlocked.CompareExchange(ref _reinitPending, 1, 0) != 0)
        {
            Debug.WriteLine($"[{GetType().Name}] ScheduleReinit({reason}) skipped — already pending");
            return;
        }

        Debug.WriteLine($"[{GetType().Name}] ScheduleReinit({reason}) queued for lang={lang}");

        Dispatcher.UIThread.Post(() =>
        {
            // Clear the flag before reinit so any language change that fires DURING
            // reinit will schedule a new one rather than being silently dropped.
            Interlocked.Exchange(ref _reinitPending, 0);

            Debug.WriteLine($"[{GetType().Name}] Executing reinit for lang={Localizer.Instance.CurrentLanguage}");
            ReinitializeComponent();
        }, DispatcherPriority.Background);
    }

    /// <summary>
    /// Override in derived classes to call <c>InitializeComponent()</c> (and re-wire
    /// any event handlers). The base implementation calls it via reflection.
    /// </summary>
    protected virtual void ReinitializeComponent()
    {
        try
        {
            var oldDataContext = DataContext;
            DataContext = null;
            Content = null;

            MethodInfo? init = GetType().GetMethod(
                "InitializeComponent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);

            init?.Invoke(this, null);

            if (oldDataContext is not null && DataContext is null)
                DataContext = oldDataContext;

            Debug.WriteLine($"[{GetType().Name}] ReinitializeComponent done, lang={Localizer.Instance.CurrentLanguage}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[{GetType().Name}] ReinitializeComponent failed: {ex.Message}");
        }
    }
}
