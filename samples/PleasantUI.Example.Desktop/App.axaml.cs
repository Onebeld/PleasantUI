using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;

namespace PleasantUI.Example.Desktop;

public partial class App : PleasantUIExampleApp
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        PleasantTheme = Styles[0] as PleasantTheme ?? throw new NullReferenceException("PleasantTheme is null");
        
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        if (Design.IsDesignMode)
        {
            desktop.MainWindow = new Window();
            return;
        }
        
        Main = new MainWindow
        {
            DataContext = ViewModel
        };

        TopLevel = TopLevel.GetTopLevel(Main as PleasantWindow) ?? throw new NullReferenceException("TopLevel is null");
            
        desktop.MainWindow = Main as PleasantWindow;
        
        base.OnFrameworkInitializationCompleted();
    }
}