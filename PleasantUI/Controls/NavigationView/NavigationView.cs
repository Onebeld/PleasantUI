﻿using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using PleasantUI.Core;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

[PseudoClasses(":normal", ":compact")]
public class NavigationView : TreeView
{
    private object? _selectedContent;
    private IEnumerable<string>? _itemsAsStrings;
    private bool _headerVisible;
    private AutoCompleteBox? _autoCompleteBox;
    private ICommand? _backButtonCommand;

    private PleasantWindow? _host;
    private CompositeDisposable? _disposable;

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<NavigationView, object?>(nameof(Header), "Header");

    public static readonly StyledProperty<Geometry> IconProperty =
        AvaloniaProperty.Register<NavigationView, Geometry>(nameof(Icon));

    public static readonly DirectProperty<NavigationView, object?> SelectedContentProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, object?>(
            nameof(SelectedContent),
            o => o.SelectedContent);

    public static readonly StyledProperty<IDataTemplate> SelectedContentTemplateProperty =
        AvaloniaProperty.Register<NavigationView, IDataTemplate>(nameof(SelectedContentTemplate));

    public static readonly StyledProperty<double> CompactPaneLengthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(CompactPaneLength));

    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(OpenPaneLength));

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(IsOpen));

    public static readonly StyledProperty<bool> DynamicDisplayModeProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(DynamicDisplayMode), true);

    public static readonly StyledProperty<bool> BindWindowSettingsProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(BindWindowSettings), true);

    public static readonly StyledProperty<Animations?> TransitionAnimationsProperty =
        AvaloniaProperty.Register<NavigationView, Animations?>(nameof(TransitionAnimations));

    public static readonly StyledProperty<SplitViewDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<NavigationView, SplitViewDisplayMode>(nameof(DisplayMode),
            SplitViewDisplayMode.CompactInline);

    public static readonly StyledProperty<bool> AlwaysOpenProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(AlwaysOpen));
    
    public static readonly StyledProperty<bool> DisplayTopIndentProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(DisplayTopIndent), true);

    public static readonly StyledProperty<bool> NotMakeOffsetForContentPanelProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(NotMakeOffsetForContentPanel));

    public static readonly StyledProperty<bool> AutoCompleteBoxIsVisibleProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(AutoCompleteBoxIsVisible), true);

    public static readonly DirectProperty<NavigationView, IEnumerable<string>?> ItemsAsStringsProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, IEnumerable<string>?>(nameof(ItemsAsStrings),
            o => o.ItemsAsStrings);

    public static readonly StyledProperty<bool> IsFloatingHeaderProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(IsFloatingHeader));

    public static readonly DirectProperty<NavigationView, bool> HeaderVisibleProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, bool>(nameof(HeaderVisible), o => o.HeaderVisible);

    public static readonly DirectProperty<NavigationView, AutoCompleteBox?> AutoCompleteBoxProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, AutoCompleteBox?>(nameof(AutoCompleteBox),
            o => o.AutoCompleteBox, (o, v) => o.AutoCompleteBox = v);

    /// <summary>
    /// Defines the <see cref="ICommand"/> property.
    /// </summary>
    public static readonly DirectProperty<NavigationView, ICommand?> BackButtonCommandProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, ICommand?>(nameof(BackButtonCommand),
            navigationView => navigationView.BackButtonCommand, (navigationView, command) => navigationView.BackButtonCommand = command, enableDataValidation: true);

    public static readonly StyledProperty<bool> ShowBackButtonProperty =
        AvaloniaProperty.Register<NavigationView, bool>(nameof(ShowBackButton));
    
    private CancellationTokenSource? _cancellationTokenSource;

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public Geometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object? SelectedContent
    {
        get => _selectedContent;
        private set => SetAndRaise(SelectedContentProperty, ref _selectedContent, value);
    }

    public IDataTemplate SelectedContentTemplate
    {
        get => GetValue(SelectedContentTemplateProperty);
        set => SetValue(SelectedContentTemplateProperty, value);
    }

    public double CompactPaneLength
    {
        get => GetValue(CompactPaneLengthProperty);
        set => SetValue(CompactPaneLengthProperty, value);
    }

    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public bool AlwaysOpen
    {
        get => GetValue(AlwaysOpenProperty);
        set => SetValue(AlwaysOpenProperty, value);
    }

    public bool DisplayTopIndent
    {
        get => GetValue(DisplayTopIndentProperty);
        set => SetValue(DisplayTopIndentProperty, value);
    }

    public bool NotMakeOffsetForContentPanel
    {
        get => GetValue(NotMakeOffsetForContentPanelProperty);
        set => SetValue(NotMakeOffsetForContentPanelProperty, value);
    }

    public bool ShowBackButton
    {
        get => GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    public SplitViewDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public bool AutoCompleteBoxIsVisible
    {
        get => GetValue(AutoCompleteBoxIsVisibleProperty);
        set => SetValue(AutoCompleteBoxIsVisibleProperty, value);
    }

    public Animations? TransitionAnimations
    {
        get => GetValue(TransitionAnimationsProperty);
        set => SetValue(TransitionAnimationsProperty, value);
    }

    public IEnumerable<string>? ItemsAsStrings
    {
        get => _itemsAsStrings;
        private set
        {
            SetAndRaise(ItemsAsStringsProperty, ref _itemsAsStrings, value);
        }
    }

    public bool DynamicDisplayMode
    {
        get => GetValue(DynamicDisplayModeProperty);
        set => SetValue(DynamicDisplayModeProperty, value);
    }

    public bool IsFloatingHeader
    {
        get => GetValue(IsFloatingHeaderProperty);
        set => SetValue(IsFloatingHeaderProperty, value);
    }

    public bool HeaderVisible
    {
        get => _headerVisible;
        private set => SetAndRaise(HeaderVisibleProperty, ref _headerVisible, value);
    }

    public AutoCompleteBox? AutoCompleteBox
    {
        get => _autoCompleteBox;
        set => SetAndRaise(AutoCompleteBoxProperty, ref _autoCompleteBox, value);
    }

    public bool BindWindowSettings
    {
        get => GetValue(BindWindowSettingsProperty);
        set => SetValue(BindWindowSettingsProperty, value);
    }

    /// <summary>
    /// Gets or sets an <see cref="ICommand"/> to be invoked when the button is clicked.
    /// </summary>
    public ICommand? BackButtonCommand
    {
        get => _backButtonCommand;
        set => SetAndRaise(BackButtonCommandProperty, ref _backButtonCommand, value);
    }
    
    private Button? _headerItem;
    private Button? _backButton;
    private ContentPresenter? _contentPresenter;
    private SplitView? _splitView;
    private const double LittleWidth = 1005;
    private const double VeryLittleWidth = 650;

    static NavigationView()
    {
        SelectionModeProperty.OverrideDefaultValue<NavigationView>(SelectionMode.Single);
        SelectedItemProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.OnSelectedItemChanged());
        FocusableProperty.OverrideDefaultValue<NavigationView>(true);
        IsOpenProperty.Changed.AddClassHandler<NavigationView>((x, _) => x.OnIsOpenChanged());
        IsFloatingHeaderProperty.Changed.Subscribe(x =>
        {
            if (x.Sender is NavigationView navigationView)
                navigationView.UpdateHeaderVisibility();
        });
    }

    public NavigationView()
    {
        PseudoClasses.Add(":normal");
        this.GetObservable(BoundsProperty).Subscribe(bounds =>
        {
            Dispatcher.UIThread.InvokeAsync(() => OnBoundsChanged(bounds));
        });
    }

    private void OnBoundsChanged(Rect rect)
    {
        if (DynamicDisplayMode)
        {
            bool isLittle = rect.Width <= LittleWidth;
            bool isVeryLittle = rect.Width <= VeryLittleWidth;

            if (!isLittle && !isVeryLittle)
            {
                UpdatePseudoClasses(false);
                DisplayMode = SplitViewDisplayMode.CompactInline;
            }
            else if (isLittle && !isVeryLittle)
            {
                UpdatePseudoClasses(false);
                DisplayMode = SplitViewDisplayMode.CompactOverlay;
                IsOpen = false;
                foreach (NavigationViewItemBase navigationViewItemBase in this.GetLogicalDescendants().OfType<NavigationViewItemBase>())
                {
                    navigationViewItemBase.IsExpanded = false;
                }
            }
            else if (isLittle && isVeryLittle)
            {
                UpdatePseudoClasses(true);
                DisplayMode = SplitViewDisplayMode.Overlay;
                IsOpen = false;
                foreach (NavigationViewItemBase navigationViewItemBase in this.GetLogicalDescendants().OfType<NavigationViewItemBase>())
                {
                    navigationViewItemBase.IsExpanded = false;
                }
            }
        }
    }

    internal void SelectSingleItemCore(object? item)
    {
        if (SelectedItem != item && TransitionAnimations is not null && _contentPresenter is not null)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            
            foreach (Animation animation in TransitionAnimations)
                animation.RunAsync(_contentPresenter, _cancellationTokenSource.Token);
        }

        if (SelectedItem is ISelectable selectableSelectedItem)
            selectableSelectedItem.IsSelected = false;

        if (item is ISelectable selectableItem)
            selectableItem.IsSelected = true;

        SelectedItems.Clear();
        SelectedItems.Add(item);

        SelectedItem = item;
    }

    internal void SelectSingleItem(object item)
    {
        SelectSingleItemCore(item);
    }

    private void UpdateHeaderVisibility() => HeaderVisible = IsOpen | IsFloatingHeader;

    private void OnSelectedItemChanged()
    {
        UpdateTitleAndSelectedContent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _headerItem = e.NameScope.Find<Button>("PART_HeaderItem");
        _splitView = e.NameScope.Find<SplitView>("split");
        _backButton = e.NameScope.Find<Button>("PART_BackButton");
        _contentPresenter = e.NameScope.Find<ContentPresenter>("PART_SelectedContentPresenter");

        if (_headerItem != null)
            _headerItem.Click += delegate
            {
                if (!AlwaysOpen)
                    IsOpen = !IsOpen;
                else
                    IsOpen = true;
            };

        if (VisualRoot is PleasantWindow pleasantWindow)
        {
            _host = pleasantWindow;
            
            Attach();
        }

        BackButtonCommandProperty.Changed.Subscribe(x =>
        {
            if (_backButton is not null)
                _backButton.IsVisible = x.NewValue.Value is not null;
        });

        UpdateTitleAndSelectedContent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);

        if (Items is IList { Count: >= 1 } l && l[0] is ISelectable s)
            SelectSingleItem(s);
    }

    private void OnIsOpenChanged()
    {
        UpdateHeaderVisibility();
    }

    private void UpdatePseudoClasses(bool isCompact)
    {
        switch (isCompact)
        {
            case true:
                PseudoClasses.Add(":compact");
                break;
            case false:
                PseudoClasses.Remove(":compact");
                break;
        }
    }

    private void UpdateTitleAndSelectedContent()
    {
        if (SelectedItem is not NavigationViewItemBase item) return;

        if (item.Content is not null)
        {
            SelectedContent = item.Content;
            return;
        }

        SelectedContent = item.FuncControl.Invoke();
    }

    private void Attach()
    {
        if (_host is null) return;
        
        _disposable = new CompositeDisposable
        {
            this.GetObservable(DisplayModeProperty).Subscribe(displayMode =>
            {
                if (displayMode is SplitViewDisplayMode.Overlay 
                    && !_host.EnableTitleBarMargin 
                    && ShowBackButton)
                {
                    _host.LeftTitleContent = new Panel
                    {
                        Width = 62
                    };
                }
                else if (!_host.EnableTitleBarMargin)
                {
                    _host.LeftTitleContent = new Panel
                    {
                        Width = 45
                    };
                }
                else
                {
                    _host.LeftTitleContent = null;
                }
            })
        };
    }
    
    private void Detach()
    {
        if (_disposable is null) return;
        
        _disposable.Dispose();
        _disposable = null;
        _host = null;
    }
}