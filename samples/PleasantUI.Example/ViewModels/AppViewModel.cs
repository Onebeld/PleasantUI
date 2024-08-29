using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using PleasantUI.Example.Helpers;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Models;
using PleasantUI.Example.Pages;

namespace PleasantUI.Example.ViewModels;

public class AppViewModel : ViewModelBase
{
    private IPage _page = null!;
    private bool _isForwardAnimation = true;

    private AvaloniaList<ControlPageCard> _controlPageCards;
    
    public IManagedNotificationManager? NotificationManager { get; set; }
    
    /// <summary>
    /// The current page
    /// </summary>
    public IPage Page
    {
        get => _page;
        set => RaiseAndSet(ref _page, value);
    }

    /// <summary>
    /// Indicates whether the animation should be forward or backward
    /// </summary>
    public bool IsForwardAnimation
    {
        get => _isForwardAnimation;
        private set => RaiseAndSet(ref _isForwardAnimation, value);
    }
    
    public AvaloniaList<ControlPageCard> ControlPageCards
    {
        get => _controlPageCards;
        set => RaiseAndSet(ref _controlPageCards, value);
    }

    public AppViewModel()
    {
        ControlPageCards = [
            new ControlPageCard("Button", MaterialIcons.ButtonCursor, "", Command.Create(() => ChangePage(new ButtonPage()))),
            new ControlPageCard("Checkbox", MaterialIcons.CheckboxMarkedOutline, "", Command.Create(() => ChangePage(new CheckBoxPage()))),
            new ControlPageCard("ProgressRing", MaterialIcons.ProgressHelper, "", Command.Create(() => ChangePage(new ProgressRingPage()))),
        ];
        
        Page = new HomePage();
    }

    /// <summary>
    /// Changes the current page
    /// </summary>
    /// <param name="page">The new page</param>
    /// <param name="forward">The direction of the animation</param>
    public void ChangePage(IPage page, bool forward = true)
    {
        IsForwardAnimation = forward;
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