using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        Debug.WriteLine("[HomePageView] Created");
        DataContext = PleasantUiExampleApp.ViewModel;
        InitializeComponent();

        // Subscribe to language changes so we can force a full content reload.
        // {Localize} bindings use LocalizeKeyObservable which updates its Value,
        // but Avalonia's reflection Binding can lose the PropertyChanged subscription
        // when the control is temporarily detached from the visual tree.
        // Reinitializing the component is the nuclear option that always works.
        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string lang)
    {
        // Post at Background priority so all LocalizeKeyObservable values have
        // already updated before we re-initialize.
        Dispatcher.UIThread.Post(() =>
        {
            Debug.WriteLine($"[HomePageView] Re-initializing for lang={lang}");
            // Re-run InitializeComponent to rebuild the entire visual tree with
            // fresh {Localize} bindings that read the current language immediately.
            InitializeComponent();
        }, DispatcherPriority.Background);
    }
}
