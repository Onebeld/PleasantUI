using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using PleasantUI.Core.Extensions;
using PleasantUI.Reactive;
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
    
    private PleasantWindow? _host;
    private PleasantCaptionButtons? _captionButtons;

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
        _dragWindowBorder = e.NameScope.Get<Border>("PART_DragWindow");
        _titlePanel = e.NameScope.Get<StackPanel>("PART_TitlePanel");

        _leftTitleBarContent = e.NameScope.Find<ContentPresenter>("PART_LeftTitleBarContent");
        _titleBarContent = e.NameScope.Find<ContentPresenter>("PART_TitleBarContent");

        if (VisualRoot is PleasantWindow window)
        {
            _host = window;
            _captionButtons.Host = window;

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
            host.GetObservable(Window.WindowStateProperty).Subscribe(windowState =>
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
            }),
            host.GetObservable(WindowBase.IsActiveProperty).Subscribe(b => { PseudoClasses.Set(":isactive", !b); }),
            host.GetObservable(PleasantWindow.SubtitleProperty).Subscribe(s =>
            {
                if (_subtitle is not null) _subtitle.Text = s;
            }),
            host.GetObservable(Window.TitleProperty).Subscribe(SetDisplayTitle),
            host.GetObservable(PleasantWindow.DisplayTitleProperty).Subscribe(SetDisplayTitle),
            host.GetObservable(PleasantWindow.DisplayIconProperty).Subscribe(SetDisplayIcon),
            host.GetObservable(Window.IconProperty).Subscribe(SetDisplayIcon),
            host.GetObservable(PleasantWindow.LeftTitleBarContentProperty).Subscribe(content =>
            {
                if (_leftTitleBarContent is not null)
                    _leftTitleBarContent.Content = content;
            }),
            host.GetObservable(PleasantWindow.TitleContentProperty).Subscribe(content =>
            {
                if (_titleBarContent is not null)
                    _titleBarContent.Content = content;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && _titlePanel is not null)
                    _titlePanel.IsVisible = !host.ExtendsContentIntoTitleBar && content is null;
            }),
            host.GetObservable(PleasantWindow.TitleBarTypeProperty).Subscribe(type =>
            {
                PseudoClasses.Set(":titlebar", type == Type.Classic);
            }),
            host.GetObservable(PleasantWindow.ExtendsContentIntoTitleBarProperty).Subscribe(b =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && _titlePanel is not null)
                    _titlePanel.IsVisible = !b;
            }),
            host.GetObservable(PleasantWindow.EnableCustomTitleBarProperty).Subscribe(enable =>
            {
                if (_dragWindowBorder is null ||
                    _host is null ||
                    _titlePanel is null ||
                    _leftTitleBarContent is null ||
                    _captionButtons is null) return;

                _dragWindowBorder.IsVisible = enable;
                _titlePanel.IsVisible = enable;
                _leftTitleBarContent.IsVisible = enable;
                _captionButtons.IsVisible = enable;

                /*
                if (!_host.ShowTitleBarContentAnyway)
                    IsVisible = enable;*/
            })
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
                _displayIcon.Children.Add(new PathIcon { Data = geometry, Width = 16, Height = 16, [!ForegroundProperty] = _displayIcon[!TextElement.ForegroundProperty]});
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
                _displayTitle.Children.Add(new PathIcon { Data = geometry, Height = 8, Width = double.NaN, [!ForegroundProperty] = _displayTitle[!TextElement.ForegroundProperty]});
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
}