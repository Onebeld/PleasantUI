using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class CalendarPage : IPage
{
    public string Title { get; } = "Calendar";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = new CalendarPageView();
}