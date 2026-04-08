using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Reactive;
using Avalonia.Platform;
using Avalonia.Threading;
using PleasantUI.Core;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// Represents the base class for PleasantUI windows, providing support for modal windows 
/// and a snackbar queue manager.
/// </summary>
public abstract class PleasantWindowBase : Window, IPleasantWindow
{
    /// <summary>
    /// Gets the snackbar queue manager for pleasant snackbars.
    /// </summary>
    public SnackbarQueueManager<PleasantSnackbar> SnackbarQueueManager { get; } = new();

    /// <summary>
    /// Gets the collection of modal windows associated with this window.
    /// </summary>
    public AvaloniaList<PleasantPopupElement> ModalWindows { get; } = [];

    private INotifyPropertyChanged? _settingsNotify;
    private IDisposable? _windowStateSub;
    private IDisposable? _isActiveSub;

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        HookSettings();
        HookWindowState();
        UpdateWin11CornerPreferenceDeferred();
    }

    protected override void OnClosed(EventArgs e)
    {
        UnhookSettings();
        UnhookWindowState();
        base.OnClosed(e);
    }

    private void HookWindowState()
    {
        if (_windowStateSub is null)
        {
            _windowStateSub = this.GetObservable(WindowStateProperty)
                .Subscribe(new AnonymousObserver<WindowState>(_ => UpdateWin11CornerPreferenceDeferred()));
        }

        if (_isActiveSub is null)
        {
            _isActiveSub = this.GetObservable(IsActiveProperty)
                .Subscribe(new AnonymousObserver<bool>(_ => UpdateWin11CornerPreferenceDeferred()));
        }
    }

    private void UnhookWindowState()
    {
        _windowStateSub?.Dispose();
        _windowStateSub = null;

        _isActiveSub?.Dispose();
        _isActiveSub = null;
    }

    private void HookSettings()
    {
        if (_settingsNotify is not null)
            return;

        if (PleasantSettings.Current is null)
            return;

        _settingsNotify = PleasantSettings.Current;
        _settingsNotify.PropertyChanged += OnSettingsPropertyChanged;
    }

    private void UnhookSettings()
    {
        if (_settingsNotify is null)
            return;

        _settingsNotify.PropertyChanged -= OnSettingsPropertyChanged;
        _settingsNotify = null;
    }

    private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PleasantSettings.Theme))
            UpdateWin11CornerPreferenceDeferred();
    }

    private void UpdateWin11CornerPreferenceDeferred()
    {
        Dispatcher.UIThread.Post(UpdateWin11CornerPreference, DispatcherPriority.Background);
    }

    private void UpdateWin11CornerPreference()
    {
        if (!OperatingSystem.IsWindows())
            return;

        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            return;

        var theme = PleasantSettings.Current?.Theme;
        bool isVgui = string.Equals(theme, "VGUI", StringComparison.Ordinal);

        var handle = TryGetPlatformHandle();
        if (handle is null)
            return;

        SetWindowCornerPreference(handle.Handle, isVgui ? DwmWindowCornerPreference.DWMWCP_DONOTROUND : DwmWindowCornerPreference.DWMWCP_DEFAULT);
    }

    private static void SetWindowCornerPreference(IntPtr hwnd, DwmWindowCornerPreference preference)
    {
        int pref = (int)preference;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref pref, Marshal.SizeOf<int>());
    }

    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

    private enum DwmWindowCornerPreference
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
}