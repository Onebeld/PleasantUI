using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using PleasantUI.Core.Enums;

namespace PleasantUI.Controls;

[TemplatePart("PART_ScrollViewer", typeof(SmoothScrollViewer))]
[TemplatePart("PART_AdderButton", typeof(Button))]
[TemplatePart("PART_InternalGrid", typeof(Grid))]
public class PleasantTabView : TabControl
{
    /// <summary>
    /// Defines the <see cref="ClickOnAddingButton" /> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickOnAddingButtonEvent =
        RoutedEvent.Register<PleasantTabView, RoutedEventArgs>(nameof(ClickOnAddingButton),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="AdderButtonIsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AdderButtonIsVisibleProperty =
        AvaloniaProperty.Register<PleasantTabView, bool>(nameof(AdderButtonIsVisible), true);

    /// <summary>
    /// Defines the <see cref="MaxWidthOfItemsPresenter" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxWidthOfItemsPresenterProperty =
        AvaloniaProperty.Register<PleasantTabView, double>(nameof(MaxWidthOfItemsPresenter),
            double.PositiveInfinity);

    public static readonly StyledProperty<IBrush> SecondaryBackgroundProperty =
        AvaloniaProperty.Register<PleasantTabView, IBrush>(nameof(SecondaryBackground));

    /// <summary>
    /// </summary>
    public static readonly StyledProperty<Thickness> ItemsMarginProperty =
        AvaloniaProperty.Register<PleasantTabView, Thickness>(nameof(ItemsMargin));

    /// <summary>
    /// Defines the <see cref="HeightRemainingSpace" /> property.
    /// </summary>
    public static readonly DirectProperty<PleasantTabView, double> HeightRemainingSpaceProperty =
        AvaloniaProperty.RegisterDirect<PleasantTabView, double>(
            nameof(HeightRemainingSpace),
            o => o.HeightRemainingSpace);

    /// <summary>
    /// Defines the <see cref="WidthRemainingSpace" /> property.
    /// </summary>
    public static readonly DirectProperty<PleasantTabView, double> WidthRemainingSpaceProperty =
        AvaloniaProperty.RegisterDirect<PleasantTabView, double>(
            nameof(WidthRemainingSpace),
            o => o.WidthRemainingSpace);

    public static readonly StyledProperty<TabViewMarginType> MarginTypeProperty =
        AvaloniaProperty.Register<PleasantTabView, TabViewMarginType>(nameof(MarginType));

    private Grid? _grid;
    private double _heightRemainingSpace;
    private double _widthRemainingSpace;

    public Button? AdderButton;

    static PleasantTabView()
    {
        SelectionModeProperty.OverrideDefaultValue<PleasantTabView>(SelectionMode.Single);
    }

    /// <summary>
    /// This property defines if the AdderButton can be visible, the default value is true.
    /// </summary>
    public bool AdderButtonIsVisible
    {
        get => GetValue(AdderButtonIsVisibleProperty);
        set => SetValue(AdderButtonIsVisibleProperty, value);
    }

    /// <summary>
    /// This property defines what is the maximum width of the ItemsPresenter.
    /// </summary>
    public double MaxWidthOfItemsPresenter
    {
        get => GetValue(MaxWidthOfItemsPresenterProperty);
        set => SetValue(MaxWidthOfItemsPresenterProperty, value);
    }

    /// <summary>
    /// Gets or Sets the SecondaryBackground.
    /// </summary>
    public IBrush SecondaryBackground
    {
        get => GetValue(SecondaryBackgroundProperty);
        set => SetValue(SecondaryBackgroundProperty, value);
    }

    /// <summary>
    /// Sets the margin of the items presenter
    /// </summary>
    public Thickness ItemsMargin
    {
        get => GetValue(ItemsMarginProperty);
        set => SetValue(ItemsMarginProperty, value);
    }

    /// <summary>
    /// Gets the space that remains in the top
    /// </summary>
    public double HeightRemainingSpace
    {
        get => _heightRemainingSpace;
        private set => SetAndRaise(HeightRemainingSpaceProperty, ref _heightRemainingSpace, value);
    }

    /// <summary>
    /// Gets the space that remains in the top.
    /// </summary>
    public double WidthRemainingSpace
    {
        get => _widthRemainingSpace;
        private set => SetAndRaise(WidthRemainingSpaceProperty, ref _widthRemainingSpace, value);
    }

    public TabViewMarginType MarginType
    {
        get => GetValue(MarginTypeProperty);
        set => SetValue(MarginTypeProperty, value);
    }

    /// <summary>
    /// It's raised when the adder button is clicked
    /// </summary>
    public event EventHandler<RoutedEventArgs> ClickOnAddingButton
    {
        add => AddHandler(ClickOnAddingButtonEvent, value);
        remove => RemoveHandler(ClickOnAddingButtonEvent, value);
    }


    protected void AdderButtonClicked(object? sender, RoutedEventArgs e)
    {
        RoutedEventArgs routedEventArgs = new(ClickOnAddingButtonEvent);
        RaiseEvent(routedEventArgs);
        routedEventArgs.Handled = true;
    }

    /// <inheritdoc />
    protected override async void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectedItemProperty && SelectedItem == null)
        {
            await Task.Delay(100);

            double d = ItemCount * 0.5;
            if ((d > 0) & (ItemCount != 0))
                SelectedItem = Items.OfType<object>().FirstOrDefault();
            else if ((d <= 0) & (ItemCount != 0))
                SelectedItem = Items.OfType<object>().LastOrDefault();
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        AdderButton = e.NameScope.Find<Button>("PART_AdderButton");

        if (AdderButton != null) AdderButton.Click += AdderButtonClicked;
        _grid = e.NameScope.Find<Grid>("PART_InternalGrid");

        PropertyChanged += PleasantTabView_PropertyChanged;
    }

    private void PleasantTabView_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_grid is null) return;

        WidthRemainingSpace = _grid.Bounds.Width;
        HeightRemainingSpace = _grid.Bounds.Height;
    }
}