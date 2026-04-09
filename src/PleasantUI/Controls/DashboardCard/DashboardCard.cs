using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace PleasantUI.Controls;

/// <summary>
/// A card control with a structured header (title + optional toolbar) and a scrollable content area.
/// Designed for dashboard panels, metric cards, and data displays.
/// </summary>
[TemplatePart(PART_HeaderToolbar, typeof(ItemsControl), IsRequired = false)]
[PseudoClasses(PC_HasToolbar, PC_HasSubtitle)]
public class DashboardCard : HeaderedContentControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_HeaderToolbar = "PART_HeaderToolbar";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_HasToolbar  = ":hasToolbar";
    private const string PC_HasSubtitle = ":hasSubtitle";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Subtitle"/> property.</summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<DashboardCard, string?>(nameof(Subtitle));

    /// <summary>Defines the <see cref="HeaderContent"/> property.</summary>
    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<DashboardCard, object?>(nameof(HeaderContent));

    /// <summary>Defines the <see cref="HeaderContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderContentTemplateProperty =
        AvaloniaProperty.Register<DashboardCard, IDataTemplate?>(nameof(HeaderContentTemplate));

    /// <summary>Defines the <see cref="FooterContent"/> property.</summary>
    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<DashboardCard, object?>(nameof(FooterContent));

    /// <summary>Defines the <see cref="FooterContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> FooterContentTemplateProperty =
        AvaloniaProperty.Register<DashboardCard, IDataTemplate?>(nameof(FooterContentTemplate));

    /// <summary>Defines the <see cref="ContentPadding"/> property.</summary>
    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<DashboardCard, Thickness>(nameof(ContentPadding), new Thickness(16));

    /// <summary>Defines the <see cref="ShowScrollViewer"/> property.</summary>
    public static readonly StyledProperty<bool> ShowScrollViewerProperty =
        AvaloniaProperty.Register<DashboardCard, bool>(nameof(ShowScrollViewer), defaultValue: true);

    /// <summary>Defines the <see cref="ToolbarItems"/> property.</summary>
    public static readonly DirectProperty<DashboardCard, AvaloniaList<object>> ToolbarItemsProperty =
        AvaloniaProperty.RegisterDirect<DashboardCard, AvaloniaList<object>>(
            nameof(ToolbarItems), o => o.ToolbarItems);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets a subtitle shown below the header.</summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Gets or sets arbitrary content placed in the header's right slot.</summary>
    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    /// <summary>Gets or sets the data template for <see cref="HeaderContent"/>.</summary>
    public IDataTemplate? HeaderContentTemplate
    {
        get => GetValue(HeaderContentTemplateProperty);
        set => SetValue(HeaderContentTemplateProperty, value);
    }

    /// <summary>Gets or sets content shown in the card footer.</summary>
    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    /// <summary>Gets or sets the data template for <see cref="FooterContent"/>.</summary>
    public IDataTemplate? FooterContentTemplate
    {
        get => GetValue(FooterContentTemplateProperty);
        set => SetValue(FooterContentTemplateProperty, value);
    }

    /// <summary>Gets or sets the padding applied to the content area.</summary>
    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    /// <summary>Gets or sets whether the content area is wrapped in a <see cref="SmoothScrollViewer"/>.</summary>
    public bool ShowScrollViewer
    {
        get => GetValue(ShowScrollViewerProperty);
        set => SetValue(ShowScrollViewerProperty, value);
    }

    /// <summary>Gets the collection of toolbar items shown in the header's right slot.</summary>
    [Content]
    public AvaloniaList<object> ToolbarItems { get; } = new();

    // ── Overrides ─────────────────────────────────────────────────────────────

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SubtitleProperty)
            PseudoClasses.Set(PC_HasSubtitle, change.NewValue is not null);
        else if (change.Property == HeaderContentProperty)
            PseudoClasses.Set(PC_HasToolbar, change.NewValue is not null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var toolbar = e.NameScope.Find<ItemsControl>(PART_HeaderToolbar);
        if (toolbar is not null)
            toolbar.ItemsSource = ToolbarItems;

        PseudoClasses.Set(PC_HasSubtitle, Subtitle is not null);
        PseudoClasses.Set(PC_HasToolbar,  HeaderContent is not null || ToolbarItems.Count > 0);
    }
}
