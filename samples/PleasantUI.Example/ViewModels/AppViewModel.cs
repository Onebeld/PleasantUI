using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Models;
using PleasantUI.Example.Pages.BasicControls;
using PleasantUI.Example.Pages.PleasantControls;
using PleasantUI.Example.Pages.Toolkit;

namespace PleasantUI.Example.ViewModels;

public class AppViewModel : ObservableObject
{
    private IPage _page = null!;
    private bool _isForwardAnimation = true;

    private AvaloniaList<ControlPageCard> _basicControlPageCards;
    private AvaloniaList<ControlPageCard> _pleasantControlPageCards;
    private AvaloniaList<ControlPageCard> _toolKitPageCards;
    
    /// <summary>
    /// The current page
    /// </summary>
    public IPage Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    /// <summary>
    /// Indicates whether the animation should be forward or backward
    /// </summary>
    public bool IsForwardAnimation
    {
        get => _isForwardAnimation;
        private set => SetProperty(ref _isForwardAnimation, value);
    }
    
    public AvaloniaList<ControlPageCard> BasicControlPageCards
    {
        get => _basicControlPageCards;
        set => SetProperty(ref _basicControlPageCards, value);
    }
    
    public AvaloniaList<ControlPageCard> PleasantControlPageCards
    {
        get => _pleasantControlPageCards;
        set => SetProperty(ref _pleasantControlPageCards, value);
    }
    
    public AvaloniaList<ControlPageCard> ToolKitPageCards
    {
        get => _toolKitPageCards;
        set => SetProperty(ref _toolKitPageCards, value);
    }

    public AppViewModel()
    {
        BasicControlPageCards = [
            new ControlPageCard("Button", MaterialIcons.ButtonCursor, "A clickable element that triggers an action when pressed.", new RelayCommand(() => ChangePage(new ButtonPage()))),
            new ControlPageCard("Checkbox", MaterialIcons.CheckboxMarkedOutline, "A small interactive box that allows users to select one or more options from a set.", new RelayCommand(() => ChangePage(new CheckBoxPage()))),
            new ControlPageCard("Progress", MaterialIcons.ProgressHelper, "A circular animation that indicates an ongoing operation or loading process.", new RelayCommand(() => ChangePage(new ProgressPage()))),
            new ControlPageCard("Calendar", MaterialIcons.CalendarOutline, "A control that displays dates in a structured format, allowing users to select a specific date or date range.", new RelayCommand(() => ChangePage(new CalendarPage()))),
            new ControlPageCard("Carousel", MaterialIcons.ViewCarouselOutline, "A container that displays a set of items (images, text, etc.) one at a time, allowing users to navigate through them sequentially.", new RelayCommand(() => ChangePage(new CarouselPage()))),
            new ControlPageCard("ComboBox", MaterialIcons.ExpandAllOutline, "A dropdown list that allows users to select a single option from a predefined list. It combines a text box with a dropdown menu.", new RelayCommand(() => ChangePage(new ComboBoxPage()))),
            new ControlPageCard("TextBox", MaterialIcons.FormTextbox, "A rectangular area where users can enter and edit text.", new RelayCommand(() => ChangePage(new TextBoxPage())))
        ];

        PleasantControlPageCards =
        [
            new ControlPageCard("PleasantSnackbar", MaterialIcons.InformationOutline, "Custom control for displaying temporary, non-intrusive messages to the user.", new RelayCommand(() => ChangePage(new PleasantSnackbarPage()))),
            new ControlPageCard("InformationBlock", MaterialIcons.InformationBoxOutline, "Custom control for displaying a structured block of information, potentially including a title, icon, and descriptive text.", new RelayCommand(() => ChangePage(new InformationBlockPage()))),
            new ControlPageCard("OptionsDisplayItem", MaterialIcons.ViewListOutline, "Custom control representing a single item in a list of options.", new RelayCommand(() => ChangePage(new OptionsDisplayItemPage()))),
            new ControlPageCard("PleasantTabView", MaterialIcons.Tab, "Custom control implementing a tabbed interface.", new RelayCommand(() => ChangePage(new PleasantTabViewPage())))
        ];

        ToolKitPageCards =
        [
            new ControlPageCard("MessageBox", MaterialIcons.MessageOutline, "", new RelayCommand(() => ChangePage(new MessageBoxPage())))
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