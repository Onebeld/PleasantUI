﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PleasantUI.Controls.Chrome;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Interfaces;
using PleasantUI.Reactive;

namespace PleasantUI.Controls;

public class PleasantWindow : Window, IPleasantWindow
{
    private Panel _modalWindowsPanel = null!;
    
    internal PleasantTitleBar? TitleBar;

    public static readonly StyledProperty<bool> EnableCustomTitleBarProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableCustomTitleBar), true);

    public static readonly StyledProperty<Control?> TitleBarContentProperty =
        AvaloniaProperty.Register<PleasantWindow, Control?>(nameof(TitleBarContent));
    
    public static readonly StyledProperty<Control?> LeftTitleContentProperty =
        AvaloniaProperty.Register<PleasantWindow, Control?>(nameof(LeftTitleContent));

    public static readonly StyledProperty<bool> EnableTitleBarMarginProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableTitleBarMargin), true);

    public static readonly StyledProperty<bool> ShowFullScreenButtonProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(ShowFullScreenButton));

    public static readonly StyledProperty<PleasantTitleBarStyle> TitleBarStyleProperty =
        AvaloniaProperty.Register<PleasantWindow, PleasantTitleBarStyle>(nameof(TitleBarStyle));

    public static readonly StyledProperty<PleasantCaptionButtonsType> CaptionButtonsProperty =
        AvaloniaProperty.Register<PleasantWindow, PleasantCaptionButtonsType>(nameof(CaptionButtons));

    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<PleasantWindow, string>(nameof(Subtitle));

    public static readonly StyledProperty<IImage?> IconImageProperty =
        AvaloniaProperty.Register<PleasantWindow, IImage?>(nameof(IconImage));

    public static readonly StyledProperty<Geometry?> TitleGeometryProperty =
        AvaloniaProperty.Register<PleasantWindow, Geometry?>(nameof(TitleGeometry));

    public static readonly StyledProperty<PleasantTitleBarType> TitleBarTypeProperty =
        AvaloniaProperty.Register<PleasantWindow, PleasantTitleBarType>(nameof(TitleBarType));

    public static readonly StyledProperty<bool> EnableBlurProperty =
        AvaloniaProperty.Register<PleasantWindow, bool>(nameof(EnableBlur));

    /// <summary>
    /// Shows the self-created TitleBar and hides the system TitleBar.
    /// </summary>
    public bool EnableCustomTitleBar
    {
        get => GetValue(EnableCustomTitleBarProperty);
        set => SetValue(EnableCustomTitleBarProperty, value);
    }

    public Control? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }
    
    /// <summary>
    /// Indicates content to the left of the window title.
    /// </summary>
    /// <remarks>
    /// Will not display when <see cref="TitleBarStyle"/> equal MacOS.
    /// </remarks>
    public Control? LeftTitleContent
    {
        get => GetValue(LeftTitleContentProperty);
        set => SetValue(LeftTitleContentProperty, value);
    }

    /// <summary>
    /// Specifies whether TitleBar can indent the main window content. If False, TitleBar overlaps the window content.
    /// </summary>
    public bool EnableTitleBarMargin
    {
        get => GetValue(EnableTitleBarMarginProperty);
        set => SetValue(EnableTitleBarMarginProperty, value);
    }
    
    /// <summary>
    /// Shows the button that allows you to switch the window to full screen mode.
    /// </summary>
    /// <remarks>
    /// This field is not used yet
    /// </remarks>
    public bool ShowFullScreenButton
    {
        get => GetValue(ShowFullScreenButtonProperty);
        set => SetValue(ShowFullScreenButtonProperty, value);
    }

    public PleasantTitleBarStyle TitleBarStyle
    {
        get => GetValue(TitleBarStyleProperty);
        set => SetValue(TitleBarStyleProperty, value);
    }
    
    /// <summary>
    /// Specifies which buttons will be displayed in the window.
    /// </summary>
    public PleasantCaptionButtonsType CaptionButtons
    {
        get => GetValue(CaptionButtonsProperty);
        set => SetValue(CaptionButtonsProperty, value);
    }

    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public IImage? IconImage
    {
        get => GetValue(IconImageProperty);
        set => SetValue(IconImageProperty, value);
    }

    public Geometry? TitleGeometry
    {
        get => GetValue(TitleGeometryProperty);
        set => SetValue(TitleGeometryProperty, value);
    }

    public PleasantTitleBarType TitleBarType
    {
        get => GetValue(TitleBarTypeProperty);
        set => SetValue(TitleBarTypeProperty, value);
    }

    public bool EnableBlur
    {
        get => GetValue(EnableBlurProperty);
        set => SetValue(EnableBlurProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(PleasantWindow);
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindowsPanel = e.NameScope.Get<Panel>("PART_ModalWindowsPanel");

        TitleBar = e.NameScope.Get<PleasantTitleBar>("PART_PleasantTitleBar");
        
        this.GetObservable(EnableCustomTitleBarProperty)
            .Subscribe(val => { ExtendClientAreaToDecorationsHint = val; });
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == EnableCustomTitleBarProperty)
        {
            ExtendClientAreaToDecorationsHint = EnableCustomTitleBar;
        }

        if (change.Property == EnableTitleBarMarginProperty)
        {
            if (TitleBarStyle == PleasantTitleBarStyle.MacOS)
                EnableTitleBarMargin = true;
        }

        if (change.Property == EnableBlurProperty)
        {
            if (EnableBlur)
            {
                TransparencyLevelHint = new[]
                {
                    WindowTransparencyLevel.Mica,
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

    public void AddModalWindow(PleasantModalWindow modalWindow)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(modalWindow.ModalBackground);
        windowPanel.Children.Add(modalWindow);
        
        _modalWindowsPanel.Children.Add(windowPanel);
    }

    public void RemoveModalWindow(PleasantModalWindow modalWindow)
    {
        _modalWindowsPanel.Children.Remove(modalWindow.Parent as Panel ?? throw new NullReferenceException());
    }
}