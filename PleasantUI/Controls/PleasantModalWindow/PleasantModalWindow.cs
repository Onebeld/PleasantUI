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
    
    public bool IsClosed
    {
        get => _isClosed;
        set => SetAndRaise(IsClosedProperty, ref _isClosed, value);
    }

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

    public PleasantModalWindow()
    {
        ModalBackground = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#3A000000"))
        };
    }
    
    public Task Show(IPleasantWindow host) => Show<object>(host);

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
    
    public void Close()
    {
        Close(false);
    }

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