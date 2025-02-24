using Avalonia.Collections;
using PleasantUI.Example.Models;
using PleasantUI.Example.Pages.BasicControls;
using PleasantUI.Example.Pages.PleasantControls;
using PleasantUI.Example.Pages.Toolkit;

namespace PleasantUI.Example.Fabrics;

public class ControlPageCardsFactory
{
    public AvaloniaList<ControlPageCard> CreateBasicControlPageCards()
    {
        return new AvaloniaList<ControlPageCard>
        {
            new("Button", MaterialIcons.ButtonCursor, "A clickable element that triggers an action when pressed.", new ButtonPage()),
            new("Checkbox", MaterialIcons.CheckboxMarkedOutline, "A small interactive box that allows users to select one or more options from a set.", new CheckBoxPage()),
            new("Progress", MaterialIcons.ProgressHelper, "A circular animation that indicates an ongoing operation or loading process.", new ProgressPage()),
            new("Calendar", MaterialIcons.CalendarOutline, "A control that displays dates in a structured format, allowing users to select a specific date or date range.", new CalendarPage()),
            new("Carousel", MaterialIcons.ViewCarouselOutline, "A container that displays a set of items (images, text, etc.) one at a time, allowing users to navigate through them sequentially.", new CarouselPage()),
            new("ComboBox", MaterialIcons.ExpandAllOutline, "A dropdown list that allows users to select a single option from a predefined list. It combines a text box with a dropdown menu.", new ComboBoxPage()),
            new("TextBox", MaterialIcons.FormTextbox, "A rectangular area where users can enter and edit text.", new TextBoxPage()),
            new("DataGrid", MaterialIcons.Grid, "A grid that displays data in a tabular format.", new DataGridPage())
        };
    }

    public AvaloniaList<ControlPageCard> CreatePleasantControlPageCards()
    {
        return new AvaloniaList<ControlPageCard>
        {
            new("PleasantSnackbar", MaterialIcons.InformationOutline, "Custom control for displaying temporary, non-intrusive messages to the user.", new PleasantSnackbarPage()),
            new("InformationBlock", MaterialIcons.InformationBoxOutline, "Custom control for displaying a structured block of information, potentially including a title, icon, and descriptive text.", new InformationBlockPage()),
            new("OptionsDisplayItem", MaterialIcons.ViewListOutline, "Custom control representing a single item in a list of options.", new OptionsDisplayItemPage()),
            new("PleasantTabView", MaterialIcons.Tab, "Custom control implementing a tabbed interface.", new PleasantTabViewPage())
        };
    }

    public AvaloniaList<ControlPageCard> CreateToolkitControlPageCards()
    {
        return new AvaloniaList<ControlPageCard>
        {
            new("MessageBox", MaterialIcons.MessageOutline, "", new MessageBoxPage())
        };
    }
}