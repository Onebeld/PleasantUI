using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace PleasantUI.Controls;

/// <summary>
/// A flyout-style application menu with a title, optional info badges, a grid of large icon buttons,
/// and a footer bar with small utility buttons.
/// </summary>
/// <example>
/// <code>
/// &lt;PleasantMenu Title="Menu" Columns="3"&gt;
///     &lt;PleasantMenu.Items&gt;
///         &lt;PleasantMenuItem Label="Open" Icon="{DynamicResource OpenRegular}" Command="{Binding OpenCmd}" /&gt;
///     &lt;/PleasantMenu.Items&gt;
///     &lt;PleasantMenu.FooterItems&gt;
///         &lt;PleasantMenuFooterItem Icon="{DynamicResource TuneRegular}" Command="{Binding SettingsCmd}" /&gt;
///     &lt;/PleasantMenu.FooterItems&gt;
/// &lt;/PleasantMenu&gt;
/// </code>
/// </example>
public class PleasantMenu : TemplatedControl
{
    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Title shown in the top-left of the menu.</summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PleasantMenu, string>(nameof(Title), string.Empty);

    /// <summary>Number of columns in the items grid. Default is 3.</summary>
    public static readonly StyledProperty<int> ColumnsProperty =
        AvaloniaProperty.Register<PleasantMenu, int>(nameof(Columns), 3);

    /// <summary>Large icon button items displayed in the grid.</summary>
    public static readonly DirectProperty<PleasantMenu, AvaloniaList<PleasantMenuItem>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<PleasantMenu, AvaloniaList<PleasantMenuItem>>(
            nameof(Items), o => o.Items);

    /// <summary>Small icon buttons in the footer bar.</summary>
    public static readonly DirectProperty<PleasantMenu, AvaloniaList<PleasantMenuFooterItem>> FooterItemsProperty =
        AvaloniaProperty.RegisterDirect<PleasantMenu, AvaloniaList<PleasantMenuFooterItem>>(
            nameof(FooterItems), o => o.FooterItems);

    /// <summary>Optional info badge controls shown in the top-right of the header.</summary>
    public static readonly StyledProperty<object?> BadgesProperty =
        AvaloniaProperty.Register<PleasantMenu, object?>(nameof(Badges));

    /// <summary>Whether the footer bar is visible. Defaults to true when FooterItems is non-empty.</summary>
    public static readonly StyledProperty<bool> ShowFooterProperty =
        AvaloniaProperty.Register<PleasantMenu, bool>(nameof(ShowFooter), true);

    /// <summary>Minimum width of each grid item button.</summary>
    public static readonly StyledProperty<double> ItemMinWidthProperty =
        AvaloniaProperty.Register<PleasantMenu, double>(nameof(ItemMinWidth), 100);

    /// <summary>Minimum height of each grid item button.</summary>
    public static readonly StyledProperty<double> ItemMinHeightProperty =
        AvaloniaProperty.Register<PleasantMenu, double>(nameof(ItemMinHeight), 70);

    private readonly AvaloniaList<PleasantMenuItem>       _items       = new();
    private readonly AvaloniaList<PleasantMenuFooterItem> _footerItems = new();

    private Panel?          _itemsPanel;
    private Panel?          _footerLeftPanel;
    private Panel?          _footerRightPanel;

    public string Title       { get => GetValue(TitleProperty);       set => SetValue(TitleProperty, value); }
    public int    Columns     { get => GetValue(ColumnsProperty);     set => SetValue(ColumnsProperty, value); }
    public object? Badges     { get => GetValue(BadgesProperty);      set => SetValue(BadgesProperty, value); }
    public bool   ShowFooter  { get => GetValue(ShowFooterProperty);  set => SetValue(ShowFooterProperty, value); }
    public double ItemMinWidth  { get => GetValue(ItemMinWidthProperty);  set => SetValue(ItemMinWidthProperty, value); }
    public double ItemMinHeight { get => GetValue(ItemMinHeightProperty); set => SetValue(ItemMinHeightProperty, value); }

    public AvaloniaList<PleasantMenuItem>       Items       => _items;
    public AvaloniaList<PleasantMenuFooterItem> FooterItems => _footerItems;

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _itemsPanel       = e.NameScope.Find<Panel>("PART_ItemsPanel");
        _footerLeftPanel  = e.NameScope.Find<Panel>("PART_FooterLeft");
        _footerRightPanel = e.NameScope.Find<Panel>("PART_FooterRight");

        RebuildItems();
        RebuildFooter();

        _items.CollectionChanged       += (_, _) => RebuildItems();
        _footerItems.CollectionChanged += (_, _) => RebuildFooter();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ColumnsProperty)
            RebuildItems();
    }

    // ── Build helpers ─────────────────────────────────────────────────────────

    private void RebuildItems()
    {
        if (_itemsPanel is null) return;
        _itemsPanel.Children.Clear();

        // Use a WrapPanel so items flow naturally and respect Columns via MinWidth
        var wrap = new WrapPanel { Orientation = Orientation.Horizontal };

        foreach (var item in _items)
            wrap.Children.Add(BuildItemButton(item));

        _itemsPanel.Children.Add(wrap);
    }

    private Button BuildItemButton(PleasantMenuItem item)
    {
        // If the item has a secondary command, split into main + chevron
        if (item.SecondaryCommand is not null)
            return BuildSplitButton(item);

        var icon = new PathIcon
        {
            Width  = 20,
            Height = 20,
            Data   = item.Icon
        };

        var label = new TextBlock
        {
            Text            = item.Label,
            TextAlignment   = TextAlignment.Center,
            TextWrapping    = TextWrapping.Wrap,
            FontSize        = 12,
            Margin          = new Thickness(0, 6, 0, 0)
        };

        var stack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            Children            = { icon, label }
        };

        var btn = new Button
        {
            Content           = stack,
            MinWidth          = ItemMinWidth,
            MinHeight         = ItemMinHeight,
            Padding           = new Thickness(12, 10),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment   = VerticalAlignment.Center,
            Command           = item.Command,
            CommandParameter  = item.CommandParameter,
            IsEnabled         = item.IsEnabled,
            Margin            = new Thickness(5)
        };

        if (item.ToolTip is not null)
            ToolTip.SetTip(btn, item.ToolTip);

        btn.Click += (_, _) => CloseFlyout();
        return btn;
    }

    private Button BuildSplitButton(PleasantMenuItem item)
    {
        var icon = new PathIcon { Width = 20, Height = 20, Data = item.Icon };

        var mainBtn = new Button
        {
            Content          = icon,
            Padding          = new Thickness(12, 10),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment   = VerticalAlignment.Center,
            Command          = item.Command,
            CommandParameter = item.CommandParameter,
            IsEnabled        = item.IsEnabled,
            [Grid.ColumnProperty] = 0
        };
        mainBtn.Click += (_, _) => CloseFlyout();

        var chevronIcon = new PathIcon
        {
            Width  = 10,
            Height = 10,
            Data   = Application.Current!.TryFindResource("ChevronDownRegular", out object? g) ? g as Geometry : null
        };

        var chevronBtn = new Button
        {
            Content          = chevronIcon,
            Padding          = new Thickness(6, 10),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment   = VerticalAlignment.Center,
            Command          = item.SecondaryCommand,
            CommandParameter = item.CommandParameter,
            IsEnabled        = item.IsEnabled,
            [Grid.ColumnProperty] = 1
        };

        var splitGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Children          = { mainBtn, chevronBtn }
        };

        var label = new TextBlock
        {
            Text          = item.Label,
            TextAlignment = TextAlignment.Center,
            TextWrapping  = TextWrapping.Wrap,
            FontSize      = 12,
            Margin        = new Thickness(0, 6, 0, 0)
        };

        var outer = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Children            = { splitGrid, label }
        };

        var wrapper = new Button
        {
            Content           = outer,
            MinWidth          = ItemMinWidth,
            MinHeight         = ItemMinHeight,
            Padding           = new Thickness(0),
            IsEnabled         = item.IsEnabled,
            Margin            = new Thickness(5),
            Background        = Brushes.Transparent,
            BorderThickness   = new Thickness(0)
        };

        if (item.ToolTip is not null)
            ToolTip.SetTip(wrapper, item.ToolTip);

        return wrapper;
    }

    private void RebuildFooter()
    {
        if (_footerLeftPanel is null || _footerRightPanel is null) return;
        _footerLeftPanel.Children.Clear();
        _footerRightPanel.Children.Clear();

        foreach (var fi in _footerItems)
        {
            var icon = new PathIcon { Width = 16, Height = 16, Data = fi.Icon };

            var btn = new Button
            {
                Content          = icon,
                Padding          = new Thickness(8, 6),
                Command          = fi.Command,
                CommandParameter = fi.CommandParameter,
                IsEnabled        = fi.IsEnabled
            };

            if (Application.Current!.TryFindResource("AppBarButtonTheme", out object? t) && t is ControlTheme theme)
                btn.Theme = theme;

            if (fi.ToolTip is not null)
                ToolTip.SetTip(btn, fi.ToolTip);

            btn.Click += (_, _) => CloseFlyout();

            if (fi.AlignRight)
                _footerRightPanel.Children.Add(btn);
            else
                _footerLeftPanel.Children.Add(btn);
        }
    }

    private void CloseFlyout()
    {
        // Walk up to find the hosting Flyout and close it
        var parent = Parent;
        while (parent is not null)
        {
            if (parent is Popup popup)
            {
                popup.IsOpen = false;
                return;
            }
            parent = parent.Parent;
        }
    }

    protected override Type StyleKeyOverride => typeof(PleasantMenu);
}
