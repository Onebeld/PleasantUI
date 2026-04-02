using Avalonia.Collections;
using PleasantUI.Core;
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
        
    /// <summary>
    /// The current page
    /// </summary>
    private IPage _page = null!;
    
    /// <summary>
    /// Indicates whether the animation should be forward or backward
    /// </summary>
    private bool _isForwardAnimation = true;

    public AvaloniaList<ControlPageCard> BasicControlPageCards { get; }
    
    public AvaloniaList<ControlPageCard> PleasantControlPageCards { get; }
    
    public AvaloniaList<ControlPageCard> ToolKitPageCards { get; }

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
            
        ControlPageCardsFactory factory = new(eventAggregator);
        
        BasicControlPageCards = factory.CreateBasicControlPageCards();
        PleasantControlPageCards = factory.CreatePleasantControlPageCards();
        ToolKitPageCards = factory.CreateToolkitControlPageCards();
        
        Page = new HomePage();

        _eventAggregator.Subscribe<ChangePageMessage>(async message =>
        {
            ChangePage(message.Page);
            await Task.CompletedTask;
        });
    }

    /// <summary>
    /// Changes the current page
    /// </summary>
    /// <param name="page">The new page</param>
    /// <param name="forward">The direction of the animation</param>
    public void ChangePage(IPage page)
    {
        IsForwardAnimation = true;
        Page = page;
    }

    /// <summary>
    /// Goes back to the home page
    /// </summary>
    public void BackToHomePage()
    {
        IsForwardAnimation = false;
        Page = new HomePage();
    }
}