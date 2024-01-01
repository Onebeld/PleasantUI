using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;
using PleasantUI.Example.ViewModels;

namespace PleasantUI.Example;

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
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = ViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    public static string GetString(string key)
    {
        if (Current!.TryFindResource(key, out object? objectText))
            return objectText as string ?? string.Empty;
        return key;
    }
}