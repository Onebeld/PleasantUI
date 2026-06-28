using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Core;

namespace PleasantUI.Example.Desktop;

public class App : PleasantUiExampleApp
{
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
        
        Main = new MainWindow { DataContext = ViewModel };

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
        TrayIcons? icons = TrayIcon.GetIcons(this);
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
        Window? mainWindow = Main as Window;

        // Footer: show/hide main window + exit
        Button showHideBtn = new()
        {
            Content = mainWindow?.IsVisible == true ? "Hide main window" : "Show main window",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Padding = new Thickness(8, 6),
        };
        showHideBtn.Click += (_, _) =>
        {
            _trayPopup?.Dismiss();
            if (mainWindow is null) return;
            if (mainWindow.IsVisible) mainWindow.Hide();
            else { mainWindow.Show(); mainWindow.Activate(); }
        };

        Button exitBtn = new()
        {
            Content = "Exit",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Padding = new Thickness(8, 6),
            Foreground = new SolidColorBrush(Color.Parse("#FF6B6B")),
        };
        exitBtn.Click += (_, _) =>
        {
            _trayPopup?.Dismiss();
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
                d.Shutdown();
        };

        StackPanel footer = new() { Spacing = 2, Children = { showHideBtn, exitBtn } };

        // Content: version info
        StackPanel content = new()
        {
            Margin = new Thickness(14, 8),
            Spacing = 4,
            Children =
            {
                new TextBlock
                {
                    Text = "PleasantUI — cross-platform UI library for Avalonia.",
                    TextWrapping = TextWrapping.Wrap,
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

        this.TryFindResource("PleasantUILogo", out object? appIconResource);

        PleasantTrayPopup popup = new()
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
}