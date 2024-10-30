using Avalonia.Controls;

namespace PleasantUI.Example.Views.Pages;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        DataContext = PleasantUiExampleApp.ViewModel;
        
        InitializeComponent();
    }
}