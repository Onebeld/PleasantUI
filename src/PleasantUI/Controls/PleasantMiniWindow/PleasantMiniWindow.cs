using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PleasantUI.Core;
using PleasantUI.Core.Interfaces;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a custom window with pleasant features such as blur, custom title bar, and modal windows.
/// </summary>
public class PleasantMiniWindow : Window, IPleasantWindow
{
    private Panel _modalWindows = null!;

    private Button? _hiddenButton;
    private Button? _closeButton;
    private Panel? _dragWindowPanel;

    public static readonly StyledProperty<bool> EnableBlurProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(EnableBlur));
    public static readonly StyledProperty<bool> ShowPinButtonProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(ShowPinButton), true);
    public static readonly StyledProperty<bool> ShowHiddenButtonProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(ShowHiddenButton));
    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(EnableCustomTitleBar), true);

    /// <summary>
    /// Gets or sets a value indicating whether the blur effect is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the blur effect is enabled; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// The blur effect can be applied to enhance the visual aesthetics of an element.
    /// By default, the blur effect is disabled.
    /// </remarks>
    public bool EnableBlur
    {
        get => GetValue(EnableBlurProperty);
        set => SetValue(EnableBlurProperty, value);
    }
    
    /// <summary>
    /// Shows a spear that allows you to dock a window on top of all other windows on the desktop.
    /// </summary>
    public bool ShowPinButton
    {
        get => GetValue(ShowPinButtonProperty);
        set => SetValue(ShowPinButtonProperty, value);
    }
    
    /// <summary>
    /// Shows a spear that allows you to hide the window.
    /// </summary>
    public bool ShowHiddenButton
    {
        get => GetValue(ShowHiddenButtonProperty);
        set => SetValue(ShowHiddenButtonProperty, value);
    }
    
    /// <summary>
    /// Shows the self-created TitleBar and hides the system TitleBar.
    /// </summary>
    public bool EnableCustomTitleBar
    {
        get => GetValue(EnableCustomTitleBarProperty);
        set => SetValue(EnableCustomTitleBarProperty, value);
    }
    
    /// <inheritdoc cref="StyleKeyOverride"/>
    protected override Type StyleKeyOverride => typeof(PleasantMiniWindow);

    /// <inheritdoc cref="OnApplyTemplate"/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindows = e.NameScope.Get<Panel>("PART_ModalWindow");

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _hiddenButton = e.NameScope.Find<Button>("PART_HiddenButton");
        _dragWindowPanel = e.NameScope.Find<Panel>("PART_DragWindow");
        
        VisualLayerManager = e.NameScope.Get<VisualLayerManager>("PART_VisualLayerManager");

        if (_closeButton is not null)
            _closeButton.Click += (_, _) => Close();
        if (_hiddenButton is not null)
            _hiddenButton.Click += (_, _) => WindowState = WindowState.Minimized;

        ExtendClientAreaToDecorationsHint = PleasantSettings.Instance.WindowSettings.EnableCustomTitleBar;

        if (_dragWindowPanel is not null)
            _dragWindowPanel.PointerPressed += OnDragWindowBorderOnPointerPressed;

        this.GetObservable(EnableCustomTitleBarProperty)
            .Subscribe(val => { ExtendClientAreaToDecorationsHint = val; });
        this.GetObservable(CanResizeProperty).Subscribe(canResize =>
        {
            ExtendClientAreaTitleBarHeightHint = canResize ? 8 : 1;
        });
    }

    /// <inheritdoc cref="OnPropertyChanged"/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if (change.Property == EnableBlurProperty)
        {
            if (EnableBlur)
            {
                TransparencyLevelHint = new[]
                {
                    WindowTransparencyLevel.AcrylicBlur,
                    WindowTransparencyLevel.Blur,
                    WindowTransparencyLevel.None
                };
            }
            else
            {
                TransparencyLevelHint = new[]
                {
                    WindowTransparencyLevel.None
                };
            }
        }
    }

    private void OnDragWindowBorderOnPointerPressed(object? _, PointerPressedEventArgs args) => BeginMoveDrag(args);

    /// <summary>
    /// Gets the list of currently opened modal windows.
    /// </summary>
    /// <value>
    /// The list of currently opened modal windows.
    /// </value>
    public AvaloniaList<PleasantModalWindow> ModalWindows { get; } = [];

    /// <summary>
    /// Gets the <see cref="VisualLayerManager"/> property.
    /// </summary>
    /// <value>The <see cref="VisualLayerManager"/>.</value>
    public VisualLayerManager VisualLayerManager { get; private set; }

    /// <summary>
    /// Adds a modal window to the application.
    /// </summary>
    /// <param name="modalWindow">The <see cref="PleasantModalWindow"/> instance to be added.</param>
    public void AddModalWindow(PleasantModalWindow modalWindow)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(modalWindow.ModalBackground);
        windowPanel.Children.Add(modalWindow);
        
        ModalWindows.Add(modalWindow);

        _modalWindows.Children.Add(windowPanel);
    }

    /// <summary>
    /// Removes a modal window from the collection of opened modal windows and removes it from the parent panel.
    /// </summary>
    /// <param name="modalWindow">The modal window to be removed.</param>
    /// <exception cref="NullReferenceException">Thrown when the parent of the modal window is null.</exception>
    public void RemoveModalWindow(PleasantModalWindow modalWindow)
    {
        ModalWindows.Remove(modalWindow);
        _modalWindows.Children.Remove(modalWindow.Parent as Panel ?? throw new NullReferenceException());
    }
}