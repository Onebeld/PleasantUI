using System.Resources;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Desktop;

public partial class App : PleasantUIExampleApp
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        
        if (Design.IsDesignMode)
        {
            desktop.MainWindow = new Window();
            
            base.OnFrameworkInitializationCompleted();
            return;
        }
        
        PleasantTheme = Styles[0] as PleasantTheme ?? throw new NullReferenceException("PleasantTheme is null");
        
        Localizer.Instance.AddResourceManager(new ResourceManager(typeof(Properties.Localization)));
        Localizer.Instance.EditLanguage("en");
        
        Main = new MainWindow
        {
            DataContext = ViewModel
        };

        TopLevel = TopLevel.GetTopLevel(Main as PleasantWindow) ?? throw new NullReferenceException("TopLevel is null");
            
        desktop.MainWindow = Main as PleasantWindow;
        
        base.OnFrameworkInitializationCompleted();
    }
}