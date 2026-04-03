using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// Wraps any content control and shows a lightweight confirmation popup
/// (with Confirm and Cancel actions) before executing a command.
/// Supports Click and/or Focus trigger modes.
/// </summary>
[TemplatePart(PART_ContentPresenter, typeof(ContentPresenter))]
[TemplatePart(PART_Popup,            typeof(Popup))]
[TemplatePart(PART_CloseButton,      typeof(Button))]
[TemplatePart(PART_ConfirmButton,    typeof(Button))]
[TemplatePart(PART_CancelButton,     typeof(Button))]
[PseudoClasses(PC_DropdownOpen)]
public class PopConfirm : ContentControl
{
    /// <summary>Template part name for the inner content presenter.</summary>
    public const string PART_ContentPresenter = "PART_ContentPresenter";
    /// <summary>Template part name for the confirmation popup.</summary>
    public const string PART_Popup            = "PART_Popup";
    /// <summary>Template part name for the close (×) button.</summary>
    public const string PART_CloseButton      = "PART_CloseButton";
    /// <summary>Template part name for the confirm button.</summary>
    public const string PART_ConfirmButton    = "PART_ConfirmButton";
    /// <summary>Template part name for the cancel button.</summary>
    public const string PART_CancelButton     = "PART_CancelButton";

    /// <summary>Pseudo-class applied while the popup is open.</summary>
    public const string PC_DropdownOpen = ":dropdownopen";

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="PopupHeader"/> property.</summary>
    public static readonly StyledProperty<object?> PopupHeaderProperty =
        AvaloniaProperty.Register<PopConfirm, object?>(nameof(PopupHeader));

    /// <summary>Defines the <see cref="PopupHeaderTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> PopupHeaderTemplateProperty =
        AvaloniaProperty.Register<PopConfirm, IDataTemplate?>(nameof(PopupHeaderTemplate));

    /// <summary>Defines the <see cref="PopupContent"/> property.</summary>
    public static readonly StyledProperty<object?> PopupContentProperty =
        AvaloniaProperty.Register<PopConfirm, object?>(nameof(PopupContent));

    /// <summary>Defines the <see cref="PopupContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> PopupContentTemplateProperty =
        AvaloniaProperty.Register<PopConfirm, IDataTemplate?>(nameof(PopupContentTemplate));

    /// <summary>Defines the <see cref="ConfirmCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> ConfirmCommandProperty =
        AvaloniaProperty.Register<PopConfirm, ICommand?>(nameof(ConfirmCommand));

    /// <summary>Defines the <see cref="CancelCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CancelCommandProperty =
        AvaloniaProperty.Register<PopConfirm, ICommand?>(nameof(CancelCommand));

    /// <summary>Defines the <see cref="ConfirmCommandParameter"/> property.</summary>
    public static readonly StyledProperty<object?> ConfirmCommandParameterProperty =
        AvaloniaProperty.Register<PopConfirm, object?>(nameof(ConfirmCommandParameter));

    /// <summary>Defines the <see cref="CancelCommandParameter"/> property.</summary>
    public static readonly StyledProperty<object?> CancelCommandParameterProperty =
        AvaloniaProperty.Register<PopConfirm, object?>(nameof(CancelCommandParameter));

    /// <summary>Defines the <see cref="TriggerMode"/> property.</summary>
    public static readonly StyledProperty<PopConfirmTriggerMode> TriggerModeProperty =
        AvaloniaProperty.Register<PopConfirm, PopConfirmTriggerMode>(nameof(TriggerMode), PopConfirmTriggerMode.Click);

    /// <summary>Defines the <see cref="HandleAsyncCommand"/> property.</summary>
    public static readonly StyledProperty<bool> HandleAsyncCommandProperty =
        AvaloniaProperty.Register<PopConfirm, bool>(nameof(HandleAsyncCommand), defaultValue: true);

    /// <summary>Defines the <see cref="IsDropdownOpen"/> property.</summary>
    public static readonly StyledProperty<bool> IsDropdownOpenProperty =
        AvaloniaProperty.Register<PopConfirm, bool>(nameof(IsDropdownOpen));

    /// <summary>Defines the <see cref="Placement"/> property.</summary>
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Popup.PlacementProperty.AddOwner<PopConfirm>(
            new StyledPropertyMetadata<PlacementMode>(defaultValue: PlacementMode.BottomEdgeAlignedLeft));

    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<PopConfirm, object?>(nameof(Icon));

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the header content shown inside the popup.</summary>
    public object? PopupHeader
    {
        get => GetValue(PopupHeaderProperty);
        set => SetValue(PopupHeaderProperty, value);
    }

    /// <summary>Gets or sets the data template for <see cref="PopupHeader"/>.</summary>
    public IDataTemplate? PopupHeaderTemplate
    {
        get => GetValue(PopupHeaderTemplateProperty);
        set => SetValue(PopupHeaderTemplateProperty, value);
    }

    /// <summary>Gets or sets the body content shown inside the popup.</summary>
    public object? PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    /// <summary>Gets or sets the data template for <see cref="PopupContent"/>.</summary>
    public IDataTemplate? PopupContentTemplate
    {
        get => GetValue(PopupContentTemplateProperty);
        set => SetValue(PopupContentTemplateProperty, value);
    }

    /// <summary>Gets or sets the command executed when the user confirms.</summary>
    public ICommand? ConfirmCommand
    {
        get => GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    /// <summary>Gets or sets the command executed when the user cancels.</summary>
    public ICommand? CancelCommand
    {
        get => GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="ConfirmCommand"/>.</summary>
    public object? ConfirmCommandParameter
    {
        get => GetValue(ConfirmCommandParameterProperty);
        set => SetValue(ConfirmCommandParameterProperty, value);
    }

    /// <summary>Gets or sets the parameter passed to <see cref="CancelCommand"/>.</summary>
    public object? CancelCommandParameter
    {
        get => GetValue(CancelCommandParameterProperty);
        set => SetValue(CancelCommandParameterProperty, value);
    }

    /// <summary>Gets or sets which interaction opens the popup.</summary>
    public PopConfirmTriggerMode TriggerMode
    {
        get => GetValue(TriggerModeProperty);
        set => SetValue(TriggerModeProperty, value);
    }

    /// <summary>
    /// When true, waits for async commands (INotifyPropertyChanged / IDisposable)
    /// to complete before closing the popup.
    /// </summary>
    public bool HandleAsyncCommand
    {
        get => GetValue(HandleAsyncCommandProperty);
        set => SetValue(HandleAsyncCommandProperty, value);
    }

    /// <summary>Gets or sets whether the confirmation popup is currently open.</summary>
    public bool IsDropdownOpen
    {
        get => GetValue(IsDropdownOpenProperty);
        set => SetValue(IsDropdownOpenProperty, value);
    }

    /// <summary>Gets or sets the popup placement relative to the wrapped content.</summary>
    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets an optional icon shown in the popup header area.
    /// When null the default warning icon is shown.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    // ── Private fields ────────────────────────────────────────────────────────

    private Button?      _closeButton;
    private Button?      _confirmButton;
    private Button?      _cancelButton;
    private Popup?       _popup;
    private IDisposable? _childChangeDisposable;
    private bool         _suppressButtonClickEvent;

    // ── Static constructor ────────────────────────────────────────────────────

    static PopConfirm()
    {
        IsDropdownOpenProperty.Changed.AddClassHandler<PopConfirm>((p, _) =>
            p.PseudoClasses.Set(PC_DropdownOpen, p.IsDropdownOpen));

        TriggerModeProperty.Changed.AddClassHandler<PopConfirm, PopConfirmTriggerMode>(
            (pop, args) => pop.OnTriggerModeChanged(args));
    }

    // ── Template ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Unsubscribe from previous parts
        if (_closeButton is not null)   _closeButton.Click   -= OnActionButtonClicked;
        if (_cancelButton is not null)  _cancelButton.Click  -= OnActionButtonClicked;
        if (_confirmButton is not null) _confirmButton.Click -= OnActionButtonClicked;

        _closeButton   = e.NameScope.Find<Button>(PART_CloseButton);
        _confirmButton = e.NameScope.Find<Button>(PART_ConfirmButton);
        _cancelButton  = e.NameScope.Find<Button>(PART_CancelButton);
        _popup         = e.NameScope.Find<Popup>(PART_Popup);

        if (_closeButton is not null)   _closeButton.Click   += OnActionButtonClicked;
        if (_cancelButton is not null)  _cancelButton.Click  += OnActionButtonClicked;
        if (_confirmButton is not null) _confirmButton.Click += OnActionButtonClicked;
    }

    /// <inheritdoc />
    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        bool result = base.RegisterContentPresenter(presenter);
        if (result)
            _childChangeDisposable = presenter
                .GetObservable(ContentPresenter.ChildProperty)
                .Subscribe(new AnonymousObserver<Control?>(OnChildChanged));
        return result;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _childChangeDisposable?.Dispose();
        base.OnDetachedFromLogicalTree(e);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void OnTriggerModeChanged(AvaloniaPropertyChangedEventArgs<PopConfirmTriggerMode> args)
    {
        var child = Presenter?.Child;
        TeardownChildEventSubscriptions(child);
        SetupChildEventSubscriptions(child, args.NewValue.Value);
    }

    private void OnChildChanged(Control? newChild)
    {
        // Called when the content presenter's child changes — rewire subscriptions
        SetupChildEventSubscriptions(newChild, TriggerMode);
    }

    private void SetupChildEventSubscriptions(Control? child, PopConfirmTriggerMode mode)
    {
        if (child is null) return;

        if (mode.HasFlag(PopConfirmTriggerMode.Click))
        {
            if (child is Button btn)
                btn.Click += OnMainButtonClicked;
            else
                child.AddHandler(PointerPressedEvent, OnMainElementPressed);
        }

        if (mode.HasFlag(PopConfirmTriggerMode.Focus))
        {
            child.AddHandler(InputElement.GotFocusEvent,  OnMainElementGotFocus);
            child.AddHandler(InputElement.LostFocusEvent, OnMainElementLostFocus);
        }
    }

    private void TeardownChildEventSubscriptions(Control? child)
    {
        if (child is null) return;
        child.RemoveHandler(PointerPressedEvent, OnMainElementPressed);
        if (child is Button btn2) btn2.Click -= OnMainButtonClicked;
        child.RemoveHandler(InputElement.GotFocusEvent,  OnMainElementGotFocus);
        child.RemoveHandler(InputElement.LostFocusEvent, OnMainElementLostFocus);
    }

    private void OnMainButtonClicked(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[PopConfirm] Main button clicked");
        if (!_suppressButtonClickEvent)
            SetCurrentValue(IsDropdownOpenProperty, !IsDropdownOpen);
        _suppressButtonClickEvent = false;
    }

    private void OnMainElementPressed(object? sender, PointerPressedEventArgs e)
        => SetCurrentValue(IsDropdownOpenProperty, !IsDropdownOpen);

    private void OnMainElementGotFocus(object? sender, RoutedEventArgs e)
    {
        Debug.WriteLine("[PopConfirm] Got focus");
        // Suppress the click that fires immediately after focus via keyboard
        if (TriggerMode.HasFlag(PopConfirmTriggerMode.Click) &&
            TriggerMode.HasFlag(PopConfirmTriggerMode.Focus))
            _suppressButtonClickEvent = true;

        SetCurrentValue(IsDropdownOpenProperty, true);
    }

    private void OnMainElementLostFocus(object? sender, RoutedEventArgs e)
    {
        // Don't close if focus moved inside the popup
        var newFocus = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (newFocus is Visual v && (_popup?.IsInsidePopup(v) ?? false)) return;
        SetCurrentValue(IsDropdownOpenProperty, false);
    }

    private void OnActionButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (!HandleAsyncCommand)
        {
            _popup?.SetCurrentValue(Popup.IsOpenProperty, false);
            return;
        }

        // Support async commands from MVVM Toolkit / Prism that implement
        // INotifyPropertyChanged — wait for CanExecuteChanged to fire twice
        // (IsRunning: true → false) before closing.
        if (sender is Button { Command: { } cmd and (INotifyPropertyChanged or IDisposable) } btn)
        {
            int count = 0;
            void OnCanExecuteChanged(object? _, EventArgs __)
            {
                count++;
                if (count < 2) return;
                if (cmd.CanExecute(btn.CommandParameter))
                    _popup?.SetCurrentValue(Popup.IsOpenProperty, false);
                cmd.CanExecuteChanged -= OnCanExecuteChanged;
            }
            cmd.CanExecuteChanged += OnCanExecuteChanged;
        }
        else
        {
            _popup?.SetCurrentValue(Popup.IsOpenProperty, false);
        }
    }
}
