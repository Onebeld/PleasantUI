using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PleasantUI.Controls.Chrome;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

public class PleasantWindow : Window, IPleasantWindow
{
    private Panel _modalWindowsPanel = null!;
    
    public PleasantTitleBar? TitleBar;

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
    
    public Control? LeftTitleContent
    {
        get => GetValue(LeftTitleContentProperty);
        set => SetValue(LeftTitleContentProperty, value);
    }

    public bool EnableTitleBarMargin
    {
        get => GetValue(EnableTitleBarMarginProperty);
        set => SetValue(EnableTitleBarMarginProperty, value);
    }

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

    protected override Type StyleKeyOverride => typeof(PleasantWindow);
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindowsPanel = e.NameScope.Get<Panel>("PART_ModalWindowsPanel");
        
        this.GetObservable(EnableCustomTitleBarProperty)
            .Subscribe(val => { ExtendClientAreaToDecorationsHint = val; });
    }

    public void AddModalWindow(PleasantModalWindow modalWindow, Animation animation)
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