using System.Windows.Input;
using Avalonia;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a large icon button in the grid area of a <see cref="PleasantMenu"/>.
/// </summary>
public class PleasantMenuItem : AvaloniaObject
{
    /// <summary>Icon geometry displayed in the button.</summary>
    public static readonly StyledProperty<Geometry?> IconProperty =
        AvaloniaProperty.Register<PleasantMenuItem, Geometry?>(nameof(Icon));

    /// <summary>Label shown below the icon.</summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<PleasantMenuItem, string>(nameof(Label), string.Empty);

    /// <summary>Command invoked when the button is clicked.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<PleasantMenuItem, ICommand?>(nameof(Command));

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        AvaloniaProperty.Register<PleasantMenuItem, object?>(nameof(CommandParameter));

    /// <summary>Tooltip text.</summary>
    public static readonly StyledProperty<string?> ToolTipProperty =
        AvaloniaProperty.Register<PleasantMenuItem, string?>(nameof(ToolTip));

    /// <summary>Whether the button is enabled.</summary>
    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<PleasantMenuItem, bool>(nameof(IsEnabled), true);

    /// <summary>
    /// Optional secondary command shown as a small dropdown chevron button next to the main button.
    /// When set, the main button area is split: left = <see cref="Command"/>, right = <see cref="SecondaryCommand"/>.
    /// </summary>
    public static readonly StyledProperty<ICommand?> SecondaryCommandProperty =
        AvaloniaProperty.Register<PleasantMenuItem, ICommand?>(nameof(SecondaryCommand));

    public Geometry? Icon           { get => GetValue(IconProperty);            set => SetValue(IconProperty, value); }
    public string    Label          { get => GetValue(LabelProperty);           set => SetValue(LabelProperty, value); }
    public ICommand? Command        { get => GetValue(CommandProperty);         set => SetValue(CommandProperty, value); }
    public object?   CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }
    public string?   ToolTip        { get => GetValue(ToolTipProperty);         set => SetValue(ToolTipProperty, value); }
    public bool      IsEnabled      { get => GetValue(IsEnabledProperty);       set => SetValue(IsEnabledProperty, value); }
    public ICommand? SecondaryCommand { get => GetValue(SecondaryCommandProperty); set => SetValue(SecondaryCommandProperty, value); }
}
