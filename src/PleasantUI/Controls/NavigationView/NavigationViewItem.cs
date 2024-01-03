using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using PleasantUI.Extensions;

namespace PleasantUI.Controls;

public class NavigationViewItem : NavigationViewItemBase
{
    /// <inheritdoc cref="StyleKeyOverride"/>
    protected override Type StyleKeyOverride => typeof(NavigationViewItemBase);

    static NavigationViewItem()
    {
        SelectableMixin.Attach<NavigationViewItem>(IsSelectedProperty);
        FocusableProperty.OverrideDefaultValue<NavigationViewItem>(true);
        ClickModeProperty.OverrideDefaultValue<NavigationViewItem>(ClickMode.Release);
    }

    protected override void OnClosed(object sender, RoutedEventArgs e)
    {
        base.OnClosed(sender, e);

        if (SelectOnClose)
            this.GetParentTOfLogical<NavigationView>()?.SelectSingleItem(this);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        NavigationViewDistance = Extensions.LogicalExtensions.CalculateDistanceFromLogicalParent<NavigationView>(this) - 1;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            e.Handled = true;

            if (ClickMode == ClickMode.Press)
                Select();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            e.Handled = true;

            if (ClickMode == ClickMode.Release &&
                this.GetVisualsAt(e.GetPosition(this)).Any(c => this == c || this.IsVisualAncestorOf(c)))
            {
                Select();
            }
        }
    }

    private void Select()
    {
        if (!IsSelected)
            this.GetParentTOfLogical<NavigationView>()?.SelectSingleItem(this);
    }
}