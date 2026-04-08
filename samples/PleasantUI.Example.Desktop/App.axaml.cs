using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Controls.Chrome;
using PleasantUI.Core;

namespace PleasantUI.Example.Desktop;

public partial class App : PleasantUiExampleApp
{
    private StyleInclude?      _vguiExampleStyles;
    private PleasantTrayPopup? _trayPopup;

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

        InitializeFromSettings();

        UpdateVguiExampleStyles();
        if (PleasantSettings.Current is not null)
            PleasantSettings.Current.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PleasantSettings.Current.Theme))
                    UpdateVguiExampleStyles();
            };
        
        Main = new MainWindow { DataContext = ViewModel };

        // Apply compact titlebar now that Main exists (UpdateVguiExampleStyles ran before Main was created)
        if (PleasantSettings.Current?.Theme == "VGUI" && Main is PleasantWindow winInit)
            winInit.TitleBarType = PleasantTitleBar.Type.Compact;

        TopLevel = TopLevel.GetTopLevel(Main as PleasantWindow)
                   ?? throw new NullReferenceException("TopLevel is null");

        desktop.MainWindow = Main as PleasantWindow;

        // Wire tray icon left-click to show PleasantTrayPopup
        WireTrayIcon();

        base.OnFrameworkInitializationCompleted();
    }

    // ── Tray popup ────────────────────────────────────────────────────────────

    private void WireTrayIcon()
    {
        var icons = TrayIcon.GetIcons(this);
        if (icons is null || icons.Count == 0) return;

        icons[0].Clicked += OnTrayIconClicked;
    }

    private void OnTrayIconClicked(object? sender, EventArgs e)
    {
        if (_trayPopup is { IsVisible: true })
        {
            _trayPopup.Dismiss();
            return;
        }

        _trayPopup = BuildTrayPopup();
        _trayPopup.ShowNearTray();
    }

    private PleasantTrayPopup BuildTrayPopup()
    {
        var mainWindow = Main as Window;

        // Footer: show/hide main window + exit
        var showHideBtn = new Button
        {
            Content = mainWindow?.IsVisible == true ? "Hide main window" : "Show main window",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Padding = new Avalonia.Thickness(8, 6),
        };
        showHideBtn.Click += (_, _) =>
        {
            _trayPopup?.Dismiss();
            if (mainWindow is null) return;
            if (mainWindow.IsVisible) mainWindow.Hide();
            else { mainWindow.Show(); mainWindow.Activate(); }
        };

        var exitBtn = new Button
        {
            Content = "Exit",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Padding = new Avalonia.Thickness(8, 6),
            Foreground = new SolidColorBrush(Color.Parse("#FF6B6B")),
        };
        exitBtn.Click += (_, _) =>
        {
            _trayPopup?.Dismiss();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
                d.Shutdown();
        };

        var footer = new StackPanel { Spacing = 2, Children = { showHideBtn, exitBtn } };

        // Content: version info
        var content = new StackPanel
        {
            Margin = new Avalonia.Thickness(14, 8),
            Spacing = 4,
            Children =
            {
                new TextBlock
                {
                    Text = "PleasantUI — cross-platform UI library for Avalonia.",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.Parse("#AAFFFFFF")),
                },
                new TextBlock
                {
                    Text = $"Theme: {PleasantSettings.Current?.Theme ?? "Default"}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.Parse("#77FFFFFF")),
                },
            }
        };

        TryFindResource("PleasantUILogo", out var appIconResource);

        var popup = new PleasantTrayPopup
        {
            Width         = 280,
            AppTitle      = "PleasantUI Example",
            StatusText    = "Running",
            StatusColor   = new SolidColorBrush(Color.Parse("#4CAF50")),
            AppIcon       = appIconResource,
            Content       = content,
            FooterContent = footer,
            ShowStatusRow = false,   // no key/value pairs needed for this simple demo
        };

        return popup;
    }

    // ── VGUI styles ───────────────────────────────────────────────────────────

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

        if (Main is PleasantWindow win)
        {
            win.TitleBarType = isVgui
                ? PleasantTitleBar.Type.Compact
                : PleasantTitleBar.Type.ClassicExtended;
        }
    }
}