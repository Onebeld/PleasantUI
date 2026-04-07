using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using PleasantUI.Controls;
using PleasantUI.Core;

namespace PleasantUI.Example.Desktop;

public partial class App : PleasantUiExampleApp
{
    private StyleInclude? _vguiExampleStyles;

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

        // Apply VGUI example styles if VGUI is the active theme, and subscribe to future changes.
        UpdateVguiExampleStyles();
        if (PleasantSettings.Current is not null)
            PleasantSettings.Current.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PleasantSettings.Current.Theme))
                    UpdateVguiExampleStyles();
            };
        
        Main = new MainWindow
        {
            DataContext = ViewModel
        };

        TopLevel = TopLevel.GetTopLevel(Main as PleasantWindow) ?? throw new NullReferenceException("TopLevel is null");
            
        desktop.MainWindow = Main as PleasantWindow;
        
        base.OnFrameworkInitializationCompleted();
    }

    private void UpdateVguiExampleStyles()
    {
        bool isVgui = PleasantSettings.Current?.Theme == "VGUI";
        if (isVgui)
        {
            if (_vguiExampleStyles is null)
            {
                _vguiExampleStyles = new StyleInclude(new Uri("avares://PleasantUI.Example/"))
                {
                    Source = new Uri("avares://PleasantUI.Example/Styling/VGUIExampleStyles.axaml")
                };
                Styles.Add(_vguiExampleStyles);
            }
        }
        else
        {
            if (_vguiExampleStyles is not null)
            {
                Styles.Remove(_vguiExampleStyles);
                _vguiExampleStyles = null;
            }
        }
    }
}