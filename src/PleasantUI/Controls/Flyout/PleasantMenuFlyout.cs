using Avalonia.Controls;
using PleasantUI.Controls.Flyout.Presenters;

namespace PleasantUI.Controls.Flyout;

/// <summary>
/// A custom flyout control that provides additional functionality and styling options.
/// </summary>
public class PleasantMenuFlyout : MenuFlyout
{
    /// <inheritdoc />
    protected override Control CreatePresenter()
    {
        return new PleasantMenuFlyoutPresenter
        {
            ItemsSource = Items,
            [!PleasantMenuFlyoutPresenter.IsOpenPresenterProperty] = this[!IsOpenProperty],
            [!ItemsControl.ItemTemplateProperty] = this[!ItemTemplateProperty],
            [!ItemsControl.ItemContainerThemeProperty] = this[!ItemContainerThemeProperty],
        };
    }
}