using Avalonia.Controls;
using PleasantUI.Controls;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class PleasantSnackbarPageView : UserControl
{
    public PleasantSnackbarPageView()
    {
        InitializeComponent();

        SnackbarButton.Click += (sender, args) =>
        {
            PleasantSnackbar.Show(PleasantUiExampleApp.Main, new PleasantSnackbarOptions("Привет, мир!")
            {
                Icon = MaterialIcons.Account
            });
        };
    }
}