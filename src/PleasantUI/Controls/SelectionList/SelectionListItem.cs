using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a single item in a <see cref="SelectionList"/>.
/// Supports an image/icon, title, subtitle, and timestamp.
/// </summary>
[PseudoClasses(":no-image", ":no-subtitle", ":no-timestamp")]
public class SelectionListItem : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SelectionListItem, object?>(nameof(Icon));

    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty =
        AvaloniaProperty.Register<SelectionListItem, IDataTemplate?>(nameof(IconTemplate));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SelectionListItem, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<SelectionListItem, string?>(nameof(Subtitle));

    public static readonly StyledProperty<string?> TimestampProperty =
        AvaloniaProperty.Register<SelectionListItem, string?>(nameof(Timestamp));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IDataTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string? Timestamp
    {
        get => GetValue(TimestampProperty);
        set => SetValue(TimestampProperty, value);
    }
    
    /// <summary>
    /// Overrides the style key so Avalonia's theme lookup resolves
    /// <see cref="SelectionListItem"/> instead of the base <see cref="ListBoxItem"/> type.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(SelectionListItem);

    static SelectionListItem() { }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IconProperty || e.Property == IconTemplateProperty || e.Property == SubtitleProperty || e.Property == TimestampProperty)
            UpdatePseudoClasses();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":no-image",     Icon is null && IconTemplate is null);
        PseudoClasses.Set(":no-subtitle",  string.IsNullOrEmpty(Subtitle));
        PseudoClasses.Set(":no-timestamp", string.IsNullOrEmpty(Timestamp));
    }
}
