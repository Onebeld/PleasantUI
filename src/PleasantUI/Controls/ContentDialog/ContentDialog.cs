using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a dialog that can be used to display content to the user.
/// </summary>
/// <remarks>
/// This dialog provides a modal experience, meaning that it blocks interaction with the rest of the application until it is closed.
/// </remarks>
public class ContentDialog : PleasantModalWindow, ICustomKeyboardNavigation
{
    /// <summary>
    /// Defines the <see cref="BottomPanelContent"/> property.
    /// </summary>
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

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Focus(NavigationMethod.Pointer);
    }

    /// <inheritdoc/>
    public (bool handled, IInputElement? next) GetNext(IInputElement element, NavigationDirection direction)
    {
        List<IInputElement> children = this.GetVisualDescendants().OfType<IInputElement>()
            .Where(x => KeyboardNavigation.GetIsTabStop((InputElement)x) && x.Focusable &&
                        x.IsEffectivelyVisible && x.IsEffectivelyEnabled).ToList();
        
        if (children.Count == 0)
            return (false, null);

        IInputElement? current = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (current == null)
            return (false, null);

        if (direction == NavigationDirection.Next)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != current) continue;
                
                if (i == children.Count - 1)
                    return (true, children[0]);

                return (true, children[i + 1]);
            }
        }
        else if (direction == NavigationDirection.Previous)
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                if (children[i] != current) continue;
                
                if (i == 0)
                    return (true, children[children.Count - 1]);

                return (true, children[i - 1]);
            }
        }

        return (false, null);
    }
}