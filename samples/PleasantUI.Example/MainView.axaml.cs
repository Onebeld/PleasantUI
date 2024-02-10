using Avalonia.Controls;
using PleasantUI.Example.Views;

namespace PleasantUI.Example;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        
        HomeView.FuncControl += () => new HomeView { DataContext = DataContext };
        SettingsView.FuncControl += () => new SettingsView();
        AboutView.FuncControl += () => new AboutView();
    }
}