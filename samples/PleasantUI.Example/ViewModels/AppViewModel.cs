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
            new ControlPageCard("Button", MaterialIcons.ButtonCursor, "A clickable element that triggers an action when pressed.", Command.Create(() => ChangePage(new ButtonPage()))),
            new ControlPageCard("Checkbox", MaterialIcons.CheckboxMarkedOutline, "A small interactive box that allows users to select one or more options from a set.", Command.Create(() => ChangePage(new CheckBoxPage()))),
            new ControlPageCard("Progress", MaterialIcons.ProgressHelper, "A circular animation that indicates an ongoing operation or loading process.", Command.Create(() => ChangePage(new ProgressRingPage()))),
            new ControlPageCard("Calendar", MaterialIcons.CalendarOutline, "A control that displays dates in a structured format, allowing users to select a specific date or date range.", Command.Create(() => ChangePage(new CalendarPage()))),
            new ControlPageCard("Carousel", MaterialIcons.ViewCarouselOutline, "A container that displays a set of items (images, text, etc.) one at a time, allowing users to navigate through them sequentially.", Command.Create(() => ChangePage(new CarouselPage()))),
            new ControlPageCard("ComboBox", MaterialIcons.ExpandAllOutline, "A dropdown list that allows users to select a single option from a predefined list. It combines a text box with a dropdown menu.", Command.Create(() => ChangePage(new ComboBoxPage()))),
            new ControlPageCard("TextBox", MaterialIcons.FormTextbox, "A rectangular area where users can enter and edit text.", Command.Create(() => ChangePage(new TextBoxPage())))
            
        ];

        PleasantControlPageCards =
        [
            new ControlPageCard("PleasantSnackbar", MaterialIcons.InformationOutline, "Custom control for displaying temporary, non-intrusive messages to the user.", Command.Create(() => ChangePage(new PleasantSnackbarPage()))),
            new ControlPageCard("InformationBlock", MaterialIcons.InformationBoxOutline, "Custom control for displaying a structured block of information, potentially including a title, icon, and descriptive text.", Command.Create(() => ChangePage(new InformationBlockPage()))),
            new ControlPageCard("OptionsDisplayItem", MaterialIcons.ViewListOutline, "Custom control representing a single item in a list of options.", Command.Create(() => ChangePage(new OptionsDisplayItemPage()))),
            new ControlPageCard("PleasantTabView", MaterialIcons.Tab, "Custom control implementing a tabbed interface.", Command.Create(() => ChangePage(new PleasantTabViewPage())))
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