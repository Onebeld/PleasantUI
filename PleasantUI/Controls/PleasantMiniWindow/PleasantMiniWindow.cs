﻿using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PleasantUI.Core;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

public class PleasantMiniWindow : Window, IPleasantWindow
{
    private Panel _modalWindows = null!;

    private Button? _hiddenButton;
    private Button? _closeButton;
    private Panel? _dragWindowPanel;

    public static readonly StyledProperty<bool> EnableTransparencyProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(EnableTransparency));
    public static readonly StyledProperty<bool> ShowPinButtonProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(ShowPinButton), true);
    public static readonly StyledProperty<bool> ShowHiddenButtonProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(ShowHiddenButton));
    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantMiniWindow, bool>(nameof(EnableCustomTitleBar), true);

    public bool EnableTransparency
    {
        get => GetValue(EnableTransparencyProperty);
        set => SetValue(EnableTransparencyProperty, value);
    }
    public bool ShowPinButton
    {
        get => GetValue(ShowPinButtonProperty);
        set => SetValue(ShowPinButtonProperty, value);
    }
    public bool ShowHiddenButton
    {
        get => GetValue(ShowHiddenButtonProperty);
        set => SetValue(ShowHiddenButtonProperty, value);
    }
    public bool EnableCustomTitleBar
    {
        get => GetValue(EnableCustomTitleBarProperty);
        set => SetValue(EnableCustomTitleBarProperty, value);
    }
    
    protected override Type StyleKeyOverride => typeof(PleasantMiniWindow);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindows = e.NameScope.Get<Panel>("PART_ModalWindow");

        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        _hiddenButton = e.NameScope.Find<Button>("PART_HiddenButton");
        _dragWindowPanel = e.NameScope.Find<Panel>("PART_DragWindow");

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

    private void OnDragWindowBorderOnPointerPressed(object? _, PointerPressedEventArgs args)
    {
        BeginMoveDrag(args);
    }

    public void AddModalWindow(PleasantModalWindow modalWindow)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(modalWindow.ModalBackground);
        windowPanel.Children.Add(modalWindow);

        _modalWindows.Children.Add(windowPanel);
    }

    public void RemoveModalWindow(PleasantModalWindow modalWindow)
    {
        _modalWindows.Children.Remove(modalWindow.Parent as Panel ?? throw new NullReferenceException());
    }
}