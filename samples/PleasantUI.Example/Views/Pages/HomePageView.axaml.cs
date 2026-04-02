using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using PleasantUI.Core.Localization;
using PleasantUI.Example.ViewModels;

namespace PleasantUI.Example.Views.Pages;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        Debug.WriteLine("[HomePageView] Created");
        DataContext = PleasantUiExampleApp.ViewModel;
        InitializeComponent();

        // Primary update path: CompiledBinding to AppViewModel properties (WelcomeText,
        // BasicControlsText, etc.) which are updated by AppViewModel.Rebuild() via SetProperty.
        //
        // Failsafe: also subscribe here so that even if the VM's Rebuild() post races or
        // the binding somehow misses the PropertyChanged, we force the VM to re-push all
        // localized texts and also re-initialize this view's visual tree.
        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string lang)
    {
        // Post at two different priorities:
        // 1. Normal — re-push VM localized text properties immediately after all handlers fire
        // 2. Background — re-initialize the visual tree as a last-resort failsafe
        Dispatcher.UIThread.Post(() =>
        {
            Debug.WriteLine($"[HomePageView] Failsafe Normal pass for lang={lang}");
            if (DataContext is AppViewModel vm)
                vm.ForceRefreshLocalizedTexts();
        }, DispatcherPriority.Normal);

        Dispatcher.UIThread.Post(() =>
        {
            Debug.WriteLine($"[HomePageView] Failsafe Background pass for lang={lang}");
            // Belt-and-suspenders: re-initialize the entire visual tree so any
            // {Localize} or {CompiledBinding} that missed the update gets a fresh read.
            InitializeComponent();
        }, DispatcherPriority.Background);
    }
}
