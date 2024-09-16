using Avalonia.Controls;
using Avalonia.Interactivity;
using PleasantUI.Controls;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ButtonPageView : UserControl
{
    public ButtonPageView()
    {
        InitializeComponent();
        
        SnackbarButton.Click += SnackbarButtonOnClick;
    }

    private void SnackbarButtonOnClick(object? sender, RoutedEventArgs e)
    {
        PleasantSnackbar.Show(PleasantUiExampleApp.Main, "This is so long message from PleasantSnackbar, this is cool and beautiful! Oh yes, omg, yes, yes, yes!!!");
    }
}