using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls.Flyout.Presenters;

/// <summary>
/// A presenter for the PleasantFlyout, providing properties and functionality for its open state.
/// </summary>
public class PleasantFlyoutPresenter : FlyoutPresenter
{
    /// <summary>
    /// Defines the IsOpenPresenter property which indicates whether the presenter is open.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenPresenterProperty =
        AvaloniaProperty.Register<PleasantFlyoutPresenter, bool>(nameof(IsOpenPresenter));

    /// <summary>
    /// Gets or sets a value indicating whether the presenter is open.
    /// </summary>
    public bool IsOpenPresenter
    {
        get => GetValue(IsOpenPresenterProperty);
        set => SetValue(IsOpenPresenterProperty, value);
    }
}