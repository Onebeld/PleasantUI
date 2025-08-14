using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Reactive;
using PleasantUI.Core.Extensions;
using PleasantUI.Core.Internal;
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
        ClassicExtended = 1
    }

    private bool isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private PleasantWindow? _host;
    private PleasantCaptionButtons? _captionButtons;

    private Grid? _titleBarGrid;

    private MenuItem? _closeMenuItem;
    private MenuItem? _collapseMenuItem;

    private Border? _dragWindowBorder;
    private MenuItem? _expandMenuItem;

    private Panel? _displayIcon;
    private Panel? _displayTitle;

    private ContentPresenter? _leftTitleBarContent;
    private MenuItem? _reestablishMenuItem;
    private TextBlock? _subtitle;

    private ContentPresenter? _titleBarContent;
    private StackPanel? _titlePanel;

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

        _displayIcon = e.NameScope.Find<Panel>("PART_DisplayIcon");
        _displayTitle = e.NameScope.Get<Panel>("PART_DisplayTitle");
        _subtitle = e.NameScope.Get<TextBlock>("PART_Subtitle");
        _titleBarGrid = e.NameScope.Find<Grid>("PART_TitleBarGrid");
        _dragWindowBorder = e.NameScope.Get<Border>("PART_DragWindow");
        _titlePanel = e.NameScope.Get<StackPanel>("PART_TitlePanel");

        _leftTitleBarContent = e.NameScope.Find<ContentPresenter>("PART_LeftTitleBarContent");
        _titleBarContent = e.NameScope.Find<ContentPresenter>("PART_TitleBarContent");

        if (VisualRoot is PleasantWindow window)
        {
            _host = window;
            _captionButtons.Host = window;

            if (window.EnableCustomTitleBar)
                PopulateTitleBar();

            _closeMenuItem.Click += (_, _) => window.Close();
            _reestablishMenuItem.Click += (_, _) => window.WindowState = WindowState.Normal;
            _expandMenuItem.Click += (_, _) => window.WindowState = WindowState.Maximized;
            _collapseMenuItem.Click += (_, _) => window.WindowState = WindowState.Minimized;

            _dragWindowBorder.PointerPressed += OnDragWindowBorderOnPointerPressed;
            _dragWindowBorder.DoubleTapped += OnDragWindowBorderOnDoubleTapped;

            Attach(window);
        }
    }

    private void Attach(PleasantWindow host)
    {
        CompositeDisposable unused = new()
        {
            host.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(windowState =>
            {
                PseudoClasses.Set(":minimized", windowState == WindowState.Minimized);
                PseudoClasses.Set(":normal", windowState == WindowState.Normal);
                PseudoClasses.Set(":maximized", windowState == WindowState.Maximized);

                if (windowState == WindowState.Maximized)
                {
                    if (_reestablishMenuItem is not null) _reestablishMenuItem.IsEnabled = true;
                    if (_expandMenuItem is not null) _expandMenuItem.IsEnabled = false;
                }
                else
                {
                    if (_reestablishMenuItem is not null) _reestablishMenuItem.IsEnabled = false;
                    if (_expandMenuItem is not null) _expandMenuItem.IsEnabled = true;
                }
            })),
            host.GetObservable(WindowBase.IsActiveProperty).Subscribe(new AnonymousObserver<bool>(b => { PseudoClasses.Set(":isactive", !b); })),
            host.GetObservable(PleasantWindow.SubtitleProperty).Subscribe(new AnonymousObserver<string>(s =>
            {
                if (_subtitle is not null) _subtitle.Text = s;
            })),
            host.GetObservable(Window.TitleProperty).Subscribe(new AnonymousObserver<string?>(SetDisplayTitle)),
            host.GetObservable(PleasantWindow.DisplayTitleProperty).Subscribe(new AnonymousObserver<object?>(SetDisplayTitle)),
            host.GetObservable(PleasantWindow.DisplayIconProperty).Subscribe(new AnonymousObserver<object?>(SetDisplayIcon)),
            host.GetObservable(Window.IconProperty).Subscribe(new AnonymousObserver<WindowIcon?>(SetDisplayIcon)),
            host.GetObservable(PleasantWindow.LeftTitleBarContentProperty).Subscribe(new AnonymousObserver<object?>(content =>
            {
                if (_leftTitleBarContent is not null)
                    _leftTitleBarContent.Content = content;
            })),
            host.GetObservable(PleasantWindow.TitleContentProperty).Subscribe(new AnonymousObserver<object?>(content =>
            {
                if (_titleBarContent is not null)
                    _titleBarContent.Content = content;

                if (isMacOS && _titlePanel is not null)
                    _titlePanel.IsVisible = !host.ExtendsContentIntoTitleBar && content is null;
            })),
            host.GetObservable(PleasantWindow.TitleBarTypeProperty).Subscribe(new AnonymousObserver<Type>(type =>
            {
                PseudoClasses.Set(":titlebar", type == Type.Classic);
            })),
            host.GetObservable(PleasantWindow.ExtendsContentIntoTitleBarProperty).Subscribe(new AnonymousObserver<bool>(b =>
            {
                if (isMacOS && _titlePanel is not null)
                    _titlePanel.IsVisible = !b;
            })),
            host.GetObservable(PleasantWindow.EnableCustomTitleBarProperty).Subscribe(new AnonymousObserver<bool>(enable =>
            {
                if (_dragWindowBorder == null || _host == null || _titlePanel == null ||
                _leftTitleBarContent == null || _captionButtons == null)
                return;

                host.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(state =>
                {
                    if (state == WindowState.FullScreen)
                    {
                        _captionButtons.IsVisible = !enable;
                        _dragWindowBorder.IsVisible = !enable;
                    }
                    else
                    {
                        _captionButtons.IsVisible = !_host.OverrideMacOSCaption ? !enable : enable;
                        _dragWindowBorder.IsVisible = enable;
                    }
                }));
                _titlePanel.IsVisible = enable;
                _leftTitleBarContent.IsVisible = enable;

                /*
                if (!_host.ShowTitleBarContentAnyway)
                    IsVisible = enable;*/
            }))
        };
    }

    private void OnDragWindowBorderOnPointerPressed(object? _, PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            _host?.BeginMoveDrag(args);
    }

    private void OnDragWindowBorderOnDoubleTapped(object? o, TappedEventArgs tappedEventArgs)
    {
        if (_host is null || !_host.CanResize) return;
        _host.WindowState = _host.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void SetDisplayIcon(object? obj)
    {
        if (_displayIcon is null || obj is WindowIcon)
            return;

        _displayIcon.Children.Clear();

        switch (obj)
        {
            case Geometry geometry:
                _displayIcon.Children.Add(new PathIcon { Data = geometry, Width = 16, Height = 16, [!ForegroundProperty] = _displayIcon[!TextElement.ForegroundProperty] });
                break;
            case IImage icon:
                _displayIcon.Children.Add(new Image { Source = icon, Width = 16, Height = 16 });
                break;
            case Control control:
                _displayIcon.Children.Add(control);
                break;

            case null when _host?.Icon is not null:
                _displayIcon.Children.Add(new Image { Source = _host?.Icon.ToBitmap(), Width = 16, Height = 16 });
                break;
        }
    }

    private void SetDisplayTitle(object? obj)
    {
        if (_displayTitle is null)
            return;

        _displayTitle.Children.Clear();

        switch (obj)
        {
            case Geometry geometry:
                _displayTitle.Children.Add(new PathIcon { Data = geometry, Height = 8, Width = double.NaN, [!ForegroundProperty] = _displayTitle[!TextElement.ForegroundProperty] });
                break;
            case IImage icon:
                _displayTitle.Children.Add(new Image { Source = icon, Height = 8, Width = double.NaN });
                break;
            case Control control:
                _displayTitle.Children.Add(control);
                break;
            case null when _host?.Title is not null:
                _displayTitle.Children.Add(new TextBlock { Text = _host.Title });
                break;
        }
    }

    private void PopulateTitleBar()
    {
        if (_titleBarGrid == null)
            return;

        _titleBarGrid.ColumnDefinitions.Clear();

        if (isMacOS)
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
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Pixel) });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _titleBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }
        // Set child placements based on platform.
        if (isMacOS)
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