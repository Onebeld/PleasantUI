using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PleasantUI.Example.Views.Pages;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        DataContext = PleasantUiExampleApp.ViewModel;
        
        InitializeComponent();
    }
}