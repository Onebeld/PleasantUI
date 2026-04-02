using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class CalendarPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Calendar";
    public override bool ShowTitle { get; } = true;
    public override Control Content { get; } = new CalendarPageView();
}
