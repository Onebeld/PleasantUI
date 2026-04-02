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
        var currentLang = Localizer.Instance.CurrentLanguage;
        Debug.WriteLine($"[{GetType().Name}] Constructor START - CurrentLanguage={currentLang}");
        
        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
        
        // CRITICAL: If this view is being constructed AFTER a language change has already
        // completed (e.g., user navigates to this page after changing language), we missed
        // the LocalizationChanged event. Schedule MULTIPLE reinits at different priorities
        // to ensure we pick up the current language.
        if (!string.IsNullOrEmpty(currentLang) && currentLang != "en")
        {
            Debug.WriteLine($"[{GetType().Name}] Constructor - CurrentLanguage={currentLang}, scheduling TRIPLE reinit");
            
            // Pass 1: Loaded priority - fires when the control is added to visual tree
            Dispatcher.UIThread.Post(() =>
            {
                Debug.WriteLine($"[{GetType().Name}] Loaded-priority reinit for lang={currentLang}");
                ReinitializeComponent();
            }, DispatcherPriority.Loaded);
            
            // Pass 2: Render priority - fires after layout pass
            Dispatcher.UIThread.Post(() =>
            {
                Debug.WriteLine($"[{GetType().Name}] Render-priority reinit for lang={currentLang}");
                ReinitializeComponent();
            }, DispatcherPriority.Render);
            
            // Pass 3: Background priority - nuclear option, fires last
            Dispatcher.UIThread.Post(() =>
            {
                Debug.WriteLine($"[{GetType().Name}] Background-priority reinit for lang={currentLang}");
                ReinitializeComponent();
            }, DispatcherPriority.Background);
        }
        
        Debug.WriteLine($"[{GetType().Name}] Constructor END");
    }

    private void OnLanguageChanged(string lang)
    {
        Debug.WriteLine($"[LocalizedUserControl.{GetType().Name}] OnLanguageChanged lang={lang} CurrentLanguage={Localizer.Instance.CurrentLanguage}");
        
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
            Debug.WriteLine($"[{GetType().Name}] Failsafe Background pass lang={lang} - calling ReinitializeComponent");
            ReinitializeComponent();
            Debug.WriteLine($"[{GetType().Name}] ReinitializeComponent done");
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
            Debug.WriteLine($"[{GetType().Name}] ReinitializeComponent - before InitializeComponent, CurrentLanguage={Localizer.Instance.CurrentLanguage}");
            
            // CRITICAL: Clear Content AND DataContext to force Avalonia to completely
            // rebuild the visual tree instead of reusing cached XAML parse results.
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
            
            // Restore DataContext if it was set
            if (oldDataContext is not null && DataContext is null)
                DataContext = oldDataContext;
            
            Debug.WriteLine($"[{GetType().Name}] ReinitializeComponent - after InitializeComponent");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[{GetType().Name}] ReinitializeComponent failed: {ex.Message}");
        }
    }
}
