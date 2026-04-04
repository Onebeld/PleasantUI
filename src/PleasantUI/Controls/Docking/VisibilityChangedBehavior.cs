using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Reactive;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// A behavior that adjusts grid column or row sizes when content visibility changes.
/// </summary>
public class VisibilityChangedBehavior : Behavior<ContentPresenter>
{
    /// <summary>Gets or sets the grid length value to apply when hidden.</summary>
    public GridLength Value { get; set; }

    /// <summary>Gets or sets the row index.</summary>
    public int? Row { get; set; }

    /// <summary>Gets or sets the column index.</summary>
    public int? Column { get; set; }

    private IDisposable? _childWatchDisposable;
    private IDisposable? _visibilityDisposable;

    /// <summary>Sets the grid length for a content presenter.</summary>
    public static void SetGridLength(ContentPresenter control, GridLength value)
        => GetBehavior(control).Value = value;

    /// <summary>Sets the row for a content presenter.</summary>
    public static void SetRow(ContentPresenter control, int value)
        => GetBehavior(control).Row = value;

    /// <summary>Sets the column for a content presenter.</summary>
    public static void SetColumn(ContentPresenter control, int value)
        => GetBehavior(control).Column = value;

    private static VisibilityChangedBehavior GetBehavior(ContentPresenter control)
    {
        var list = Interaction.GetBehaviors(control);
        var behavior = list.OfType<VisibilityChangedBehavior>().FirstOrDefault();

        if (behavior == null)
        {
            behavior = new VisibilityChangedBehavior();
            list.Add(behavior);
        }

        return behavior;
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject == null) return;

        _childWatchDisposable = AssociatedObject.GetObservable(ContentPresenter.ChildProperty)
            .Subscribe(new AnonymousObserver<Control?>(child =>
            {
                _visibilityDisposable?.Dispose();
                _visibilityDisposable = null;

                if (child != null)
                {
                    _visibilityDisposable = child.GetObservable(Visual.IsVisibleProperty)
                        .Subscribe(new AnonymousObserver<bool>(OnIsVisibleChanged));
                }
            }));
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        _visibilityDisposable?.Dispose();
        _childWatchDisposable?.Dispose();
    }

    private void OnIsVisibleChanged(bool value)
    {
        if (value || AssociatedObject == null) return;

        var grid = AssociatedObject.FindAncestorOfType<Grid>();
        if (grid == null) return;

        var column = Column ?? Grid.GetColumn(AssociatedObject);
        if (column < grid.ColumnDefinitions.Count)
            grid.ColumnDefinitions[column].Width = Value;

        var row = Row ?? Grid.GetRow(AssociatedObject);
        if (row < grid.RowDefinitions.Count)
            grid.RowDefinitions[row].Height = Value;
    }
}
