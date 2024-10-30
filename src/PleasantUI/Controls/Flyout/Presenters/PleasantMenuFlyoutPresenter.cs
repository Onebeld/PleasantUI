using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls.Flyout.Presenters;

/// <summary>
/// A presenter for the <see cref="PleasantMenuFlyout"/>.
/// </summary>
public class PleasantMenuFlyoutPresenter : MenuFlyoutPresenter
{
    /// <summary>
    /// Defines the <see cref="IsOpenPresenter"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenPresenterProperty =
        AvaloniaProperty.Register<PleasantMenuFlyoutPresenter, bool>(nameof(IsOpenPresenter), false);

    /// <summary>
    /// Gets or sets whether the menu flyout is currently open.
    /// </summary>
    public bool IsOpenPresenter
    {
        get => GetValue(IsOpenPresenterProperty);
        set => SetValue(IsOpenPresenterProperty, value);
    }
}