﻿using System.ComponentModel;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PleasantUI.Core.Interfaces;

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
    
    public static readonly StyledProperty<Animation> BackgroundOpenAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation>(nameof(BackgroundOpenAnimation));
    
    public static readonly StyledProperty<Animation> BackgroundCloseAnimationProperty =
        AvaloniaProperty.Register<PleasantModalWindow, Animation>(nameof(BackgroundCloseAnimation));

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PleasantModalWindow, string>(nameof(Title));

    public static readonly StyledProperty<bool> ShowTitleBarProperty =
        AvaloniaProperty.Register<PleasantModalWindow, bool>(nameof(ShowTitleBar));

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

    public Animation? OpenAnimation
    {
        get => GetValue(OpenAnimationProperty);
        set => SetValue(OpenAnimationProperty, value);
    }
    
    public Animation? ShowBackgroundAnimation
    {
        get => GetValue(ShowBackgroundAnimationProperty);
        set => SetValue(ShowBackgroundAnimationProperty, value);
    }
    
    public Animation? HideBackgroundAnimation
    {
        get => GetValue(HideBackgroundAnimationProperty);
        set => SetValue(HideBackgroundAnimationProperty, value);
    }

    public Animation? CloseAnimation
    {
        get => GetValue(CloseAnimationProperty);
        set => SetValue(CloseAnimationProperty, value);
    }
    
    public Animation BackgroundOpenAnimation
    {
        get => GetValue(BackgroundOpenAnimationProperty);
        set => SetValue(BackgroundOpenAnimationProperty, value);
    }

    public Animation BackgroundCloseAnimation
    {
        get => GetValue(BackgroundCloseAnimationProperty);
        set => SetValue(BackgroundCloseAnimationProperty, value);
    }
    
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowTitleBar
    {
        get => GetValue(ShowTitleBarProperty);
        set => SetValue(ShowTitleBarProperty, value);
    }
    
    public bool CanOpen { get; set; }

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

        CanOpen = true;

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

                CanOpen = false;

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