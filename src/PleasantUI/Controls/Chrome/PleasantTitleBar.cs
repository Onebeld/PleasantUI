using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PleasantUI.Extensions;
using PleasantUI.Reactive;
using Path = Avalonia.Controls.Shapes.Path;

namespace PleasantUI.Controls.Chrome;

/// <summary>
/// Represents the title bar of a <see cref="PleasantWindow"/>.
/// </summary>
/// <remarks>
/// This control provides the standard title bar functionality, including window dragging, caption buttons, and title/subtitle display.
/// It uses a template to define its visual structure and interacts with <see cref="PleasantWindow"/> for window-related operations.
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
    private PleasantCaptionButtons? _captionButtons;

    private MenuItem? _closeMenuItem;
    private MenuItem? _collapseMenuItem;
    private MenuItem? _expandMenuItem;
    private MenuItem? _reestablishMenuItem;

    private Border? _dragWindowBorder;
    private Image? _image;
    private Path? _logoPath;

    private TextBlock? _title;
    private StackPanel? _titlePanel;
    private TextBlock? _subtitle;

    private ContentPresenter? _leftTitleBarContent;
    private ContentPresenter? _titleBarContent;

    private PleasantWindow? _host;

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _captionButtons?.Detach();

        _captionButtons = e.NameScope.Get<PleasantCaptionButtons>("PART_PleasantCaptionButtons");

        _closeMenuItem = e.NameScope.Get<MenuItem>("PART_CloseMenuItem");
        _expandMenuItem = e.NameScope.Get<MenuItem>("PART_ExpandMenuItem");
        _collapseMenuItem = e.NameScope.Get<MenuItem>("PART_CollapseMenuItem");
        _reestablishMenuItem = e.NameScope.Get<MenuItem>("PART_ReestablishMenuItem");

        _image = e.NameScope.Find<Image>("PART_Icon");
        _title = e.NameScope.Get<TextBlock>("PART_Title");
        _subtitle = e.NameScope.Get<TextBlock>("PART_Subtitle");
        _logoPath = e.NameScope.Get<Path>("PART_LogoPath");
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

            window.TitleBar = this;
        }
    }
    
    private void Attach(PleasantWindow host)
    {
        CompositeDisposable compositeDisposable = new()
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
            host.GetObservable(WindowBase.IsActiveProperty).Subscribe(b =>
            {
                PseudoClasses.Set(":isactive", !b);
            }),
            host.GetObservable(Window.TitleProperty).Subscribe(s =>
            {
                if (_title is not null) _title.Text = s;
            }),
            host.GetObservable(PleasantWindow.SubtitleProperty).Subscribe(s =>
            {
                if (_subtitle is not null) _subtitle.Text = s;
            }),
            host.GetObservable(PleasantWindow.TitleGeometryProperty).Subscribe(geometry =>
            {
                if (_logoPath is not null)
                {
                    _logoPath.Data = geometry!;
                    _logoPath.IsVisible = geometry is null == false;
                }

                if (_title != null) _title.IsVisible = geometry is null;
            }),
            host.GetObservable(PleasantWindow.IconImageProperty).Subscribe(image =>
            {
                if (image is not null)
                {
                    if (_image is not null) _image.Source = image;
                }
                else
                {
                    if (host.Icon is not null && _image is not null)
                        _image.Source = host.Icon.ToBitmap();
                }
            }),
            host.GetObservable(Window.IconProperty).Subscribe(_ =>
            {
                if (host.IconImage is null)
                {
                    if (_image is not null && host.Icon is not null)
                        _image.Source = host.Icon.ToBitmap();
                }
            }),
            host.GetObservable(PleasantWindow.LeftTitleContentProperty).Subscribe(content =>
            {
                if (_leftTitleBarContent is not null)
                    _leftTitleBarContent.Content = content;
            }),
            host.GetObservable(PleasantWindow.TitleContentProperty).Subscribe(content =>
            {
                if (_titleBarContent is not null)
                    _titleBarContent.Content = content;
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
                
                if (!_host.ShowTitleBarContentAnyway)
                    IsVisible = enable;
            })
        };
    }
    
    internal FlyoutBase? GetContextFlyout() => _dragWindowBorder?.ContextFlyout;

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
}