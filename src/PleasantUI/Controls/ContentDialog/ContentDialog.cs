using System.ComponentModel;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a dialog that can be used to display content to the user.
/// </summary>
/// <remarks>
/// This dialog provides a modal experience, meaning that it blocks interaction with the rest of the application until
/// it is closed.
/// </remarks>
public class ContentDialog : ContentControl, ICustomKeyboardNavigation
{
    private ModalWindowHost? _host;
    
    private object? _dialogResult;
    private Control? _originalHost;
    private int _originalHostIndex;
    private IInputElement? _lastFocus;
    private Border? _modalBackground;
    private Panel? _panel;
        
    private bool _isClosed;
    private bool _isClosing;
    
    /// <summary>
    /// Defines the <see cref="BottomPanelContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object> BottomPanelContentProperty =
        AvaloniaProperty.Register<ContentDialog, object>(nameof(BottomPanelContent));
    
    /// <summary>
    /// Defines the WindowClosed event.
    /// </summary>
    public static readonly RoutedEvent WindowClosedEvent =
        RoutedEvent.Register<PleasantModalWindow, RoutedEventArgs>("WindowClosed", RoutingStrategies.Direct);

    /// <summary>
    /// Defines the WindowOpened event.
    /// </summary>
    public static readonly RoutedEvent WindowOpenedEvent =
        RoutedEvent.Register<PleasantModalWindow, RoutedEventArgs>("WindowOpened", RoutingStrategies.Direct);
    
    /// <summary>
    /// Defines the <see cref="IsClosed"/> property.
    /// </summary>
    public static readonly DirectProperty<PleasantModalWindow, bool> IsClosedProperty =
        AvaloniaProperty.RegisterDirect<PleasantModalWindow, bool>(nameof(IsClosed), o => o.IsClosed,
            (o, v) => o.IsClosed = v);

    /// <summary>
    /// Defines the <see cref="IsClosing"/> property.
    /// </summary>
    public static readonly DirectProperty<PleasantModalWindow, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<PleasantModalWindow, bool>(nameof(IsClosing), o => o.IsClosing);
    
    /// <summary>
    /// Defines the <see cref="OpenAnimation"/> property.
    /// </summary>
    public static readonly StyledProperty<Animation?> OpenAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation?>(nameof(OpenAnimation));

    /// <summary>
    /// Defines the <see cref="ShowBackgroundAnimation"/> property.
    /// </summary>
    public static readonly StyledProperty<Animation?> ShowBackgroundAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation?>(nameof(ShowBackgroundAnimation));

    /// <summary>
    /// Defines the <see cref="CloseAnimation"/> property.
    /// </summary>
    public static readonly StyledProperty<Animation?> CloseAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation?>(nameof(CloseAnimation));

    /// <summary>
    /// Defines the <see cref="HideBackgroundAnimation"/> property.
    /// </summary>
    public static readonly StyledProperty<Animation?> HideBackgroundAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation?>(nameof(HideBackgroundAnimation));

    /// <summary>
    /// Allows you to embed content on the modal window (e.g. buttons)
    /// </summary>
    public object BottomPanelContent
    {
        get => GetValue(BottomPanelContentProperty);
        set => SetValue(BottomPanelContentProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a value indicating that the window has been closed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the window is closed; otherwise, <c>false</c>.
    /// </value>
    public bool IsClosed
    {
        get => _isClosed;
        set => SetAndRaise(IsClosedProperty, ref _isClosed, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window is currently closing.
    /// </summary>
    /// <value>
    /// <c>true</c> if the window is closing; otherwise, <c>false</c>.
    /// </value>
    public bool IsClosing
    {
        get => _isClosing;
        set => SetAndRaise(IsClosingProperty, ref _isClosing, value);
    }
    
    /// <summary>
    /// Set the animation when opening a modal window.
    /// </summary>
    public Animation? OpenAnimation
    {
        get => GetValue(OpenAnimationProperty);
        set => SetValue(OpenAnimationProperty, value);
    }

    /// <summary>
    /// Set the animation of the modal background when opening a modal window.
    /// </summary>
    public Animation? ShowBackgroundAnimation
    {
        get => GetValue(ShowBackgroundAnimationProperty);
        set => SetValue(ShowBackgroundAnimationProperty, value);
    }

    /// <summary>
    /// Sets the animation of the modal background when the modal window is closed.
    /// </summary>
    public Animation? HideBackgroundAnimation
    {
        get => GetValue(HideBackgroundAnimationProperty);
        set => SetValue(HideBackgroundAnimationProperty, value);
    }

    /// <summary>
    /// Sets the animation when the modal window is closed.
    /// </summary>
    public Animation? CloseAnimation
    {
        get => GetValue(CloseAnimationProperty);
        set => SetValue(CloseAnimationProperty, value);
    }
    
    /// <summary>
    /// Raised when the modal window is closed.
    /// </summary>
    public event EventHandler? Closed;

    public Task ShowAsync() => ShowAsyncCoreForTopLevel<object>(null);

    public Task ShowAsync(Window window) => ShowAsyncCoreForTopLevel<object>(window);

    public Task ShowAsync(IPleasantWindow pleasantWindow) => ShowAsyncCoreForTopLevel<object>(pleasantWindow as TopLevel);
    
    public Task ShowAsync(TopLevel topLevel) => ShowAsyncCoreForTopLevel<object>(topLevel);
    
    public Task<T?> ShowAsync<T>() => ShowAsyncCoreForTopLevel<T>(null);

    public Task<T?> ShowAsync<T>(Window window) => ShowAsyncCoreForTopLevel<T>(window);

    public Task<T?> ShowAsync<T>(IPleasantWindow pleasantWindow) => ShowAsyncCoreForTopLevel<T>(pleasantWindow as TopLevel);

    public Task<T?> ShowAsync<T>(TopLevel topLevel) => ShowAsyncCoreForTopLevel<T>(topLevel);
    
    /// <summary>
    /// Closes the window.
    /// </summary>
    public async Task CloseAsync()
    {
        await CloseAsync(false);
    }

    /// <summary>
    /// Closes the current dialog with an optional dialog result.
    /// </summary>
    /// <param name="dialogResult">The dialog result to be set.</param>
    public async Task CloseAsync(object? dialogResult)
    {
        _dialogResult = dialogResult;
        await CloseAsync(false);
    }
    
    private async Task CloseAsync(bool ignoreCancel)
    {
        bool close = true;

        try
        {
            if (!ignoreCancel && ShouldCancelClose()) close = false;
        }
        finally
        {
            if (close)
            {
                RaiseEvent(new RoutedEventArgs(WindowClosedEvent));
                OnClosed();
                PseudoClasses.Set(":close", true);

                IsHitTestVisible = false;
                _modalBackground.IsHitTestVisible = false;

                Focus();

                HideBackgroundAnimation?.RunAsync(_modalBackground);
                await CloseAnimation?.RunAsync(this);

                if (_lastFocus is not null)
                {
                    _lastFocus.Focus();
                    _lastFocus = null;
                }
                
                OverlayLayer overlayLayer = OverlayLayer.GetOverlayLayer(_host);

                if (overlayLayer is not null)
                {
                    overlayLayer.Children.Remove(_host);

                    _host.Content = null;

                    if (_originalHost is not null)
                    {
                        if (_originalHost is Panel panel)
                        {
                            panel.Children.Insert(_originalHostIndex, this);
                        }
                        else if (_originalHost is Decorator decorator)
                        {
                            decorator.Child = this;
                        }
                        else if (_originalHost is ContentControl contentControl)
                        {
                            contentControl.Content = this;
                        }
                        else if (_originalHost is ContentPresenter contentPresenter)
                        {
                            contentPresenter.Content = this;
                        }
                    }
                }
            }
        }
    }

    private async Task<T?> ShowAsyncCoreForTopLevel<T>(TopLevel? topLevel)
    {
        TaskCompletionSource<T?> taskCompletionSource = new();

        if (Parent is not null)
        {
            _originalHost = (Control)Parent;

            switch (_originalHost)
            {
                case Panel panel:
                    _originalHostIndex = panel.Children.IndexOf(this);
                    panel.Children.Remove(this);
                    break;
                case Decorator decorator:
                    decorator.Child = null;
                    break;
                case ContentControl contentControl:
                    contentControl.Content = null;
                    break;
                case ContentPresenter contentPresenter:
                    contentPresenter.Content = null;
                    break;
            }
        }

        _panel = new Panel();
        
        _modalBackground = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#3A000000"))
        };
        
        _panel.Children.Add(_modalBackground);
        _panel.Children.Add(this);

        _host ??= new ModalWindowHost();
        _host.Content = _panel;

        OverlayLayer? overlayLayer;

        if (topLevel is not null)
        {
            overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
        }
        else
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                var windows = desktopLifetime.Windows;

                for (int i = 0; i < windows.Count; i++)
                {
                    if (!windows[i].IsActive) continue;
                    
                    topLevel = windows[i];
                    break;
                }

                if (topLevel is null)
                    topLevel = desktopLifetime.MainWindow ?? throw new NotSupportedException("No TopLevel root found to parent ContentDialog");
                
                overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
            }
            else if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                topLevel = TopLevel.GetTopLevel(singleViewLifetime.MainView);
                overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
            }
            else
            {
                throw new InvalidOperationException(
                    "No TopLevel found for ContentDialog and no ApplicationLifetime is set. " +
                    "Please either supply a valid ApplicationLifetime or TopLevel to ShowAsync()");
            }
        }

        if (overlayLayer is null)
            throw new InvalidOperationException("Unable to find OverlayLayer from given TopLevel");

        _lastFocus = topLevel.FocusManager?.GetFocusedElement();
        
        overlayLayer.Children.Add(_host);
        
        ShowBackgroundAnimation?.RunAsync(_modalBackground);
        OpenAnimation?.RunAsync(this);

        return await taskCompletionSource.Task;
    }

    /// <inheritdoc />
    public (bool handled, IInputElement? next) GetNext(IInputElement element, NavigationDirection direction)
    {
        List<IInputElement> children = this.GetVisualDescendants().OfType<IInputElement>()
            .Where(x => KeyboardNavigation.GetIsTabStop((InputElement)x) && x.Focusable &&
                        x.IsEffectivelyVisible && x.IsEffectivelyEnabled).ToList();

        if (children.Count == 0)
            return (false, null);

        IInputElement? current = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (current == null)
            return (false, null);

        if (direction == NavigationDirection.Next)
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != current) continue;

                if (i == children.Count - 1)
                    return (true, children[0]);

                return (true, children[i + 1]);
            }
        else if (direction == NavigationDirection.Previous)
            for (int i = children.Count - 1; i >= 0; i--)
            {
                if (children[i] != current) continue;

                if (i == 0)
                    return (true, children[children.Count - 1]);

                return (true, children[i - 1]);
            }

        return (false, null);
    }
    
    /// <summary>
    /// Raises the <see cref="Closed"/> event.
    /// </summary>
    protected virtual void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
    
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ContentDialog);

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Focus(NavigationMethod.Pointer);
    }
    
    private bool ShouldCancelClose(CancelEventArgs? args = null)
    {
        args ??= new CancelEventArgs();
        return args.Cancel;
    }
}