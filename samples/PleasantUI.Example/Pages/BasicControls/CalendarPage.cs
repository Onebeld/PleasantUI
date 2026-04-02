using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class CalendarPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/Calendar";
    public override bool ShowTitle { get; } = true;
    protected override Control CreateContent() => new CalendarPageView();
}
