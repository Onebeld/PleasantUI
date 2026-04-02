using System.Windows.Input;
using Avalonia;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a small icon button in the footer bar of a <see cref="PleasantMenu"/>.
/// </summary>
public class PleasantMenuFooterItem : AvaloniaObject
{
    /// <summary>Icon geometry.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<PleasantMenuFooterItem, Geometry?>(nameof(Icon));

    /// <summary>Command invoked when clicked.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<PleasantMenuFooterItem, ICommand?>(nameof(Command));

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<PleasantMenuFooterItem, object?>(nameof(CommandParameter));

    /// <summary>Tooltip text.</summary>
    public static readonly StyledProperty<string?> ToolTipProperty =
        AvaloniaProperty.Register<PleasantMenuFooterItem, string?>(nameof(ToolTip));

    /// <summary>Whether the button is enabled.</summary>
    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<PleasantMenuFooterItem, bool>(nameof(IsEnabled), true);

    /// <summary>
    /// When true the item is right-aligned in the footer; otherwise left-aligned.
    /// </summary>
    public static readonly StyledProperty<bool> AlignRightProperty =
        AvaloniaProperty.Register<PleasantMenuFooterItem, bool>(nameof(AlignRight));

    public Geometry? Icon             { get => GetValue(IconProperty);             set => SetValue(IconProperty, value); }
    public ICommand? Command          { get => GetValue(CommandProperty);           set => SetValue(CommandProperty, value); }
    public object?   CommandParameter { get => GetValue(CommandParameterProperty);  set => SetValue(CommandParameterProperty, value); }
    public string?   ToolTip          { get => GetValue(ToolTipProperty);           set => SetValue(ToolTipProperty, value); }
    public bool      IsEnabled        { get => GetValue(IsEnabledProperty);         set => SetValue(IsEnabledProperty, value); }
    public bool      AlignRight       { get => GetValue(AlignRightProperty);        set => SetValue(AlignRightProperty, value); }
}
