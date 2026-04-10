using System.Collections.Generic;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using AvaloniaFlyout = Avalonia.Controls.Flyout;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a single item inside a <see cref="BreadcrumbBar"/>.
/// </summary>
[TemplatePart(PART_LayoutRoot,                    typeof(Grid))]
[TemplatePart(PART_ItemButton,                    typeof(Button))]
[TemplatePart(PART_LastItemContentPresenter,      typeof(ContentPresenter), IsRequired = false)]
[TemplatePart(PART_ChevronTextBlock,              typeof(PathIcon),         IsRequired = false)]
[TemplatePart(PART_EllipsisDropDownItemPresenter, typeof(ContentPresenter), IsRequired = false)]
[PseudoClasses(PC_Inline, PC_Ellipsis, PC_LastItem, PC_EllipsisDropDown, PC_AllowClick, PC_Pressed)]
public class BreadcrumbBarItem : ContentControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_LayoutRoot                    = "PART_LayoutRoot";
    internal const string PART_ItemButton                    = "PART_ItemButton";
    internal const string PART_LastItemContentPresenter      = "PART_LastItemContentPresenter";
    internal const string PART_ChevronTextBlock              = "PART_ChevronTextBlock";
    internal const string PART_EllipsisDropDownItemPresenter = "PART_EllipsisDropDownItemPresenter";
    internal const string PART_EllipsisFlyoutKey             = "PART_EllipsisFlyout";
    internal const string PART_EllipsisItemsRepeater         = "PART_EllipsisItemsRepeater";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    public const string PC_Inline           = ":inline";
    public const string PC_Ellipsis         = ":ellipsis";
    public const string PC_LastItem         = ":lastItem";
    public const string PC_EllipsisDropDown = ":ellipsisDropDown";
    public const string PC_AllowClick       = ":allowClick";
    public const string PC_Pressed          = ":pressed";

    // ── Private state ─────────────────────────────────────────────────────────

    private Button?                      _button;
    private AvaloniaFlyout?              _ellipsisFlyout;
    private StackPanel?                  _flyoutPanel;
    private IDataTemplate?               _ellipsisDropDownItemDataTemplate;
    private BreadcrumbBarItem?           _ellipsisItem;
    private WeakReference<BreadcrumbBar>? _parentBreadcrumb;

    private int  _index;
    private bool _isEllipsisItem;
    private bool _isLastItem;
    private bool _allowClickOnLastItem;
    private bool _isPressed;
    private int  _trackedPointerId;
    private bool _keyDownHooked;

    // ── Public read-only state ────────────────────────────────────────────────

    /// <summary>Gets whether this item is rendered inside the ellipsis drop-down flyout.</summary>
    public bool IsEllipsisDropDownItem { get; private set; }

    // ── Template application ──────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DetachPartsListeners();

        if (_isEllipsisItem)
        {
            var root = e.NameScope.Find<Grid>(PART_LayoutRoot);
            _ellipsisFlyout = root?.Resources[PART_EllipsisFlyoutKey] as AvaloniaFlyout
                ?? throw new InvalidOperationException(
                    $"BreadcrumbBarItem: PART_LayoutRoot is missing a Flyout resource keyed '{PART_EllipsisFlyoutKey}'.");
        }

        _button = e.NameScope.Find<Button>(PART_ItemButton);
        if (_button is not null)
            _button.Loaded += OnButtonLoaded;

        UpdateButtonVisualState();
        UpdateInlineTypeVisualState();
        UpdateItemTypeVisualState();
    }

    protected override AutomationPeer OnCreateAutomationPeer()
        => new BreadcrumbBarItemAutomationPeer(this);

    // ── Pointer handling for drop-down items ──────────────────────────────────

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        if (IsEllipsisDropDownItem) ProcessPointerOver(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (IsEllipsisDropDownItem) ProcessPointerOver(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (IsEllipsisDropDownItem) ProcessPointerCanceled(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsEllipsisDropDownItem) return;
        if (IgnorePointerId(e.Pointer)) return;

        _isPressed = e.Pointer.Type == PointerType.Mouse
            ? e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
            : true;

        UpdateDropDownPressedState();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!IsEllipsisDropDownItem) return;
        if (IgnorePointerId(e.Pointer)) return;

        if (_isPressed)
        {
            _isPressed = false;
            UpdateDropDownPressedState();
            OnClickEvent(null, null);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (IsEllipsisDropDownItem)
            ProcessPointerCanceled(null, e.Pointer);
    }

    // ── Internal API called by BreadcrumbBar ──────────────────────────────────

    internal void SetParentBreadcrumb(BreadcrumbBar parent)
        => _parentBreadcrumb = new WeakReference<BreadcrumbBar>(parent);

    internal void SetIndex(int index) => _index = index;

    internal void SetIsEllipsisDropDownItem(bool value)
    {
        IsEllipsisDropDownItem = value;
        EnsureKeyDownHook();
        UpdateItemTypeVisualState();
    }

    internal void SetEllipsisDropDownItemDataTemplate(IDataTemplate? template)
    {
        _ellipsisDropDownItemDataTemplate = template;
        RebuildFlyoutItems();
    }

    internal void SetPropertiesForEllipsisItem()
    {
        _isEllipsisItem = true;
        _isLastItem     = false;
        InstantiateFlyout();
        UpdateButtonVisualState();
        UpdateInlineTypeVisualState();
    }

    internal void SetPropertiesForLastItem()
    {
        _isEllipsisItem = false;
        _isLastItem     = true;
        _allowClickOnLastItem = _parentBreadcrumb?.TryGetTarget(out var bar) == true
            && bar.IsLastItemClickEnabled;
        UpdateButtonVisualState();
        UpdateInlineTypeVisualState();
    }

    internal void ResetVisualProperties()
    {
        if (IsEllipsisDropDownItem)
        {
            UpdateDropDownPressedState();
            return;
        }

        _isEllipsisItem       = false;
        _isLastItem           = false;
        _allowClickOnLastItem = false;

        if (_button is not null)
            _button.Flyout = null;

        _ellipsisFlyout = null;
        _flyoutPanel    = null;

        UpdateButtonVisualState();
        UpdateInlineTypeVisualState();
    }

    internal void RaiseItemClickedEvent(object? content, int index)
    {
        if (_parentBreadcrumb?.TryGetTarget(out var bar) == true)
            bar.RaiseItemClickedEvent(content, index);
    }

    /// <summary>Programmatic click — used by automation and keyboard handling.</summary>
    internal void OnClickEvent(object? sender, RoutedEventArgs? args)
    {
        if (IsEllipsisDropDownItem)
        {
            _ellipsisItem?.CloseFlyout();
            _ellipsisItem?.RaiseItemClickedEvent(Content, _index - 1);
        }
        else if (_isEllipsisItem)
        {
            OnEllipsisItemClick(null, null);
        }
        else
        {
            OnBreadcrumbBarItemClick(null, null);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void OnButtonLoaded(object? sender, RoutedEventArgs e)
    {
        _button!.Loaded -= OnButtonLoaded;

        if (_isEllipsisItem)
            _button.Click += OnEllipsisItemClick;
        else
            _button.Click += OnBreadcrumbBarItemClick;

        if (_isEllipsisItem)
            SetPropertiesForEllipsisItem();
        else if (_isLastItem)
            SetPropertiesForLastItem();
        else
            ResetVisualProperties();
    }

    private void OnBreadcrumbBarItemClick(object? sender, RoutedEventArgs? e)
        => RaiseItemClickedEvent(Content, _index - 1);

    private void OnEllipsisItemClick(object? sender, RoutedEventArgs? e)
    {
        if (_parentBreadcrumb is null) return;
        if (!_parentBreadcrumb.TryGetTarget(out var bar)) return;

        var hidden = bar.HiddenElements();
        if (hidden is null) return;

        RebuildFlyoutItems(hidden);
        OpenFlyout();
    }

    private void InstantiateFlyout()
    {
        if (_button is null || _ellipsisFlyout is null) return;

        _flyoutPanel = new StackPanel
        {
            Spacing = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        AutomationProperties.SetName(_flyoutPanel, "EllipsisItemsRepeater");

        _ellipsisFlyout.Content   = _flyoutPanel;
        _ellipsisFlyout.Placement = PlacementMode.BottomEdgeAlignedLeft;
    }

    private void RebuildFlyoutItems(IEnumerable<object?>? items = null)
    {
        if (_flyoutPanel is null) return;

        _flyoutPanel.Children.Clear();

        IEnumerable<object?>? source = items;
        if (source is null && _parentBreadcrumb?.TryGetTarget(out var bar) == true)
            source = bar.HiddenElements();

        if (source is null) return;

        // Reverse so the most-recent hidden item appears at the top.
        var list = source.Reverse().ToList();
        int total = list.Count;

        for (int i = 0; i < total; i++)
        {
            var data = list[i];

            Control content;
            if (_ellipsisDropDownItemDataTemplate is not null && _ellipsisDropDownItemDataTemplate.Match(data))
                content = _ellipsisDropDownItemDataTemplate.Build(data)!;
            else
                content = new TextBlock { Text = data?.ToString() };

            var dropDownItem = new BreadcrumbBarItem
            {
                Content = content,
                DataContext = data
            };

            dropDownItem.SetIsEllipsisDropDownItem(true);
            dropDownItem._ellipsisItem = this;
            dropDownItem.SetIndex(total - i);

            AutomationProperties.SetPositionInSet(dropDownItem, i + 1);
            AutomationProperties.SetSizeOfSet(dropDownItem, total);

            _flyoutPanel.Children.Add(dropDownItem);
        }
    }

    private void OpenFlyout()  => _ellipsisFlyout?.ShowAt(this);
    internal void CloseFlyout() => _ellipsisFlyout?.Hide();

    // ── Visual state helpers ──────────────────────────────────────────────────

    private void UpdateItemTypeVisualState()
    {
        PseudoClasses.Set(PC_Inline,           !IsEllipsisDropDownItem);
        PseudoClasses.Set(PC_EllipsisDropDown,  IsEllipsisDropDownItem);
    }

    private void UpdateInlineTypeVisualState()
    {
        PseudoClasses.Set(PC_Ellipsis,   _isEllipsisItem);
        PseudoClasses.Set(PC_LastItem,   _isLastItem);
        PseudoClasses.Set(PC_AllowClick, _allowClickOnLastItem);
    }

    private void UpdateButtonVisualState()
    {
        if (_button?.Classes is not IPseudoClasses pc) return;
        pc.Set(PC_LastItem, _isLastItem && !_allowClickOnLastItem);
    }

    private void UpdateDropDownPressedState()
        => PseudoClasses.Set(PC_Pressed, _isPressed);

    // ── Pointer tracking ──────────────────────────────────────────────────────

    private void ProcessPointerOver(PointerEventArgs args)
    {
        if (IgnorePointerId(args.Pointer)) return;

        if (_isPressed)
        {
            bool inBounds = new Rect(Bounds.Size).Contains(args.GetPosition(this));
            PseudoClasses.Set(":pointerover", IsPointerOver && inBounds);
            PseudoClasses.Set(PC_Pressed,     _isPressed && inBounds);

            if (!inBounds)
                ProcessPointerCanceled(args);
        }
    }

    private void ProcessPointerCanceled(PointerEventArgs? args, IPointer? pointer = null)
    {
        if (IgnorePointerId(args?.Pointer ?? pointer)) return;
        _isPressed        = false;
        _trackedPointerId = 0;
        UpdateDropDownPressedState();
    }

    private bool IgnorePointerId(IPointer? pointer)
    {
        if (pointer is null) return false;
        int id = pointer.Id;
        if (_trackedPointerId == 0) { _trackedPointerId = id; return false; }
        return _trackedPointerId != id;
    }

    // ── Keyboard handling ─────────────────────────────────────────────────────

    private void EnsureKeyDownHook()
    {
        if (_keyDownHooked) return;
        AddHandler(KeyDownEvent, OnChildPreviewKeyDown, RoutingStrategies.Tunnel);
        _keyDownHooked = true;
    }

    private void OnChildPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (IsEllipsisDropDownItem)
        {
            if (e.Key is Key.Enter or Key.Space)
            {
                OnClickEvent(sender, null);
                e.Handled = true;
            }
        }
        else if (e.Key is Key.Enter or Key.Space)
        {
            if (_isEllipsisItem) OnEllipsisItemClick(null, null);
            else                 OnBreadcrumbBarItemClick(null, null);
            e.Handled = true;
        }
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    private void DetachPartsListeners()
    {
        if (_button is not null)
        {
            _button.Loaded -= OnButtonLoaded;
            _button.Click  -= OnEllipsisItemClick;
            _button.Click  -= OnBreadcrumbBarItemClick;
        }
    }
}
