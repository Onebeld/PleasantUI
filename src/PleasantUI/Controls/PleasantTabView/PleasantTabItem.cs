using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Core.Extensions;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// An item in a <see cref="PleasantTabView" />.
/// </summary>
/// <remarks>
/// Reference: https://github.com/PieroCastillo/Aura.UI/blob/master/src/Aura.UI/Controls/AuraTabItem/AuraTabItem.cs
/// </remarks>
[TemplatePart("PART_CloseButton", typeof(Button))]
public class PleasantTabItem : TabItem
{
    private Button? _closeButton;
    private bool _isClosing;
    
    /// <summary>
    /// Defines the <see cref="ClosingEvent" /> property.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClosingEvent =
        RoutedEvent.Register<PleasantTabItem, RoutedEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="CloseButtonClickEvent" /> property.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> CloseButtonClickEvent =
        RoutedEvent.Register<PleasantTabItem, RoutedEventArgs>(nameof(CloseButtonClick), RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(IsClosable), true);

    /// <summary>
    /// Defines the <see cref="IsClosing" /> property.
    /// </summary>
    public static readonly DirectProperty<PleasantTabItem, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<PleasantTabItem, bool>(nameof(IsClosing), o => o.IsClosing);

    /// <summary>
    /// Defines the <see cref="IsEditedIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsEditedIndicatorProperty =
        AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(IsEditedIndicator));

    /// <summary>
    /// This property sets if the PleasantTabItem can be closed
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Returns if the tab is closing.
    /// </summary>
    public bool IsClosing
    {
        get => _isClosing;
        set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }

    /// <summary>
    /// Gets or sets whether the tab has been edited and has an indicator.
    /// </summary>
    public bool IsEditedIndicator
    {
        get => GetValue(IsEditedIndicatorProperty);
        set => SetValue(IsEditedIndicatorProperty, value);
    }

    /// <summary>
    /// Occurs when the <see cref="Close"/> method is called.
    /// </summary>
    public event EventHandler<RoutedEventArgs> Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    /// <summary>
    /// Occurs when the tab is clicked.
    /// </summary>
    public event EventHandler<RoutedEventArgs> CloseButtonClick
    {
        add => AddHandler(CloseButtonClickEvent, value);
        remove => RemoveHandler(CloseButtonClickEvent, value);
    }
    
    static PleasantTabItem()
    {
        IsSelectedProperty.Changed.AddClassHandler<PleasantTabItem>((x, _) => UpdatePseudoClass(x));
        IsClosableProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is PleasantTabItem { _closeButton: not null } a) a._closeButton.IsVisible = a.IsClosable;
        });
    }

    /// <summary>
    /// Is called before <see cref="PleasantTabItem.Closing" /> occurs
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnClosing(object sender, RoutedEventArgs e)
    {
        IsClosing = true;
    }

    /// <summary>
    /// Attempts to close the tab item by calling <see cref="PleasantTabView.CloseTab(object)"/> with
    /// the <see cref="DataContext"/>. If that fails, it calls <see cref="PleasantTabView.CloseTab(PleasantTabItem)"/>
    /// with this tab item as the argument.
    /// </summary>
    /// <returns><see langword="true"/> if the tab item was successfully closed, <see langword="false"/> otherwise.</returns>
    public bool CloseCore()
    {
        PleasantTabView x = (Parent as PleasantTabView)!;
        try
        {
            if (DataContext is not null)
            {
                x.CloseTab(DataContext);
                return true;
            }

            x.CloseTab(this);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Close the Tab
    /// </summary>
    public bool Close()
    {
        RaiseEvent(new RoutedEventArgs(ClosingEvent));
        return IsClosing && CloseCore();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");

        if (_closeButton is null) return;

        if (IsClosable)
            _closeButton.Click += CloseButton_Click;
        else
            _closeButton.IsVisible = false;
    }
    
    private static void UpdatePseudoClass(PleasantTabItem item)
    {
        if (!item.IsSelected) item.PseudoClasses.Remove(":dragging");
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        Close();
    }
}