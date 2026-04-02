using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class DataGridPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/DataGrid";
    public override bool ShowTitle { get; } = true;
    public override Control Content => new DataGridPageView();
}
