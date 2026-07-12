using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Example.ViewModels.Pages.ControlPages;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class TableViewPageView : LocalizedUserControl
{
    public TableViewPageView()
    {
        InitializeComponent();
        DataContext = new TableViewPageViewModel();
    }
}