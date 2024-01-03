using Avalonia;
using Avalonia.Controls.Primitives;

namespace PleasantUI.Controls;

public class ContentDialog : PleasantModalWindow
{
    public static readonly StyledProperty<object> BottomPanelContentProperty =
        AvaloniaProperty.Register<ContentDialog, object>(nameof(BottomPanelContent));

    /// <summary>
    /// Allows you to embed content on the modal window (e.g. buttons)
    /// </summary>
    public object BottomPanelContent
    {
        get => GetValue(BottomPanelContentProperty);
        set => SetValue(BottomPanelContentProperty, value);
    }
    
    static ContentDialog() { }
    
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ContentDialog);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Focus();
    }
}