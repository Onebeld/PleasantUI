using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages;

/// <summary>
/// Base class for all sub-page views that contain <c>{Localize}</c> bindings.
/// Subscribes to <see cref="Localizer.LocalizationChanged"/> and re-initializes
/// the visual tree at two dispatch priorities as a belt-and-suspenders failsafe,
/// ensuring static localized texts always update even if the binding engine
/// loses its <see cref="System.ComponentModel.INotifyPropertyChanged"/> subscription
/// while the view is detached from the visual tree.
/// </summary>
public abstract class LocalizedUserControl : UserControl
{
    protected LocalizedUserControl()
    {
        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string lang)
    {
        // Pass 1 — Normal priority: fires after all LocalizationChanged handlers complete.
        // LocalizeKeyObservable.Value is already updated at this point, so any binding
        // that is still alive will pick up the new value immediately.
        Dispatcher.UIThread.Post(() =>
        {
            Debug.WriteLine($"[{GetType().Name}] Failsafe Normal pass lang={lang}");
            // Force Avalonia to re-evaluate all bindings by invalidating the visual.
            InvalidateVisual();
        }, DispatcherPriority.Normal);

        // Pass 2 — Background priority: nuclear option. Re-runs InitializeComponent so
        // the entire visual tree is rebuilt with fresh {Localize} bindings that read the
        // current language from scratch. Handles the case where the binding subscription
        // was silently dropped.
        Dispatcher.UIThread.Post(() =>
        {
            Debug.WriteLine($"[{GetType().Name}] Failsafe Background pass lang={lang}");
            ReinitializeComponent();
        }, DispatcherPriority.Background);
    }

    /// <summary>
    /// Override in derived classes to call <c>InitializeComponent()</c>.
    /// The base implementation attempts to call an instance method named
    /// <c>InitializeComponent()</c> via reflection.
    /// </summary>
    protected virtual void ReinitializeComponent()
    {
        try
        {
            MethodInfo? init = GetType().GetMethod(
                "InitializeComponent",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);

            init?.Invoke(this, null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[{GetType().Name}] ReinitializeComponent failed: {ex.Message}");
        }
    }
}
