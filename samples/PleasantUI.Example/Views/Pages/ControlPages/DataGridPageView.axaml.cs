using Avalonia.Controls;
using Avalonia.Interactivity;
using PleasantUI.Example.ViewModels.Pages.ControlPages;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class DataGridPageView : UserControl
{
    public DataGridPageView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var grid = this.FindControl<DataGrid>("MainDataGrid");
        if (grid is null || DataContext is not DataGridViewModel vm) return;

        vm.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(DataGridViewModel.ShowGridLines):
                    grid.GridLinesVisibility = vm.ShowGridLines
                        ? DataGridGridLinesVisibility.All
                        : DataGridGridLinesVisibility.None;
                    break;
                case nameof(DataGridViewModel.ShowRowDetails):
                    grid.RowDetailsVisibilityMode = vm.ShowRowDetails
                        ? DataGridRowDetailsVisibilityMode.VisibleWhenSelected
                        : DataGridRowDetailsVisibilityMode.Collapsed;
                    break;
            }
        };
    }
}
