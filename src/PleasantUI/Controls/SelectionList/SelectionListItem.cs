using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a single item in a <see cref="SelectionList"/>.
/// Supports an image/icon, title, subtitle, and timestamp.
/// </summary>
[PseudoClasses(":no-image", ":no-subtitle", ":no-timestamp")]
public class SelectionListItem : ListBoxItem
{
    public static readonly StyledProperty<IImage?> ImageProperty =
        AvaloniaProperty.Register<SelectionListItem, IImage?>(nameof(Image));

    public static readonly StyledProperty<IDataTemplate?> ImageTemplateProperty =
        AvaloniaProperty.Register<SelectionListItem, IDataTemplate?>(nameof(ImageTemplate));

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SelectionListItem, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<SelectionListItem, string?>(nameof(Subtitle));

    public static readonly StyledProperty<string?> TimestampProperty =
        AvaloniaProperty.Register<SelectionListItem, string?>(nameof(Timestamp));

    public IImage? Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    public IDataTemplate? ImageTemplate
    {
        get => GetValue(ImageTemplateProperty);
        set => SetValue(ImageTemplateProperty, value);
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

    static SelectionListItem()
    {
        ImageProperty.Changed.AddClassHandler<SelectionListItem>((i, _) => i.UpdatePseudoClasses());
        ImageTemplateProperty.Changed.AddClassHandler<SelectionListItem>((i, _) => i.UpdatePseudoClasses());
        SubtitleProperty.Changed.AddClassHandler<SelectionListItem>((i, _) => i.UpdatePseudoClasses());
        TimestampProperty.Changed.AddClassHandler<SelectionListItem>((i, _) => i.UpdatePseudoClasses());
    }

    /// <summary>
    /// Overrides the style key so Avalonia's theme lookup resolves
    /// <see cref="SelectionListItem"/> instead of the base <see cref="ListBoxItem"/> type.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(SelectionListItem);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":no-image",     Image is null && ImageTemplate is null);
        PseudoClasses.Set(":no-subtitle",  string.IsNullOrEmpty(Subtitle));
        PseudoClasses.Set(":no-timestamp", string.IsNullOrEmpty(Timestamp));
    }
}
