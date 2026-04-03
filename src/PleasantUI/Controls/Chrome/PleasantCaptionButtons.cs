using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Reactive;
using PleasantUI.Core.Internal.Reactive;

namespace PleasantUI.Controls.Chrome;

/// <summary>
/// Represents a set of caption buttons for a <see cref="PleasantWindow" />.
/// </summary>
[PseudoClasses(":minimized", ":normal", ":maximized", ":fullscreen", ":isactive")]
[TemplatePart("PART_CloseButton", typeof(Button))]
[TemplatePart("PART_MaximizeButton", typeof(Button))]
[TemplatePart("PART_MinimizeButton", typeof(Button))]
[TemplatePart("PART_FullScreenButton", typeof(Button))]
public class PleasantCaptionButtons : TemplatedControl
{
    /// <summary>
    /// Represents the types of pleasant caption buttons.
    /// </summary>
    public enum Type
    {
        /// <summary>
        /// All buttons are visible (Close, Collapse, Expand).
        /// </summary>
        All = 0,

        /// <summary>
        /// Close and Collapse buttons are visible.
        /// </summary>
        CloseAndCollapse = 1,

        /// <summary>
        /// Close and Expand buttons are visible.
        /// </summary>
        CloseAndExpand = 2,

        /// <summary>
        /// Only the Close button is visible.
        /// </summary>
        Close = 3,

        /// <summary>
        /// No buttons are visible.
        /// </summary>
        None = 4
    }

    private Button? _closeButton;
    private CompositeDisposable? _disposable;

    private Button? _maximizeButton;
    private Button? _minimizeButton;
    private Button? _fullScreenButton;

    /// <summary>
    /// Stores the previous window state before entering full-screen mode.
    /// </summary>
    private WindowState? _oldWindowState;

    /// <summary>
    /// Gets or sets the host window for these caption buttons.
    /// </summary>
    public PleasantWindow? Host { get; set; }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Get<Button>("PART_CloseButton");
        _maximizeButton = e.NameScope.Get<Button>("PART_MaximizeButton");
        _minimizeButton = e.NameScope.Get<Button>("PART_MinimizeButton");
        _fullScreenButton = e.NameScope.Find<Button>("PART_FullScreenButton");

        _closeButton.Click += (_, _) => Host?.Close();
        _maximizeButton.Click += (_, _) =>
        {
            if (Host == null) return;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Host.WindowState = Host.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
            else
                Host.WindowState = Host.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        };
        _minimizeButton.Click += (_, _) =>
        {
            if (Host == null) return;
            Host.WindowState = WindowState.Minimized;
        };
        if (_fullScreenButton is not null)
            _fullScreenButton.Click += (_, _) => OnToggleFullScreen();

        if (_disposable == null && Host is not null)
        {
            _disposable = new CompositeDisposable
            {
                Host.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(state =>
                {
                    PseudoClasses.Set(":minimized", state == WindowState.Minimized);
                    PseudoClasses.Set(":normal", state == WindowState.Normal);
                    PseudoClasses.Set(":maximized", state == WindowState.Maximized);
                    PseudoClasses.Set(":fullscreen", state == WindowState.FullScreen);

                    UpdateButtonVisibility();
                })),

                Host.GetObservable(Window.CanResizeProperty).Subscribe(new AnonymousObserver<bool>(_ => UpdateButtonVisibility())),

                Host.GetObservable(WindowBase.IsActiveProperty).Subscribe(new AnonymousObserver<bool>(isActive => PseudoClasses.Set(":isactive", !isActive))),

                Host.GetObservable(PleasantWindow.CaptionButtonsProperty).Subscribe(new AnonymousObserver<Type>(_ => UpdateButtonVisibility())),

                Host.GetObservable(PleasantWindow.IsCloseButtonVisibleProperty).Subscribe(new AnonymousObserver<bool>(_ => UpdateButtonVisibility())),
                Host.GetObservable(PleasantWindow.IsMinimizeButtonVisibleProperty).Subscribe(new AnonymousObserver<bool>(_ => UpdateButtonVisibility())),
                Host.GetObservable(PleasantWindow.IsRestoreButtonVisibleProperty).Subscribe(new AnonymousObserver<bool>(_ => UpdateButtonVisibility())),
                Host.GetObservable(PleasantWindow.IsFullScreenButtonVisibleProperty).Subscribe(new AnonymousObserver<bool>(_ => UpdateButtonVisibility())),
                Host.GetObservable(Window.CanMinimizeProperty).Subscribe(new AnonymousObserver<bool>(_ => UpdateButtonVisibility()))
            };

            UpdateButtonVisibility();
        }
    }

    /// <summary>
    /// Toggles full-screen mode, restoring the previous window state when leaving full-screen.
    /// </summary>
    protected virtual void OnToggleFullScreen()
    {
        if (Host is null) return;

        if (Host.WindowState != WindowState.FullScreen)
        {
            _oldWindowState = Host.WindowState;
            Host.WindowState = WindowState.FullScreen;
        }
        else
        {
            Host.WindowState = _oldWindowState ?? WindowState.Normal;
            _oldWindowState = null;
        }
    }

    private void UpdateButtonVisibility()
    {
        if (Host is null || _closeButton is null || _maximizeButton is null || _minimizeButton is null)
            return;

        var state = Host.WindowState;
        bool isFullScreen = state == WindowState.FullScreen;

        // In fullscreen: only Close and FullScreen button are relevant
        if (isFullScreen)
        {
            _minimizeButton.IsVisible = false;
            _maximizeButton.IsVisible = false;
            _closeButton.IsVisible = Host.IsCloseButtonVisible;
            if (_fullScreenButton is not null)
                _fullScreenButton.IsVisible = Host.IsFullScreenButtonVisible;
            return;
        }

        // Apply CaptionButtons enum (coarse-grained control)
        switch (Host.CaptionButtons)
        {
            case Type.None:
                _closeButton.IsVisible = _maximizeButton.IsVisible = _minimizeButton.IsVisible = false;
                if (_fullScreenButton is not null) _fullScreenButton.IsVisible = false;
                return;
            case Type.Close:
                _minimizeButton.IsVisible = false;
                _maximizeButton.IsVisible = false;
                _closeButton.IsVisible = Host.IsCloseButtonVisible;
                if (_fullScreenButton is not null) _fullScreenButton.IsVisible = false;
                return;
            case Type.CloseAndCollapse:
                _maximizeButton.IsVisible = false;
                _minimizeButton.IsVisible = Host.CanMinimize && Host.IsMinimizeButtonVisible;
                _closeButton.IsVisible = Host.IsCloseButtonVisible;
                if (_fullScreenButton is not null) _fullScreenButton.IsVisible = false;
                return;
            case Type.CloseAndExpand:
                _minimizeButton.IsVisible = false;
                _maximizeButton.IsVisible = Host.CanResize && Host.IsRestoreButtonVisible;
                _closeButton.IsVisible = Host.IsCloseButtonVisible;
                if (_fullScreenButton is not null) _fullScreenButton.IsVisible = false;
                return;
        }

        // Type.All — respect per-button visibility properties
        _closeButton.IsVisible = Host.IsCloseButtonVisible;
        _maximizeButton.IsVisible = Host.CanResize && Host.IsRestoreButtonVisible;
        _minimizeButton.IsVisible = Host.CanMinimize && Host.IsMinimizeButtonVisible;
        if (_fullScreenButton is not null)
            _fullScreenButton.IsVisible = Host.IsFullScreenButtonVisible;
    }

    /// <summary>
    /// Detaches the object from its host by disposing the disposable object and setting it to null.
    /// Also sets the host object to null.
    /// </summary>
    public void Detach()
    {
        if (_disposable is null) return;

        _disposable.Dispose();
        _disposable = null;
        Host = null;
    }
}