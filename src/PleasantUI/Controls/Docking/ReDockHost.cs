using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents the main host control for the docking system.
/// </summary>
[TemplatePart("PART_Root", typeof(Grid))]
[TemplatePart("PART_LeftThumb", typeof(Thumb))]
[TemplatePart("PART_RightThumb", typeof(Thumb))]
[TemplatePart("PART_MainContent", typeof(ContentPresenter))]
public class ReDockHost : ContentControl
{
    /// <summary>Defines the <see cref="ButtonMoveEvent"/> routed event.</summary>
    public static readonly RoutedEvent<SideBarButtonMoveEventArgs> ButtonMoveEvent =
        RoutedEvent.Register<ReDockHost, SideBarButtonMoveEventArgs>(nameof(ButtonMove), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="ButtonDisplayModeChangedEvent"/> routed event.</summary>
    public static readonly RoutedEvent<SideBarButtonDisplayModeChangedEventArgs> ButtonDisplayModeChangedEvent =
        RoutedEvent.Register<ReDockHost, SideBarButtonDisplayModeChangedEventArgs>(nameof(ButtonDisplayModeChanged), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="ButtonFlyoutRequestedEvent"/> routed event.</summary>
    public static readonly RoutedEvent<SideBarButtonFlyoutRequestedEventArgs> ButtonFlyoutRequestedEvent =
        RoutedEvent.Register<ReDockHost, SideBarButtonFlyoutRequestedEventArgs>(nameof(ButtonFlyoutRequested), RoutingStrategies.Bubble);

    /// <summary>Defines the <see cref="LeftSideBar"/> property.</summary>
    public static readonly StyledProperty<SideBar?> LeftSideBarProperty =
        AvaloniaProperty.Register<ReDockHost, SideBar?>(nameof(LeftSideBar));

    /// <summary>Defines the <see cref="RightSideBar"/> property.</summary>
    public static readonly StyledProperty<SideBar?> RightSideBarProperty =
        AvaloniaProperty.Register<ReDockHost, SideBar?>(nameof(RightSideBar));

    /// <summary>Defines the <see cref="MainContent"/> property.</summary>
    public static readonly StyledProperty<object?> MainContentProperty =
        AvaloniaProperty.Register<ReDockHost, object?>(nameof(MainContent));

    /// <summary>Defines the <see cref="MainContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> MainContentTemplateProperty =
        AvaloniaProperty.Register<ReDockHost, IDataTemplate?>(nameof(MainContentTemplate));

    /// <summary>Defines the <see cref="LeftSideBarWidth"/> property.</summary>
    public static readonly StyledProperty<double> LeftSideBarWidthProperty =
        AvaloniaProperty.Register<ReDockHost, double>(nameof(LeftSideBarWidth), 48.0);

    /// <summary>Defines the <see cref="RightSideBarWidth"/> property.</summary>
    public static readonly StyledProperty<double> RightSideBarWidthProperty =
        AvaloniaProperty.Register<ReDockHost, double>(nameof(RightSideBarWidth), 48.0);

    /// <summary>Defines the <see cref="IsFloatingEnabled"/> property.</summary>
    public static readonly StyledProperty<bool> IsFloatingEnabledProperty =
        AvaloniaProperty.Register<ReDockHost, bool>(nameof(IsFloatingEnabled));

    private Thumb? _leftThumb;
    private Thumb? _rightThumb;

    /// <summary>Gets or sets the left sidebar.</summary>
    public SideBar? LeftSideBar
    {
        get => GetValue(LeftSideBarProperty);
        set => SetValue(LeftSideBarProperty, value);
    }

    /// <summary>Gets or sets the right sidebar.</summary>
    public SideBar? RightSideBar
    {
        get => GetValue(RightSideBarProperty);
        set => SetValue(RightSideBarProperty, value);
    }

    /// <summary>Gets or sets the main content.</summary>
    public object? MainContent
    {
        get => GetValue(MainContentProperty);
        set => SetValue(MainContentProperty, value);
    }

    /// <summary>Gets or sets the main content template.</summary>
    public IDataTemplate? MainContentTemplate
    {
        get => GetValue(MainContentTemplateProperty);
        set => SetValue(MainContentTemplateProperty, value);
    }

    /// <summary>Gets or sets the width of the left sidebar.</summary>
    public double LeftSideBarWidth
    {
        get => GetValue(LeftSideBarWidthProperty);
        set => SetValue(LeftSideBarWidthProperty, value);
    }

    /// <summary>Gets or sets the width of the right sidebar.</summary>
    public double RightSideBarWidth
    {
        get => GetValue(RightSideBarWidthProperty);
        set => SetValue(RightSideBarWidthProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether floating is enabled.</summary>
    public bool IsFloatingEnabled
    {
        get => GetValue(IsFloatingEnabledProperty);
        set => SetValue(IsFloatingEnabledProperty, value);
    }

    /// <summary>Gets the available dock areas.</summary>
    public AvaloniaList<DockArea> DockAreas { get; } = [];

    /// <summary>Occurs when a button is moved between sidebars.</summary>
    public event EventHandler<SideBarButtonMoveEventArgs> ButtonMove
    {
        add => AddHandler(ButtonMoveEvent, value);
        remove => RemoveHandler(ButtonMoveEvent, value);
    }

    /// <summary>Occurs when a button's display mode changes.</summary>
    public event EventHandler<SideBarButtonDisplayModeChangedEventArgs> ButtonDisplayModeChanged
    {
        add => AddHandler(ButtonDisplayModeChangedEvent, value);
        remove => RemoveHandler(ButtonDisplayModeChangedEvent, value);
    }

    /// <summary>Occurs when a button flyout is requested.</summary>
    public event EventHandler<SideBarButtonFlyoutRequestedEventArgs> ButtonFlyoutRequested
    {
        add => AddHandler(ButtonFlyoutRequestedEvent, value);
        remove => RemoveHandler(ButtonFlyoutRequestedEvent, value);
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_leftThumb != null) _leftThumb.DragDelta -= OnLeftThumbDragDelta;
        if (_rightThumb != null) _rightThumb.DragDelta -= OnRightThumbDragDelta;

        _leftThumb = e.NameScope.Find<Thumb>("PART_LeftThumb");
        _rightThumb = e.NameScope.Find<Thumb>("PART_RightThumb");

        if (_leftThumb != null) _leftThumb.DragDelta += OnLeftThumbDragDelta;
        if (_rightThumb != null) _rightThumb.DragDelta += OnRightThumbDragDelta;
    }

    private void OnLeftThumbDragDelta(object? sender, VectorEventArgs e)
    {
        LeftSideBarWidth = Math.Clamp(LeftSideBarWidth + e.Vector.X, 32, Bounds.Width / 3);
    }

    private void OnRightThumbDragDelta(object? sender, VectorEventArgs e)
    {
        RightSideBarWidth = Math.Clamp(RightSideBarWidth - e.Vector.X, 32, Bounds.Width / 3);
    }

    /// <summary>Shows a flyout menu for the specified button.</summary>
    internal void ShowFlyout(SideBarButton button)
    {
        var args = new SideBarButtonFlyoutRequestedEventArgs(button, this, ButtonFlyoutRequestedEvent, this);
        RaiseEvent(args);
        if (args.Handled) return;

        var flyout = new SideBarButtonMenuFlyout(this);
        flyout.Placement = button.DockLocation?.LeftRight == SideBarLocation.Left
            ? PlacementMode.Right
            : PlacementMode.Left;
        flyout.ShowAt(button);
    }
}
