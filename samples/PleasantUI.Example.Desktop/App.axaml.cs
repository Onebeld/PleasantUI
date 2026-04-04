using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;

namespace PleasantUI.Example.Desktop;

public partial class App : PleasantUiExampleApp
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        
        if (Design.IsDesignMode)
        {
            desktop.MainWindow = new PleasantWindow();
            
            base.OnFrameworkInitializationCompleted();
            return;
        }
        
        PleasantTheme = Styles[0] as PleasantTheme ?? throw new NullReferenceException("PleasantTheme is null");

        // PleasantTheme constructor has now loaded PleasantSettings from disk.
        // Apply persisted language and construct the AppViewModel with the correct language.
        InitializeFromSettings();
        
        Main = new MainWindow
        {
            DataContext = ViewModel
        };

        TopLevel = TopLevel.GetTopLevel(Main as PleasantWindow) ?? throw new NullReferenceException("TopLevel is null");
            
        desktop.MainWindow = Main as PleasantWindow;
        
        base.OnFrameworkInitializationCompleted();
    }
}