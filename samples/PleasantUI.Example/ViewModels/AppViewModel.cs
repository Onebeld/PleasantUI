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
    private readonly IEventAggregator _eventAggregator;
    private readonly ControlPageCardsFactory _factory;

    private IPage _page = null!;
    private bool _isForwardAnimation = true;

    // Localized header strings — updated directly on language change so
    // {CompiledBinding} in HomePageView always gets the correct value.
    private string _welcomeText = string.Empty;
    private string _basicControlsText = string.Empty;
    private string _pleasantControlsText = string.Empty;
    private string _toolKitText = string.Empty;

    public AvaloniaList<ControlPageCard> BasicControlPageCards { get; } = [];
    public AvaloniaList<ControlPageCard> PleasantControlPageCards { get; } = [];
    public AvaloniaList<ControlPageCard> ToolKitPageCards { get; } = [];

    public IPage Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    public bool IsForwardAnimation
    {
        get => _isForwardAnimation;
        set => SetProperty(ref _isForwardAnimation, value);
    }

    public string WelcomeText
    {
        get => _welcomeText;
        private set => SetProperty(ref _welcomeText, value);
    }

    public string BasicControlsText
    {
        get => _basicControlsText;
        private set => SetProperty(ref _basicControlsText, value);
    }

    public string PleasantControlsText
    {
        get => _pleasantControlsText;
        private set => SetProperty(ref _pleasantControlsText, value);
    }

    public string ToolKitText
    {
        get => _toolKitText;
        private set => SetProperty(ref _toolKitText, value);
    }

    public AppViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _factory = new ControlPageCardsFactory(eventAggregator);

        BasicControlPageCards.AddRange(_factory.CreateBasicControlPageCards());
        PleasantControlPageCards.AddRange(_factory.CreatePleasantControlPageCards());
        ToolKitPageCards.AddRange(_factory.CreateToolkitControlPageCards());

        Page = new HomePage();

        _eventAggregator.Subscribe<ChangePageMessage>(async message =>
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
        System.Diagnostics.Debug.WriteLine($"[AppViewModel] Rebuild lang={Localizer.Instance.CurrentLanguage}");

        // Refresh header texts first — these are bound via CompiledBinding so they
        // update instantly without any view recreation needed.
        RefreshLocalizedTexts();

        // Rebuild card collections with fresh instances that read the new language
        var newBasic    = _factory.CreateBasicControlPageCards().ToList();
        var newPleasant = _factory.CreatePleasantControlPageCards().ToList();
        var newToolkit  = _factory.CreateToolkitControlPageCards().ToList();

        BasicControlPageCards.Clear();
        PleasantControlPageCards.Clear();
        ToolKitPageCards.Clear();

        BasicControlPageCards.AddRange(newBasic);
        PleasantControlPageCards.AddRange(newPleasant);
        ToolKitPageCards.AddRange(newToolkit);

        System.Diagnostics.Debug.WriteLine($"[AppViewModel] Rebuild done, welcome=\"{_welcomeText}\" first card=\"{newBasic.FirstOrDefault()?.Title}\"");
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

    public void ChangePage(IPage page)
    {
        IsForwardAnimation = true;
        Page = page;
    }

    public void BackToHomePage()
    {
        IsForwardAnimation = false;
        Page = new HomePage();
    }
}
