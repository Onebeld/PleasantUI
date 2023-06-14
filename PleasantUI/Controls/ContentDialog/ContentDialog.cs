using Avalonia;
using Avalonia.Controls.Primitives;

namespace PleasantUI.Controls;

public class ContentDialog : PleasantModalWindow
{
    public static readonly StyledProperty<object> BottomPanelContentProperty =
        AvaloniaProperty.Register<ContentDialog, object>(nameof(BottomPanelContent));

    public object BottomPanelContent
    {
        get => GetValue(BottomPanelContentProperty);
        set => SetValue(BottomPanelContentProperty, value);
    }
    
    static ContentDialog() { }
    
    protected override Type StyleKeyOverride => typeof(ContentDialog);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Focus();
    }
}