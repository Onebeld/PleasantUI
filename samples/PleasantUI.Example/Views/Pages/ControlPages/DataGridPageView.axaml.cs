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

        if (DataContext is DataGridViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(DataGridViewModel.ShowGridLines):
                        MainDataGrid.GridLinesVisibility = vm.ShowGridLines
                            ? DataGridGridLinesVisibility.All
                            : DataGridGridLinesVisibility.None;
                        break;
                    case nameof(DataGridViewModel.ShowRowDetails):
                        MainDataGrid.RowDetailsVisibilityMode = vm.ShowRowDetails
                            ? DataGridRowDetailsVisibilityMode.VisibleWhenSelected
                            : DataGridRowDetailsVisibilityMode.Collapsed;
                        break;
                }
            };
        }
    }
}
