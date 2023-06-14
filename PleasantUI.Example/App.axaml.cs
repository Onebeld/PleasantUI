using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PleasantUI.Example.ViewModels;

namespace PleasantUI.Example;

public partial class App : Application
{
    public PleasantTheme PleasantTheme { get; private set; } = null!;
    
    public App()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new ApplicationViewModel();
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        PleasantTheme = new PleasantTheme();
        Styles.Add(PleasantTheme);
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = DataContext
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