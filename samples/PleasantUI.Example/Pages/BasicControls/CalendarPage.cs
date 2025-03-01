using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages.BasicControls;

public class CalendarPage : IPage
{
    public string Title { get; } = "Calendar";
    public bool ShowTitle { get; } = true;
    public Control Content { get; } = null!;
}