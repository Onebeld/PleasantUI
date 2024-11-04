using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class DataGridPage : IPage
{
    public string Title { get; } = "DataGrid";
    public bool ShowTitle { get; } = true;

    public Control Content => new DataGridPageView();
}