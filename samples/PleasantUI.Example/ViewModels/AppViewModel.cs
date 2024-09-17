using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using PleasantUI.Controls;
using PleasantUI.Core.Helpers;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Models;
using PleasantUI.Example.Pages.BasicControls;
using PleasantUI.Example.Pages.PleasantControls;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.ViewModels;

public class AppViewModel : ViewModelBase
{
    private IPage _page = null!;
    private bool _isForwardAnimation = true;

    private AvaloniaList<ControlPageCard> _basicControlPageCards;
    private AvaloniaList<ControlPageCard> _pleasantControlPageCards;
    
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
    
    public AvaloniaList<ControlPageCard> BasicControlPageCards
    {
        get => _basicControlPageCards;
        set => RaiseAndSet(ref _basicControlPageCards, value);
    }
    
    public AvaloniaList<ControlPageCard> PleasantControlPageCards
    {
        get => _pleasantControlPageCards;
        set => RaiseAndSet(ref _pleasantControlPageCards, value);
    }

    public AppViewModel()
    {
        BasicControlPageCards = [
            new ControlPageCard("Button", MaterialIcons.ButtonCursor, "", Command.Create(() => ChangePage(new ButtonPage()))),
            new ControlPageCard("Checkbox", MaterialIcons.CheckboxMarkedOutline, "", Command.Create(() => ChangePage(new CheckBoxPage()))),
            new ControlPageCard("ProgressRing", MaterialIcons.ProgressHelper, "", Command.Create(() => ChangePage(new ProgressRingPage()))),
            new ControlPageCard("Calendar", MaterialIcons.CalendarOutline, "", Command.Create(() => ChangePage(new CalendarPage()))),
            new ControlPageCard("Carousel", MaterialIcons.ViewCarouselOutline, "", Command.Create(() => ChangePage(new CarouselPage()))),
            new ControlPageCard("ComboBox", null, "", Command.Create(() => ChangePage(new ComboBoxPage()))),
            new ControlPageCard("ProgressBar", null,  "", Command.Create(() => ChangePage(new ProgressBarPage()))),
            new ControlPageCard("TextBox", null, "", Command.Create(() => ChangePage(new TextBoxPage())))
            
        ];

        PleasantControlPageCards =
        [
            new ControlPageCard("PleasantSnackbar", null, "", Command.Create(() => ChangePage(new PleasantSnackbarPage()))),
            new ControlPageCard("InformationBlock", null, "", Command.Create(() => ChangePage(new InformationBlockPage()))),
            new ControlPageCard("OptionsDisplayItem", null, "", Command.Create(() => ChangePage(new OptionsDisplayItemPage()))),
            new ControlPageCard("PleasantTabView", MaterialIcons.Tab, "", Command.Create(() => ChangePage(new PleasantTabViewPage())))
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