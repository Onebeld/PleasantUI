using Avalonia.Collections;
using Avalonia.Threading;
using PleasantUI.Core;
using PleasantUI.Core.Localization;
using PleasantUI.Example.Factories;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Messages;
using PleasantUI.Example.Models;
using PleasantUI.Example.Pages.BasicControls;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.Example.ViewModels;

public class AppViewModel : ViewModelBase
{
    private readonly ControlPageCardsFactory _factory;

    // Localized header strings — updated directly on language change so
    // {CompiledBinding} in HomePageView always gets the correct value.
    private string _welcomeText = string.Empty;

    public AvaloniaList<ControlPageCard> BasicControlPageCards { get; } = [];
    public AvaloniaList<ControlPageCard> PleasantControlPageCards { get; } = [];
    public AvaloniaList<ControlPageCard> ToolKitPageCards { get; } = [];

    public IPage Page
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsForwardAnimation
    {
        get;
        set => SetProperty(ref field, value);
    } = true;
    
    public bool IsEnabledAnimation
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public string WelcomeText
    {
        get => _welcomeText;
        private set => SetProperty(ref _welcomeText, value);
    }

    public string BasicControlsText
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string PleasantControlsText
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public string ToolKitText
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    public AppViewModel(IEventAggregator eventAggregator)
    {
        _factory = new ControlPageCardsFactory(eventAggregator);

        BasicControlPageCards.AddRange(_factory.CreateBasicControlPageCards());
        PleasantControlPageCards.AddRange(_factory.CreatePleasantControlPageCards());
        ToolKitPageCards.AddRange(_factory.CreateToolkitControlPageCards());

        Page = new HomePage();

        eventAggregator.Subscribe<ChangePageMessage>(async message =>
        {
            ChangePage(message.Page);
            await Task.CompletedTask;
        });

        Localizer.Instance.LocalizationChanged += OnLanguageChanged;

        // Initialize text properties with current language
        RefreshLocalizedTexts();
    }

    private void OnLanguageChanged(string _)
    {
        Dispatcher.UIThread.Post(Rebuild, DispatcherPriority.Background);
    }

    private void Rebuild()
    {
        // Refresh header texts first — these are bound via CompiledBinding so they
        // update instantly without any view recreation needed.
        RefreshLocalizedTexts();

        // Rebuild card collections with fresh instances that read the new language
        List<ControlPageCard> newBasic    = _factory.CreateBasicControlPageCards().ToList();
        List<ControlPageCard> newPleasant = _factory.CreatePleasantControlPageCards().ToList();
        List<ControlPageCard> newToolkit  = _factory.CreateToolkitControlPageCards().ToList();

        BasicControlPageCards.Clear();
        PleasantControlPageCards.Clear();
        ToolKitPageCards.Clear();

        BasicControlPageCards.AddRange(newBasic);
        PleasantControlPageCards.AddRange(newPleasant);
        ToolKitPageCards.AddRange(newToolkit);
    }

    private void RefreshLocalizedTexts()
    {
        WelcomeText          = Localizer.Tr("WelcomeToPleasantUI");
        BasicControlsText    = Localizer.Tr("BasicControls");
        PleasantControlsText = Localizer.Tr("PleasantControls");
        ToolKitText          = Localizer.Tr("ToolKit");
    }

    /// <summary>
    /// Public entry point so views can force a re-push of all localized text properties
    /// as a failsafe when their own LocalizationChanged subscription fires.
    /// </summary>
    public void ForceRefreshLocalizedTexts() => RefreshLocalizedTexts();

    public void ChangePage(IPage page, bool enableAnimation = true)
    {
        IsEnabledAnimation = enableAnimation;
        IsForwardAnimation = true;
        Page = page;
    }

    public void BackToHomePage(bool enableAnimation = true)
    {
        if (Page is HomePage)
            return;
        
        IsEnabledAnimation = enableAnimation;
        IsForwardAnimation = false;
        Page = new HomePage();
    }
    
    public void BackToHomePageXaml()
    {
        IsEnabledAnimation = true;
        IsForwardAnimation = false;
        Page = new HomePage();
    }
}
