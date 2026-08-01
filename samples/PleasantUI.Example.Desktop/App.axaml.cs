using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using Serilog;

namespace PleasantUI.Example.Desktop;

public class App : PleasantUiExampleApp
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

        Avalonia.Threading.Dispatcher.UIThread.UnhandledException += (sender, args) =>
        {
            Log.Error(args.Exception, "Unhandled UI exception");
        };
        
        if (Design.IsDesignMode)
        {
            desktop.MainWindow = new PleasantWindow();
            base.OnFrameworkInitializationCompleted();
            return;
        }
        
        PleasantTheme = Styles[0] as PleasantTheme ?? throw new NullReferenceException("PleasantTheme is null");

        InitializeFromSettings();
        
        Main = new MainWindow { DataContext = ViewModel };

        TopLevel = TopLevel.GetTopLevel(Main as PleasantWindow)
                   ?? throw new NullReferenceException("TopLevel is null");

        desktop.MainWindow = Main as PleasantWindow;

        base.OnFrameworkInitializationCompleted();
    }
}