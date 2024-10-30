using Avalonia.Controls;
using PleasantUI.Controls.Flyout.Presenters;

namespace PleasantUI.Controls.Flyout;

/// <summary>
/// A custom flyout control that provides additional functionality and styling options.
/// </summary>
public class PleasantFlyout : Avalonia.Controls.Flyout
{
    /// <inheritdoc />
    protected override Control CreatePresenter()
    {
        return new PleasantFlyoutPresenter
        {
            [!ContentControl.ContentProperty] = this[!ContentProperty],
            [!PleasantFlyoutPresenter.IsOpenPresenterProperty] = this[!IsOpenProperty]
        };
    }
}