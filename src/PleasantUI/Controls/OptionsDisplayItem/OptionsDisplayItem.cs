using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a control that displays an item with a header, content, icon, and optional navigation functionality.
/// </summary>
[TemplatePart("LayoutRoot", typeof(Border))]
public class OptionsDisplayItem : TemplatedControl
{
    private bool _isPressed;
    private Border? _layoutRoot;
    
    /// <summary>
    /// Defines the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, string>(nameof(Header));

    /// <summary>
    /// Defines the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, string>(nameof(Description));

    /// <summary>
    /// Defines the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, Geometry>(nameof(Icon));

    /// <summary>
    /// Defines the <see cref="Navigates" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> NavigatesProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, bool>(nameof(Navigates));

    /// <summary>
    /// Defines the <see cref="ActionButton" /> property.
    /// </summary>
    public static readonly StyledProperty<Control> ActionButtonProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, Control>(nameof(ActionButton));

    /// <summary>
    /// Defines the <see cref="Expands" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ExpandsProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, bool>(nameof(Expands));

    /// <summary>
    /// Defines the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<OptionsDisplayItem>();

    /// <summary>
    /// Defines the <see cref="NavigationCommand" /> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> NavigationCommandProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, ICommand?>(nameof(NavigationCommand));

    /// <summary>
    /// Defines the <see cref="IsExpanded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        Expander.IsExpandedProperty.AddOwner<OptionsDisplayItem>();

    /// <summary>
    /// Defines the <see cref="NavigationRequestedEvent" /> property.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> NavigationRequestedEvent =
        RoutedEvent.Register<OptionsDisplayItem, RoutedEventArgs>(nameof(NavigationRequested),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Gets or sets the header property.
    /// </summary>
    /// <value>
    /// The header value.
    /// </value>
    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the description of the property.
    /// </summary>
    /// <value>
    /// The description of the property.
    /// </value>
    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon geometry.
    /// </summary>
    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control navigates.
    /// </summary>
    /// <value>
    /// <c>true</c> if the control navigates; otherwise, <c>false</c>.
    /// </value>
    public bool Navigates
    {
        get => GetValue(NavigatesProperty);
        set => SetValue(NavigatesProperty, value);
    }

    /// <summary>
    /// Gets or sets the action button control.
    /// </summary>
    /// <value>
    /// The action button control.
    /// </value>
    public Control ActionButton
    {
        get => GetValue(ActionButtonProperty);
        set => SetValue(ActionButtonProperty, value);
    }

    /// <summary>
    /// Specifies that <see cref="OptionsDisplayItem"/> will reveal hidden content
    /// </summary>
    public bool Expands
    {
        get => GetValue(ExpandsProperty);
        set => SetValue(ExpandsProperty, value);
    }

    /// <summary>
    /// Gets or sets the content of the property.
    /// </summary>
    /// <value>
    /// The content of the property.
    /// </value>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Indicates whether the <see cref="OptionsDisplayItem"/> content is open 
    /// </summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to be executed for navigation.
    /// </summary>
    /// <value>
    /// The navigation command.
    /// </value>
    public ICommand? NavigationCommand
    {
        get => GetValue(NavigationCommandProperty);
        set => SetValue(NavigationCommandProperty, value);
    }

    /// <summary>
    /// Event that is raised when a navigation is requested.
    /// </summary>
    /// <remarks>
    /// This event can be used to handle navigation requests in a component.
    /// </remarks>
    public event EventHandler<RoutedEventArgs> NavigationRequested
    {
        add => AddHandler(NavigationRequestedEvent, value);
        remove => RemoveHandler(NavigationRequestedEvent, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NavigatesProperty)
        {
            if (Expands)
                throw new InvalidOperationException("Control cannot both Navigate and Expand");

            PseudoClasses.Set(":navigates", (bool?)change.NewValue ?? false);
        }
        else if (change.Property == ExpandsProperty)
        {
            if (Navigates)
                throw new InvalidOperationException("Control cannot both Navigate and Expand");

            PseudoClasses.Set(":expands", (bool?)change.NewValue ?? false);
        }
        else if (change.Property == IsExpandedProperty)
        {
            PseudoClasses.Set(":expanded", (bool?)change.NewValue ?? false);
        }
        else if (change.Property == IconProperty)
        {
            PseudoClasses.Set(":icon", change.NewValue != null);
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _layoutRoot = e.NameScope.Find<Border>("LayoutRoot")!;
        _layoutRoot.PointerPressed += OnLayoutRootPointerPressed;
        _layoutRoot.PointerReleased += OnLayoutRootPointerReleased;
        _layoutRoot.PointerCaptureLost += OnLayoutRootPointerCaptureLost;
    }

    private void OnLayoutRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
            return;

        _isPressed = true;
        PseudoClasses.Set(":pressed", true);
    }

    private void OnLayoutRootPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        PointerPoint pt = e.GetCurrentPoint(this);
        if (!_isPressed || pt.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
            return;

        _isPressed = false;

        PseudoClasses.Set(":pressed", false);

        if (Expands)
            IsExpanded = !IsExpanded;

        if (!Navigates)
            return;

        RaiseEvent(new RoutedEventArgs(NavigationRequestedEvent, this));
        NavigationCommand?.Execute(null);
    }

    private void OnLayoutRootPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _isPressed = false;
        PseudoClasses.Set(":pressed", false);
    }
}