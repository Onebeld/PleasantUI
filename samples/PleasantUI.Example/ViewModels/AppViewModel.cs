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

    // Backing stores so we can rebuild on language change
    private readonly List<ControlPageCard> _basicCards;
    private readonly List<ControlPageCard> _pleasantCards;
    private readonly List<ControlPageCard> _toolkitCards;

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

    public AppViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _factory = new ControlPageCardsFactory(eventAggregator);

        _basicCards   = [.. _factory.CreateBasicControlPageCards()];
        _pleasantCards = [.. _factory.CreatePleasantControlPageCards()];
        _toolkitCards  = [.. _factory.CreateToolkitControlPageCards()];

        BasicControlPageCards.AddRange(_basicCards);
        PleasantControlPageCards.AddRange(_pleasantCards);
        ToolKitPageCards.AddRange(_toolkitCards);

        Page = new HomePage();

        _eventAggregator.Subscribe<ChangePageMessage>(async message =>
        {
            ChangePage(message.Page);
            await Task.CompletedTask;
        });

        // On language change: rebuild all card lists so Avalonia tears down and
        // recreates every item container, picking up the freshly translated strings.
        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string _)
    {
        // Always post with Background priority so this runs AFTER all LocalizationChanged
        // handlers have fired and all resource managers have been primed with the new language.
        // Running inline (even on UI thread) means new card instances still read stale values.
        Dispatcher.UIThread.Post(Rebuild, DispatcherPriority.Background);
    }

    private void Rebuild()
    {
        System.Diagnostics.Debug.WriteLine($"[AppViewModel] Rebuild cards, lang={Localizer.Instance.CurrentLanguage}");

        var newBasic    = _factory.CreateBasicControlPageCards().ToList();
        var newPleasant = _factory.CreatePleasantControlPageCards().ToList();
        var newToolkit  = _factory.CreateToolkitControlPageCards().ToList();

        BasicControlPageCards.Clear();
        PleasantControlPageCards.Clear();
        ToolKitPageCards.Clear();

        BasicControlPageCards.AddRange(newBasic);
        PleasantControlPageCards.AddRange(newPleasant);
        ToolKitPageCards.AddRange(newToolkit);

        // Force-recreate the current page so its view rebuilds with fresh LocalizeKeyObservable
        // bindings. HomePageView also handles this itself via its own LocalizationChanged handler,
        // but replacing the page ensures the DataTemplate re-applies cleanly.
        if (Page is HomePage)
        {
            System.Diagnostics.Debug.WriteLine($"[AppViewModel] Replacing HomePage with fresh instance");
            Page = new HomePage();
        }

        System.Diagnostics.Debug.WriteLine($"[AppViewModel] Rebuild done, first card title={newBasic.FirstOrDefault()?.Title}");
    }

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
