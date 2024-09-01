using System.ComponentModel;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PleasantUI.Core.Interfaces;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

public class PleasantModalWindow : ContentControl
{
    private IPleasantWindow _host = null!;
    
    private object? _dialogResult;
    private bool _isClosed;
    private bool _isClosing;

    public event EventHandler? Closed;
    
    public static readonly RoutedEvent WindowClosedEvent =
        RoutedEvent.Register<PleasantModalWindow, RoutedEventArgs>("WindowClosed", RoutingStrategies.Direct);

    public static readonly RoutedEvent WindowOpenedEvent =
        RoutedEvent.Register<PleasantModalWindow, RoutedEventArgs>("WindowOpened", RoutingStrategies.Direct);
    
    public static readonly DirectProperty<PleasantModalWindow, bool> IsClosedProperty =
        AvaloniaProperty.RegisterDirect<PleasantModalWindow, bool>(nameof(IsClosed), o => o.IsClosed,
            (o, v) => o.IsClosed = v);

    public static readonly DirectProperty<PleasantModalWindow, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<PleasantModalWindow, bool>(nameof(IsClosing), o => o.IsClosing);

    public static readonly StyledProperty<Animation?> OpenAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation?>(nameof(OpenAnimation));
    
    public static readonly StyledProperty<Animation?> ShowBackgroundAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation?>(nameof(ShowBackgroundAnimation));
    
    public static readonly StyledProperty<Animation?> CloseAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation?>(nameof(CloseAnimation));
    
    public static readonly StyledProperty<Animation?> HideBackgroundAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation?>(nameof(HideBackgroundAnimation));

    static PleasantModalWindow() { }
    
    internal readonly Control ModalBackground;

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
    /// Initializes a new instance of the <see cref="PleasantModalWindow"/> class.
    /// </summary>
    public PleasantModalWindow()
    {
        ModalBackground = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#3A000000"))
        };
    }

    /// <summary>
    /// Shows a <see cref="PleasantWindow"/> asynchronously.
    /// </summary>
    /// <param name="host">The <see cref="IPleasantWindow"/> to show.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task Show(IPleasantWindow host) => Show<object>(host);

    /// <summary>
    /// Shows a modal window and returns a task that completes with the dialog result.
    /// </summary>
    /// <typeparam name="T">The type of the dialog result.</typeparam>
    /// <param name="host">The <see cref="IPleasantWindow"/> implementation representing the hosting window.</param>
    /// <returns>A task that completes with the dialog result.</returns>
    public Task<T?> Show<T>(IPleasantWindow host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));

        RaiseEvent(new RoutedEventArgs(WindowOpenedEvent));

        _host.AddModalWindow(this);

        ShowBackgroundAnimation?.RunAsync(ModalBackground);
        OpenAnimation?.RunAsync(this);

        TaskCompletionSource<T?> result = new();

        Observable.FromEventPattern(
                x => Closed += x,
                x => Closed -= x)
            .Take(1)
            .Subscribe(_ => { result.SetResult((T?)_dialogResult); });

        return result.Task;
    }

    /// <summary>
    /// Closes the window.
    /// </summary>
    public void Close()
    {
        Close(false);
    }

    /// <summary>
    /// Closes the current dialog with an optional dialog result.
    /// </summary>
    /// <param name="dialogResult">The dialog result to be set.</param>
    public void Close(object? dialogResult)
    {
        _dialogResult = dialogResult;
        Close(false);
    }
    
    protected virtual void OnClosed()
    {
        Closed?.Invoke(this, null!);
    }
    
    internal async void Close(bool ignoreCancel)
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
                ModalBackground.IsHitTestVisible = false;

                HideBackgroundAnimation?.RunAsync(ModalBackground);
                if (CloseAnimation is not null)
                    await CloseAnimation.RunAsync(this);

                _host.RemoveModalWindow(this);
            }
        }
    }

    private bool ShouldCancelClose(CancelEventArgs? args = null)
    {
        args ??= new CancelEventArgs();
        return args.Cancel;
    }
}