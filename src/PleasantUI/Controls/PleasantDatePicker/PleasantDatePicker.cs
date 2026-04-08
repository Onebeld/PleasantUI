using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace PleasantUI.Controls;

/// <summary>
/// A PleasantUI-styled DatePicker that decouples the visual container from
/// the internal flyout button, allowing full theme control over the outer shape.
/// <para>
/// The template must provide:
/// <list type="bullet">
///   <item><c>PART_ClickArea</c> — the visible, styled container that the user clicks</item>
///   <item><c>PART_FlyoutButton</c> — a hidden zero-size Button kept for Avalonia's internal wiring</item>
/// </list>
/// </para>
/// </summary>
[TemplatePart("PART_ClickArea",   typeof(InputElement))]
[TemplatePart("PART_FlyoutButton", typeof(Button))]
public class PleasantDatePicker : DatePicker
{
    private InputElement? _clickArea;
    private Button?       _flyoutButton;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PleasantDatePicker);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // Detach old handlers
        if (_clickArea is not null)
            _clickArea.PointerPressed -= OnClickAreaPointerPressed;

        base.OnApplyTemplate(e);

        _flyoutButton = e.NameScope.Find<Button>("PART_FlyoutButton");
        _clickArea    = e.NameScope.Find<InputElement>("PART_ClickArea");

        // Hide the real flyout button — Avalonia's DatePicker.cs already wired its Click
        // handler in base.OnApplyTemplate. We keep it in the tree (zero size) so the
        // internal wiring stays intact, but we drive it from PART_ClickArea instead.
        if (_flyoutButton is not null)
        {
            _flyoutButton.Width  = 0;
            _flyoutButton.Height = 0;
            _flyoutButton.IsHitTestVisible = false;
            _flyoutButton.IsTabStop        = false;
        }

        if (_clickArea is not null)
            _clickArea.PointerPressed += OnClickAreaPointerPressed;
    }

    private void OnClickAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Simulate a click on the hidden flyout button so Avalonia's internal
        // DatePicker logic opens the popup exactly as designed.
        _flyoutButton?.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
    }
}
