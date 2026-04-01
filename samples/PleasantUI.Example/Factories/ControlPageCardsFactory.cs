using Avalonia.Collections;
using PleasantUI.Example.Models;
using PleasantUI.Example.Pages.BasicControls;
using PleasantUI.Example.Pages.PleasantControls;
using PleasantUI.Example.Pages.Toolkit;
using PleasantUI.ToolKit.Services.Interfaces;

namespace PleasantUI.Example.Factories;

public class ControlPageCardsFactory
{
    private readonly IEventAggregator _eventAggregator;
    
    public ControlPageCardsFactory(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }
    
    public AvaloniaList<ControlPageCard> CreateBasicControlPageCards()
    {
        return
        [
            new("Button",       MaterialIcons.ButtonCursor,        "Card/Button",    new ButtonPage(),   _eventAggregator),
            new("Checkbox",     MaterialIcons.CheckboxMarkedOutline,"Card/Checkbox",  new CheckBoxPage(), _eventAggregator),
            new("Progress",     MaterialIcons.ProgressHelper,       "Card/Progress",  new ProgressPage(), _eventAggregator),
            new("Calendar",     MaterialIcons.CalendarOutline,      "Card/Calendar",  new CalendarPage(), _eventAggregator),
            new("Carousel",     MaterialIcons.ViewCarouselOutline,  "Card/Carousel",  new CarouselPage(), _eventAggregator),
            new("ComboBox",     MaterialIcons.ExpandAllOutline,     "Card/ComboBox",  new ComboBoxPage(), _eventAggregator),
            new("TextBox",      MaterialIcons.FormTextbox,          "Card/TextBox",   new TextBoxPage(),  _eventAggregator),
            new("DataGrid",     MaterialIcons.Grid,                 "Card/DataGrid",  new DataGridPage(), _eventAggregator),
        ];
    }

    public AvaloniaList<ControlPageCard> CreatePleasantControlPageCards()
    {
        return
        [
            new("PleasantSnackbar",    MaterialIcons.InformationOutline,    "Card/PleasantSnackbar",    new PleasantSnackbarPage(),    _eventAggregator),
            new("InformationBlock",    MaterialIcons.InformationBoxOutline, "Card/InformationBlock",    new InformationBlockPage(),    _eventAggregator),
            new("OptionsDisplayItem",  MaterialIcons.ViewListOutline,       "Card/OptionsDisplayItem",  new OptionsDisplayItemPage(),  _eventAggregator),
            new("PleasantTabView",     MaterialIcons.Tab,                   "Card/PleasantTabView",     new PleasantTabViewPage(),     _eventAggregator),
        ];
    }

    public AvaloniaList<ControlPageCard> CreateToolkitControlPageCards()
    {
        return
        [
            new("MessageBox", MaterialIcons.MessageOutline, "Card/MessageBox", new MessageBoxPage(), _eventAggregator),
        ];
    }
}