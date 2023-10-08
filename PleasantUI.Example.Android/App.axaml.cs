using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;
using PleasantUI.Example.ViewModels;
using Application = Avalonia.Application;

namespace PleasantUI.Example.Android;

public partial class App : Application
{
    public static PleasantTheme PleasantTheme { get; private set; } = null!;
    
    public static ApplicationViewModel ViewModel { get; private set; }
    
    public App()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = ViewModel = new ApplicationViewModel();
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        PleasantTheme = new PleasantTheme();
        Styles.Add(PleasantTheme);
        
        if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
        {
            lifetime.MainView = new PleasantMainView()
            {
                DataContext = ViewModel
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}