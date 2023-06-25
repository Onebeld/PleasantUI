using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Extensions;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

[TemplatePart("PART_CloseButton", typeof(Button))]
[PseudoClasses(":dragging")]
public partial class PleasantTabItem : TabItem
{
    private bool _isClosing;
    private Button? _closeButton;

    public static readonly RoutedEvent<RoutedEventArgs> ClosingEvent =
        RoutedEvent.Register<PleasantTabItem, RoutedEventArgs>(nameof(Closing), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> CloseButtonClickEvent =
        RoutedEvent.Register<PleasantTabItem, RoutedEventArgs>(nameof(CloseButtonClick), RoutingStrategies.Bubble);

    /// <summary>
    ///     Defines the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(IsClosable), true);

    /// <summary>
    ///     Defines the <see cref="IsClosing" /> property.
    /// </summary>
    public static readonly DirectProperty<PleasantTabItem, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<PleasantTabItem, bool>(nameof(IsClosing), o => o.IsClosing);

    /// <summary>
    ///     Defines the <see cref="CanBeDragged" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanBeDraggedProperty =
        AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(CanBeDragged), true);

    public static readonly StyledProperty<bool> IsEditedIndicatorProperty =
        AvaloniaProperty.Register<PleasantTabItem, bool>(nameof(IsEditedIndicator));

    /// <summary>
    ///     This property sets if the PleasantTabItem can be closed
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    ///     Returns if the tab is closing.
    /// </summary>
    public bool IsClosing
    {
        get => _isClosing;
        set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }

    public bool CanBeDragged
    {
        get => GetValue(CanBeDraggedProperty);
        set => SetValue(CanBeDraggedProperty, value);
    }

    public bool IsEditedIndicator
    {
        get => GetValue(IsEditedIndicatorProperty);
        set => SetValue(IsEditedIndicatorProperty, value);
    }

    public event EventHandler<RoutedEventArgs> Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    public event EventHandler<RoutedEventArgs> CloseButtonClick
    {
        add => AddHandler(CloseButtonClickEvent, value);
        remove => RemoveHandler(CloseButtonClickEvent, value);
    }
    
    static PleasantTabItem()
    {
        CanBeDraggedProperty.Changed.AddClassHandler<PleasantTabItem>((x, e) =>
            x.OnCanDraggablePropertyChanged(x, e));
        IsSelectedProperty.Changed.AddClassHandler<PleasantTabItem>((x, _) => UpdatePseudoClass(x));
        IsClosableProperty.Changed.Subscribe(e =>
        {
            if (e.Sender is PleasantTabItem { _closeButton: not null } a) a._closeButton.IsVisible = a.IsClosable;
        });
    }

    /// <summary>
    ///     Is called before <see cref="PleasantTabItem.Closing" /> occurs
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnClosing(object sender, RoutedEventArgs e)
    {
        IsClosing = true;
    }

    private static void UpdatePseudoClass(PleasantTabItem item)
    {
        if (!item.IsSelected) item.PseudoClasses.Remove(":dragging");
    }

    public bool CloseCore()
    {
        TabControl x = (Parent as TabControl)!;
        try
        {
            if (this.DataContext is not null)
            {
                x.CloseTab(this.DataContext);
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
    ///     Close the Tab
    /// </summary>
    public bool Close()
    {
        RaiseEvent(new RoutedEventArgs(ClosingEvent));
        if (IsClosing)
            return CloseCore();
        return false;
    }

    protected void OnCanDraggablePropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (CanBeDragged)
        {
            case true:
                PseudoClasses.Add(":lockdrag");
                break;
            case false:
                PseudoClasses.Remove(":lockdrag");
                break;
        }
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

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        Close();
    }
}