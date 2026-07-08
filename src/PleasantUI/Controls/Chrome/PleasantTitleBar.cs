using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Reactive;
using PleasantUI.Core.Internal.Reactive;
using Path = Avalonia.Controls.Shapes.Path;

namespace PleasantUI.Controls.Chrome;

/// <summary>
/// Represents the title bar of a <see cref="PleasantWindow" />.
/// </summary>
/// <remarks>
/// This control provides the standard title bar functionality, including window dragging, caption buttons, and
/// title/subtitle display.
/// It uses a template to define its visual structure and interacts with <see cref="PleasantWindow" /> for
/// window-related operations.
/// </remarks>
[TemplatePart("PART_PleasantCaptionButtons", typeof(PleasantCaptionButtons))]
[TemplatePart("PART_CloseMenuItem", typeof(MenuItem))]
[TemplatePart("PART_ExpandMenuItem", typeof(MenuItem))]
[TemplatePart("PART_ReestablishMenuItem", typeof(MenuItem))]
[TemplatePart("PART_CollapseMenuItem", typeof(MenuItem))]
[TemplatePart("PART_DragWindow", typeof(Border))]
[TemplatePart("PART_Icon", typeof(Image))]
[TemplatePart("PART_Title", typeof(TextBlock))]
[TemplatePart("PART_Subtitle", typeof(TextBlock))]
[TemplatePart("PART_LogoPath", typeof(Path))]
[TemplatePart("PART_LeftTitleBarContent", typeof(ContentPresenter))]
[TemplatePart("PART_TitleBarContent", typeof(ContentPresenter))]
[TemplatePart("PART_TitlePanel", typeof(StackPanel))]
[PseudoClasses(":active", ":minimized", ":normal", ":maximized", ":isactive", ":titlebar", ":title-visible", ":compact-titlebar")]
public class PleasantTitleBar : TemplatedControl
{
    /// <summary>
    /// Defines the title bar type for the window
    /// </summary>
    public enum Type
    {
        /// <summary>
        /// Regular title bar
        /// </summary>
        Classic = 0,

        /// <summary>
        /// The title bar is slightly larger than usual
        /// </summary>
        ClassicExtended = 1,
        
        NavigationViewClassicExtended = 2,

        /// <summary>
        /// A compact title bar that takes up minimal vertical space
        /// </summary>
        Compact = 3
    }
    
    private PleasantWindow? _host;
    private PleasantCaptionButtons? _captionButtons;

    private Grid? _titleBarGrid;

    private MenuItem? _closeMenuItem;
    private MenuItem? _collapseMenuItem;

    private Border? _dragWindowBorder;
    private MenuItem? _expandMenuItem;

    private PleasantIcon? _displayIcon;
    private PleasantIcon? _displayTitle;

    private ContentPresenter? _leftTitleBarContent;
    private MenuItem? _reestablishMenuItem;
    private TextBlock? _subtitle;

    private ContentPresenter? _titleBarContent;
    private StackPanel? _titlePanel;
    
    private CompositeDisposable? _disposables;
    
    public static readonly DirectProperty<PleasantTitleBar, bool> IsMacOSProperty =
        AvaloniaProperty.RegisterDirect<PleasantTitleBar, bool>(nameof(IsMacOS),
            titleBar => titleBar.IsMacOS);

    /// <summary>
    /// Defines the <see cref="IsTitleVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTitleVisibleProperty =
        AvaloniaProperty.Register<PleasantTitleBar, bool>(nameof(IsTitleVisible), true);
    
    /// <summary>
    /// Defines the attached property that controls whether the drag area of the title bar responds to hit-testing.
    /// </summary>
    public static readonly AttachedProperty<bool> IsTitleBarHitTestVisibleProperty =
        AvaloniaProperty.RegisterAttached<PleasantTitleBar, Window, bool>(
            "IsTitleBarHitTestVisible", defaultValue: true);
    
    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<PleasantTitleBar, Thickness>(nameof(ContentPadding));

    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the title panel (icon + title + subtitle) is visible.
    /// </summary>
    public bool IsTitleVisible
    {
        get => GetValue(IsTitleVisibleProperty);
        set => SetValue(IsTitleVisibleProperty, value);
    }

    public bool IsMacOS
    {
        get;
        set => SetAndRaise(IsMacOSProperty, ref field, value);
    } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>Gets the IsTitleBarHitTestVisible attached value from a window.</summary>
    public static bool GetIsTitleBarHitTestVisible(Window obj) => obj.GetValue(IsTitleBarHitTestVisibleProperty);

    /// <summary>Sets the IsTitleBarHitTestVisible attached value on a window.</summary>
    public static void SetIsTitleBarHitTestVisible(Window obj, bool value) => obj.SetValue(IsTitleBarHitTestVisibleProperty, value);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _captionButtons?.Detach();

        _captionButtons = e.NameScope.Get<PleasantCaptionButtons>("PART_PleasantCaptionButtons");

        _closeMenuItem = e.NameScope.Get<MenuItem>("PART_CloseMenuItem");
        _expandMenuItem = e.NameScope.Get<MenuItem>("PART_ExpandMenuItem");
        _collapseMenuItem = e.NameScope.Get<MenuItem>("PART_CollapseMenuItem");
        _reestablishMenuItem = e.NameScope.Get<MenuItem>("PART_ReestablishMenuItem");

        _displayIcon = e.NameScope.Get<PleasantIcon>("PART_DisplayIcon");
        _displayTitle = e.NameScope.Get<PleasantIcon>("PART_DisplayTitle");
        _subtitle = e.NameScope.Get<TextBlock>("PART_Subtitle");
        _titleBarGrid = e.NameScope.Get<Grid>("PART_TitleBarGrid");
        _dragWindowBorder = e.NameScope.Get<Border>("PART_DragWindow");
        _titlePanel = e.NameScope.Get<StackPanel>("PART_TitlePanel");

        _leftTitleBarContent = e.NameScope.Find<ContentPresenter>("PART_LeftTitleBarContent");
        _titleBarContent = e.NameScope.Find<ContentPresenter>("PART_TitleBarContent");

        if (TopLevel.GetTopLevel(this) is PleasantWindow window)
        {
            _host = window;
            _captionButtons.Host = window;

            if (window.EnableCustomTitleBar)
                PopulateTitleBar();

            _closeMenuItem.Click += (_, _) => window.Close();
            _reestablishMenuItem.Click += (_, _) => window.WindowState = WindowState.Normal;
            _expandMenuItem.Click += (_, _) => window.WindowState = WindowState.Maximized;
            _collapseMenuItem.Click += (_, _) => window.WindowState = WindowState.Minimized;

            Attach(window);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsTitleVisibleProperty && e.NewValue is bool visible)
        {
            PseudoClasses.Set(":title-visible", visible);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        
        _disposables?.Dispose();
        _disposables = null;
    }

    private void Attach(PleasantWindow host)
    {
        _disposables?.Dispose();
        
        _disposables = new CompositeDisposable
        {
            host.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(windowState =>
            {
                PseudoClasses.Set(":minimized", windowState == WindowState.Minimized);
                PseudoClasses.Set(":normal", windowState == WindowState.Normal);
                PseudoClasses.Set(":maximized", windowState == WindowState.Maximized);

                if (windowState == WindowState.Maximized)
                {
                    _reestablishMenuItem?.IsEnabled = true;
                    _expandMenuItem?.IsEnabled = false;
                }
                else
                {
                    _reestablishMenuItem?.IsEnabled = false;
                    _expandMenuItem?.IsEnabled = true;
                }
            })),
            host.GetObservable(WindowBase.IsActiveProperty).Subscribe(new AnonymousObserver<bool>(b =>
            {
                PseudoClasses.Set(":active", b);
                PseudoClasses.Set(":isactive", !b);
            })),
            host.GetObservable(PleasantWindow.SubtitleProperty).Subscribe(new AnonymousObserver<string>(s =>
            {
                _subtitle?.Text = s;
            })),
            host.GetObservable(Window.TitleProperty).Subscribe(new AnonymousObserver<string?>(SetDisplayTitle)),
            host.GetObservable(PleasantWindow.DisplayTitleProperty).Subscribe(new AnonymousObserver<object?>(SetDisplayTitle)),
            host.GetObservable(PleasantWindow.DisplayIconProperty).Subscribe(new AnonymousObserver<object?>(SetDisplayIcon)),
            host.GetObservable(Window.IconProperty).Subscribe(new AnonymousObserver<WindowIcon?>(SetDisplayIcon)),
            host.GetObservable(PleasantWindow.LeftTitleBarContentProperty).Subscribe(new AnonymousObserver<object?>(content =>
            {
                _leftTitleBarContent?.Content = content;
            })),
            host.GetObservable(PleasantWindow.TitleContentProperty).Subscribe(new AnonymousObserver<object?>(content =>
            {
                _titleBarContent?.Content = content;

                if (IsMacOS && _titlePanel is not null)
                    _titlePanel.IsVisible = !host.ExtendsContentIntoTitleBar && content is null;
            })),
            host.GetObservable(PleasantWindow.TitleBarTypeProperty).Subscribe(new AnonymousObserver<Type>(type =>
            {
                PseudoClasses.Set(":titlebar", type == Type.Classic);
                PseudoClasses.Set(":compact-titlebar", type == Type.Compact);
            })),
            host.GetObservable(PleasantWindow.ExtendsContentIntoTitleBarProperty).Subscribe(new AnonymousObserver<bool>(b =>
            {
                if (IsMacOS && _titlePanel is not null)
                    _titlePanel.IsVisible = !b;
            })),
            host.GetObservable(IsTitleBarHitTestVisibleProperty).Subscribe(new AnonymousObserver<bool>(hitTestVisible =>
            {
                _dragWindowBorder?.IsHitTestVisible = hitTestVisible;
            })),
            host.GetObservable(PleasantWindow.EnableCustomTitleBarProperty).Subscribe(new AnonymousObserver<bool>(enable =>
            {
                if (_dragWindowBorder == null || _host == null || _titlePanel == null ||
                    _leftTitleBarContent == null || _captionButtons == null)
                    return;

                host.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(state =>
                {
                    // On macOS without caption override, native buttons are used — hide custom ones.
                    // Otherwise always keep the panel visible; UpdateButtonVisibility controls individual buttons.
                    bool usesCustomCaptions = !IsMacOS || _host.OverrideMacOSCaption;
                    _captionButtons.IsVisible = enable && usesCustomCaptions;
                    // Drag border is not useful in fullscreen
                    _dragWindowBorder.IsVisible = enable && state != WindowState.FullScreen;
                }));

                bool usesCustomCaptionsNow = !IsMacOS || _host.OverrideMacOSCaption;
                _captionButtons.IsVisible = enable && usesCustomCaptionsNow;
                _titlePanel.IsVisible = enable && IsTitleVisible;
                _leftTitleBarContent.IsVisible = enable;
            }))
        };
    }

    private void SetDisplayIcon(object? obj)
    {
        if (_displayIcon is null || obj is WindowIcon)
            return;

        _displayIcon.Icon = obj;
    }

    private void SetDisplayTitle(object? obj)
    {
        _displayTitle?.Icon = obj;
    }

    private void PopulateTitleBar()
    {
        if (_titleBarGrid == null)
            return;

        _titleBarGrid.ColumnDefinitions.Clear();

        if (IsMacOS)
        {
            if (_host != null)
            {
                GridLength firstColWidth = !_host.OverrideMacOSCaption
                ? new GridLength(75, GridUnitType.Pixel)
                : GridLength.Auto;

                _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = firstColWidth });
            }
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _host?.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(state =>
            {
                _titleBarGrid.ColumnDefinitions[0].Width = state == WindowState.FullScreen
                    ? new GridLength(45, GridUnitType.Pixel)
                    : (!_host.OverrideMacOSCaption
                        ? new GridLength(75, GridUnitType.Pixel)
                        : GridLength.Auto);
            }));
        }
        else
        {
            // Non-macOS layout
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }
        // Set child placements based on platform.
        if (IsMacOS)
        {
            if (_captionButtons != null)
            {
                Grid.SetColumn(_captionButtons, 0);
                _captionButtons.IsHitTestVisible = true;
            }
            if (_dragWindowBorder != null)
            {
                Grid.SetColumn(_dragWindowBorder, 0);
                Grid.SetColumnSpan(_dragWindowBorder, 4);
            }
            if (_leftTitleBarContent != null)
                Grid.SetColumn(_leftTitleBarContent, 1);
            if (_titlePanel != null)
                Grid.SetColumn(_titlePanel, 2);
            if (_titleBarContent != null)
                Grid.SetColumn(_titleBarContent, 3);
        }
        else
        {
            if (_dragWindowBorder != null)
            {
                Grid.SetColumn(_dragWindowBorder, 1);
                Grid.SetColumnSpan(_dragWindowBorder, 3);
            }
            if (_leftTitleBarContent != null)
            {
                Grid.SetColumn(_leftTitleBarContent, 1);
                _leftTitleBarContent.IsHitTestVisible = false;
            }
            if (_titlePanel != null)
                Grid.SetColumn(_titlePanel, 2);
            if (_titleBarContent != null)
                Grid.SetColumn(_titleBarContent, 3);
            if (_captionButtons != null)
                Grid.SetColumn(_captionButtons, 4);
        }
    }
}