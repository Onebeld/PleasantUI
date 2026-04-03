using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia.Data;
using Avalonia.VisualTree;

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

    /// <summary>
    /// Dictionary to store the original content for each button data context.
    /// This enables automatic content switching when buttons move between dock areas.
    /// </summary>
    private Dictionary<object, Control> _buttonContentMap = new();

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

    /// <summary>
    /// Registers content for a button's data context.
    /// This content will be automatically displayed in the appropriate target area when the button is toggled.
    /// </summary>
    /// <param name="dataContext">The button's data context</param>
    /// <param name="content">The content to display</param>
    public void RegisterButtonContent(object dataContext, Control content)
    {
        if (dataContext != null && content != null)
        {
            _buttonContentMap[dataContext] = content;
            Debug.WriteLine($"[ReDockHost] Registered content for {dataContext}");
        }
    }

    /// <summary>
    /// Handles button click/toggle to show/hide content in the appropriate target area.
    /// </summary>
    /// <param name="button">The button that was toggled</param>
    /// <param name="isVisible">Whether the button is now visible (checked)</param>
    internal void HandleButtonToggle(SideBarButton button, bool isVisible)
    {
        if (button.DataContext == null || button.DockLocation == null) return;

        // Find the dock area whose Location matches the button's current DockLocation.
        // A button at LeftUpperTop maps to the DockArea with Location=LeftUpperTop, etc.
        var dockArea = DockAreas.FirstOrDefault(da =>
            da.Location.ButtonLocation == button.DockLocation.ButtonLocation &&
            da.Location.LeftRight      == button.DockLocation.LeftRight);

        if (dockArea == null)
        {
            Debug.WriteLine($"[ReDockHost] HandleButtonToggle — no DockArea found for {button.DockLocation.ButtonLocation}/{button.DockLocation.LeftRight}");
            return;
        }

        // Resolve the target ContentPresenter inside the ReDock view.
        // dockArea.Target is a property name on ReDock ("LeftContent" / "RightContent").
        // The corresponding ContentPresenter template parts are named
        // "PART_LeftContentPresenter" and "PART_RightContentPresenter".
        var presenterName = dockArea.Target switch
        {
            "LeftContent"  => "PART_LeftContentPresenter",
            "RightContent" => "PART_RightContentPresenter",
            _              => null
        };

        if (presenterName == null)
        {
            Debug.WriteLine($"[ReDockHost] HandleButtonToggle — unknown target '{dockArea.Target}'");
            return;
        }

        var presenter = this.GetVisualDescendants()
            .OfType<ContentPresenter>()
            .FirstOrDefault(cp => cp.Name == presenterName);

        if (presenter == null)
        {
            Debug.WriteLine($"[ReDockHost] HandleButtonToggle — ContentPresenter '{presenterName}' not found in visual tree");
            return;
        }

        if (isVisible)
        {
            // Retrieve the registered content for this button's data context.
            if (!_buttonContentMap.TryGetValue(button.DataContext, out var content))
            {
                Debug.WriteLine($"[ReDockHost] HandleButtonToggle — no registered content for {button.DataContext}, using default");
                content = CreateDefaultContent(button.DataContext);
                _buttonContentMap[button.DataContext] = content;
            }

            // Detach content from any previous ContentPresenter to avoid
            // "already has a visual parent" error when moving between sidebars.
            if (content.Parent is ContentPresenter oldPresenter)
            {
                oldPresenter.Content = null;
                Debug.WriteLine($"[ReDockHost] HandleButtonToggle — detached content from previous presenter {oldPresenter.Name}");
            }

            // Place the content into the presenter and make it visible.
            // Setting Content replaces whatever was there before, so only one
            // button's content is shown per target area at a time.
            presenter.Content = content;
            presenter.IsVisible = true;
            Debug.WriteLine($"[ReDockHost] HandleButtonToggle — showing content for {button.DataContext} in {presenterName}");
        }
        else
        {
            // Hide the presenter only if it is currently showing THIS button's content.
            // If another button's content is already there, leave it alone.
            if (_buttonContentMap.TryGetValue(button.DataContext, out var myContent) &&
                ReferenceEquals(presenter.Content, myContent))
            {
                presenter.Content = null;
                presenter.IsVisible = false;
                Debug.WriteLine($"[ReDockHost] HandleButtonToggle — hiding {presenterName} (was showing {button.DataContext})");
            }
            else
            {
                Debug.WriteLine($"[ReDockHost] HandleButtonToggle — {presenterName} shows different content, not hiding");
            }
        }
    }

    /// <summary>
    /// Finds the content control for a target area name.
    /// Searches for the ContentPresenter template part that corresponds to the
    /// given ReDock property name ("LeftContent" → "PART_LeftContentPresenter", etc.).
    /// </summary>
    private ContentPresenter? FindTargetContentArea(string? targetName)
    {
        if (string.IsNullOrEmpty(targetName)) return null;

        var presenterName = targetName switch
        {
            "LeftContent"  => "PART_LeftContentPresenter",
            "RightContent" => "PART_RightContentPresenter",
            _              => null
        };

        if (presenterName == null) return null;

        return this.GetVisualDescendants()
            .OfType<ContentPresenter>()
            .FirstOrDefault(cp => cp.Name == presenterName);
    }

    /// <summary>
    /// Creates a default placeholder content control for a button that has no
    /// registered content. This is used as a fallback so the target area always
    /// shows something meaningful when toggled on.
    /// </summary>
    private static Control CreateDefaultContent(object dataContext)
    {
        var label = dataContext?.ToString() ?? "Panel";
        var content = new Border
        {
            Padding = new Thickness(12),
            Child = new TextBlock
            {
                Text = label,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Center,
                FontSize            = 13
            }
        };

        Debug.WriteLine($"[ReDockHost] CreateDefaultContent — created placeholder for '{label}'");
        return content;
    }
}
