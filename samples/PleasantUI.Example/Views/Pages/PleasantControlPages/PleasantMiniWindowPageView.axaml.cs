using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class PleasantMiniWindowPageView : LocalizedUserControl
{
    public PleasantMiniWindowPageView()
    {
        InitializeComponent();
        OpenSteamButton.Click += (_, _) => OpenSteamPanel();
        OpenGamesButton.Click += (_, _) => OpenGamesPanel(null);
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        OpenSteamButton.Click += (_, _) => OpenSteamPanel();
        OpenGamesButton.Click += (_, _) => OpenGamesPanel(null);
    }

    private static string T(string key) =>
        Localizer.Instance.TryGetString(key, out var v) ? v : key;

    private Window? OwnerWindow => TopLevel.GetTopLevel(this) as Window;

    // ── Steam main panel ────────────────────────────────────────────────────
    private void OpenSteamPanel()
    {
        var steamWindow = new PleasantMiniWindow
        {
            Title            = "Steam",
            Width            = 460,
            Height           = 340,
            ShowHiddenButton = true,
        };

        // Nav items: (label, description, opens games list)
        var navItems = new[]
        {
            (T("MiniWindow/NavGames"),    T("MiniWindow/NavGamesDesc"),    true),
            (T("MiniWindow/NavFriends"),  T("MiniWindow/NavFriendsDesc"),  false),
            (T("MiniWindow/NavServers"),  T("MiniWindow/NavServersDesc"),  false),
            (T("MiniWindow/NavMonitor"),  T("MiniWindow/NavMonitorDesc"),  false),
            (T("MiniWindow/NavSettings"), T("MiniWindow/NavSettingsDesc"), false),
        };

        // Left nav column — each row: [Button] [description text] inline
        var navStack = new StackPanel { Spacing = 8, Margin = new Thickness(12, 12, 12, 12) };
        foreach (var (label, desc, opensGames) in navItems)
        {
            var btn = new Button
            {
                Content                    = label,
                Width                      = 110,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding                    = new Thickness(8, 4),
            };
            var descText = new TextBlock
            {
                Text              = desc,
                TextWrapping      = TextWrapping.Wrap,
                Foreground        = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(12, 0, 0, 0),
                FontSize          = 12,
                MaxWidth          = 240,
            };
            var capturedOpens = opensGames;
            btn.Click += (_, _) =>
            {
                if (capturedOpens)
                    OpenGamesPanel(steamWindow);
            };

            navStack.Children.Add(new StackPanel
            {
                Orientation       = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children          = { btn, descText },
            });
        }

        // Main layout: nav rows fill the content area
        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*"),
            RowDefinitions    = new RowDefinitions("*,Auto"),
        };
        mainGrid.Children.Add(navStack);

        // Footer: VALVE  ◆ STEAM  [Close]
        var closeBtn = new Button { Content = T("MiniWindow/Close") };
        closeBtn.Click += (_, _) => steamWindow.Close();

        var footer = new Border
        {
            BorderBrush     = Brushes.Gray,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding         = new Thickness(12, 8),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*,Auto"),
                Children =
                {
                    new TextBlock
                    {
                        Text              = "VALVE",
                        FontWeight        = FontWeight.Bold,
                        FontSize          = 11,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground        = Brushes.Gray,
                    },
                    new TextBlock
                    {
                        Text              = "  ◆ STEAM",
                        FontSize          = 11,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground        = Brushes.Gray,
                        [Grid.ColumnProperty] = 1,
                    },
                    closeBtn.WithGridColumn(3),
                }
            }
        };
        Grid.SetRow(footer, 1);
        mainGrid.Children.Add(footer);

        steamWindow.Content = mainGrid;
        Show(steamWindow);
    }

    // ── Games list panel ────────────────────────────────────────────────────
    private void OpenGamesPanel(PleasantMiniWindow? owner)
    {
        var games = new[]
        {
            "Half-Life", "Counter-Strike", "Team Fortress Classic",
            "Deathmatch Classic", "Opposing Force", "Ricochet", "Dedicated Server",
        };

        var stack = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };
        stack.Children.Add(new TextBlock
        {
            Text       = T("MiniWindow/MyGames"),
            FontSize   = 11,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.Goldenrod,
            Margin     = new Thickness(8, 6, 8, 6),
        });

        Border? selected = null;
        foreach (var game in games)
        {
            var row = new Border { Padding = new Thickness(8, 5) };
            row.Child = new TextBlock { Text = game, FontSize = 13 };
            row.PointerEntered += (_, _) =>
            {
                if (row != selected)
                    row.Background = new SolidColorBrush(Color.Parse("#22FFFFFF"));
            };
            row.PointerExited += (_, _) =>
            {
                if (row != selected)
                    row.Background = null;
            };
            row.PointerPressed += (_, _) =>
            {
                if (selected is not null) selected.Background = null;
                selected = row;
                row.Background = new SolidColorBrush(Color.Parse("#44FFFFFF"));
            };
            stack.Children.Add(row);
        }

        var gamesWindow = new PleasantMiniWindow
        {
            Title   = T("MiniWindow/GamesTitle"),
            Width   = 240,
            Height  = 320,
            Content = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content                     = stack,
            },
        };

        // Position to the right of the Steam panel if it's open
        if (owner is not null)
        {
            gamesWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            gamesWindow.Opened += (_, _) =>
            {
                gamesWindow.Position = new PixelPoint(
                    (int)(owner.Position.X + owner.Width + 4),
                    owner.Position.Y);
            };
        }

        Show(gamesWindow);
    }

    private void Show(PleasantMiniWindow w)
    {
        var owner = OwnerWindow;
        if (owner is not null)
            w.ShowDialog(owner);
        else
            w.Show();
    }
}

// Small helper to set Grid.Column inline without a statement
file static class ControlExtensions
{
    public static T WithGridColumn<T>(this T control, int col) where T : Control
    {
        Grid.SetColumn(control, col);
        return control;
    }
}
