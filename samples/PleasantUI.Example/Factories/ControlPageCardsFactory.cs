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
            new("CardTitle/Button",       MaterialIcons.ButtonCursor,        "Card/Button",    new ButtonPage(),   _eventAggregator),
            new("CardTitle/Checkbox",     MaterialIcons.CheckboxMarkedOutline,"Card/Checkbox",  new CheckBoxPage(), _eventAggregator),
            new("CardTitle/Progress",     MaterialIcons.ProgressHelper,       "Card/Progress",  new ProgressPage(), _eventAggregator),
            new("CardTitle/Calendar",     MaterialIcons.CalendarOutline,      "Card/Calendar",  new CalendarPage(), _eventAggregator),
            new("CardTitle/Carousel",     MaterialIcons.ViewCarouselOutline,  "Card/Carousel",  new CarouselPage(), _eventAggregator),
            new("CardTitle/ComboBox",     MaterialIcons.ExpandAllOutline,     "Card/ComboBox",  new ComboBoxPage(), _eventAggregator),
            new("CardTitle/TextBox",      MaterialIcons.FormTextbox,          "Card/TextBox",   new TextBoxPage(),  _eventAggregator),
            new("CardTitle/DataGrid",     MaterialIcons.Grid,                 "Card/DataGrid",  new DataGridPage(), _eventAggregator),
            new("CardTitle/PinCode",      MaterialIcons.KeyboardOutline,       "Card/PinCode",   new PinCodePage(),  _eventAggregator),
        ];
    }

    public AvaloniaList<ControlPageCard> CreatePleasantControlPageCards()
    {
        return
        [
            new("CardTitle/PleasantSnackbar",    MaterialIcons.InformationOutline,    "Card/PleasantSnackbar",    new PleasantSnackbarPage(),    _eventAggregator),
            new("CardTitle/InformationBlock",    MaterialIcons.InformationBoxOutline, "Card/InformationBlock",    new InformationBlockPage(),    _eventAggregator),
            new("CardTitle/OptionsDisplayItem",  MaterialIcons.ViewListOutline,       "Card/OptionsDisplayItem",  new OptionsDisplayItemPage(),  _eventAggregator),
            new("CardTitle/PleasantTabView",     MaterialIcons.Tab,                   "Card/PleasantTabView",     new PleasantTabViewPage(),     _eventAggregator),
            new("CardTitle/PleasantMenu",        MaterialIcons.MenuOpen,              "Card/PleasantMenu",        new PleasantMenuPage(),        _eventAggregator),
        ];
    }

    public AvaloniaList<ControlPageCard> CreateToolkitControlPageCards()
    {
        return
        [
            new("CardTitle/MessageBox", MaterialIcons.MessageOutline, "Card/MessageBox", new MessageBoxPage(), _eventAggregator),
        ];
    }
}