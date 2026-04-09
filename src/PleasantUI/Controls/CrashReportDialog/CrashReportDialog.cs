using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using PleasantUI.Core.Helpers;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// The result returned when a <see cref="CrashReportDialog"/> closes.
/// </summary>
public enum CrashReportResult
{
    /// <summary>The user dismissed the dialog without sending.</summary>
    Cancelled,
    /// <summary>The report was sent successfully.</summary>
    Sent,
    /// <summary>The report was saved to disk.</summary>
    Saved
}

/// <summary>
/// Provides data for the <see cref="CrashReportDialog.SendReportRequested"/> event.
/// </summary>
public sealed class SendReportEventArgs : EventArgs
{
    /// <summary>Gets the user-supplied email address (may be empty).</summary>
    public string Email { get; }

    /// <summary>Gets the user-supplied message.</summary>
    public string UserMessage { get; }

    /// <summary>Gets whether the screenshot should be included.</summary>
    public bool IncludeScreenshot { get; }

    /// <summary>
    /// Call this to signal that the send operation completed successfully.
    /// The dialog will transition to its success state.
    /// </summary>
    public Action? ReportSuccess { get; set; }

    /// <summary>
    /// Call this with an error message to signal failure.
    /// The dialog will transition to its failure state.
    /// </summary>
    public Action<string>? ReportFailure { get; set; }

    internal SendReportEventArgs(string email, string userMessage, bool includeScreenshot)
    {
        Email             = email;
        UserMessage       = userMessage;
        IncludeScreenshot = includeScreenshot;
    }
}

/// <summary>
/// Provides data for the <see cref="CrashReportDialog.SaveReportRequested"/> event.
/// </summary>
public sealed class SaveReportEventArgs : EventArgs
{
    /// <summary>Gets the user-supplied message to embed in the report.</summary>
    public string UserMessage { get; }

    internal SaveReportEventArgs(string userMessage) => UserMessage = userMessage;
}

/// <summary>
/// A Fluent-styled modal crash-report dialog.
/// Displays three tabs — General, Exception, and Screenshot — and provides
/// Send Report, Save Report, and Cancel actions.
/// Extends <see cref="PleasantPopupElement"/> so it integrates with the
/// PleasantUI overlay system exactly like <see cref="ContentDialog"/>.
/// </summary>
[TemplatePart(PART_SendButton,       typeof(Button))]
[TemplatePart(PART_SaveButton,       typeof(Button))]
[TemplatePart(PART_CancelButton,     typeof(Button))]
[TemplatePart(PART_EmailBox,         typeof(TextBox))]
[TemplatePart(PART_UserMessageBox,   typeof(TextBox))]
[TemplatePart(PART_ScreenshotToggle, typeof(CheckBox))]
[TemplatePart(PART_TabStrip,         typeof(ListBox))]
[TemplatePart(PART_TabContent,       typeof(ContentPresenter))]
[PseudoClasses(PC_Sending, PC_Success, PC_Failure, PC_HasScreenshot, PC_EmailRequired, PC_EmailInvalid)]
public class CrashReportDialog : PleasantPopupElement
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_SendButton       = "PART_SendButton";
    internal const string PART_SaveButton       = "PART_SaveButton";
    internal const string PART_CancelButton     = "PART_CancelButton";
    internal const string PART_EmailBox         = "PART_EmailBox";
    internal const string PART_UserMessageBox   = "PART_UserMessageBox";
    internal const string PART_ScreenshotToggle = "PART_ScreenshotToggle";
    internal const string PART_TabStrip         = "PART_TabStrip";
    internal const string PART_TabContent       = "PART_TabContent";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_Sending       = ":sending";
    private const string PC_Success       = ":success";
    private const string PC_Failure       = ":failure";
    private const string PC_HasScreenshot = ":hasScreenshot";
    private const string PC_EmailRequired = ":emailRequired";
    private const string PC_EmailInvalid  = ":emailInvalid";

    // ── Email regex ───────────────────────────────────────────────────────────

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="ApplicationName"/> property.</summary>
    public static readonly StyledProperty<string?> ApplicationNameProperty =
        AvaloniaProperty.Register<CrashReportDialog, string?>(nameof(ApplicationName));

    /// <summary>Defines the <see cref="ApplicationVersion"/> property.</summary>
    public static readonly StyledProperty<string?> ApplicationVersionProperty =
        AvaloniaProperty.Register<CrashReportDialog, string?>(nameof(ApplicationVersion));

    /// <summary>Defines the <see cref="ExceptionType"/> property.</summary>
    public static readonly StyledProperty<string?> ExceptionTypeProperty =
        AvaloniaProperty.Register<CrashReportDialog, string?>(nameof(ExceptionType));

    /// <summary>Defines the <see cref="ExceptionMessage"/> property.</summary>
    public static readonly StyledProperty<string?> ExceptionMessageProperty =
        AvaloniaProperty.Register<CrashReportDialog, string?>(nameof(ExceptionMessage));

    /// <summary>Defines the <see cref="ExceptionSource"/> property.</summary>
    public static readonly StyledProperty<string?> ExceptionSourceProperty =
        AvaloniaProperty.Register<CrashReportDialog, string?>(nameof(ExceptionSource));

    /// <summary>Defines the <see cref="StackTrace"/> property.</summary>
    public static readonly StyledProperty<string?> StackTraceProperty =
        AvaloniaProperty.Register<CrashReportDialog, string?>(nameof(StackTrace));

    /// <summary>Defines the <see cref="OccurredAt"/> property.</summary>
    public static readonly StyledProperty<string?> OccurredAtProperty =
        AvaloniaProperty.Register<CrashReportDialog, string?>(nameof(OccurredAt));

    /// <summary>Defines the <see cref="Screenshot"/> property.</summary>
    public static readonly StyledProperty<Bitmap?> ScreenshotProperty =
        AvaloniaProperty.Register<CrashReportDialog, Bitmap?>(nameof(Screenshot));

    /// <summary>Defines the <see cref="IncludeScreenshot"/> property.</summary>
    public static readonly StyledProperty<bool> IncludeScreenshotProperty =
        AvaloniaProperty.Register<CrashReportDialog, bool>(nameof(IncludeScreenshot), defaultValue: true);

    /// <summary>Defines the <see cref="IsEmailRequired"/> property.</summary>
    public static readonly StyledProperty<bool> IsEmailRequiredProperty =
        AvaloniaProperty.Register<CrashReportDialog, bool>(nameof(IsEmailRequired));

    /// <summary>Defines the <see cref="ShowScreenshotTab"/> property.</summary>
    public static readonly StyledProperty<bool> ShowScreenshotTabProperty =
        AvaloniaProperty.Register<CrashReportDialog, bool>(nameof(ShowScreenshotTab), defaultValue: true);

    /// <summary>Defines the <see cref="StatusMessage"/> property.</summary>
    public static readonly StyledProperty<string?> StatusMessageProperty =
        AvaloniaProperty.Register<CrashReportDialog, string?>(nameof(StatusMessage));

    /// <summary>Defines the <see cref="OpenAnimation"/> property.</summary>
    public static readonly StyledProperty<Animation?> OpenAnimationProperty =
        AvaloniaProperty.Register<CrashReportDialog, Animation?>(nameof(OpenAnimation));

    /// <summary>Defines the <see cref="CloseAnimation"/> property.</summary>
    public static readonly StyledProperty<Animation?> CloseAnimationProperty =
        AvaloniaProperty.Register<CrashReportDialog, Animation?>(nameof(CloseAnimation));

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the application name shown in the General tab.</summary>
    public string? ApplicationName
    {
        get => GetValue(ApplicationNameProperty);
        set => SetValue(ApplicationNameProperty, value);
    }

    /// <summary>Gets or sets the application version shown in the General tab.</summary>
    public string? ApplicationVersion
    {
        get => GetValue(ApplicationVersionProperty);
        set => SetValue(ApplicationVersionProperty, value);
    }

    /// <summary>Gets or sets the fully-qualified exception type name.</summary>
    public string? ExceptionType
    {
        get => GetValue(ExceptionTypeProperty);
        set => SetValue(ExceptionTypeProperty, value);
    }

    /// <summary>Gets or sets the exception message shown in both tabs.</summary>
    public string? ExceptionMessage
    {
        get => GetValue(ExceptionMessageProperty);
        set => SetValue(ExceptionMessageProperty, value);
    }

    /// <summary>Gets or sets the exception source assembly/module.</summary>
    public string? ExceptionSource
    {
        get => GetValue(ExceptionSourceProperty);
        set => SetValue(ExceptionSourceProperty, value);
    }

    /// <summary>Gets or sets the full stack trace (inner exception + trace).</summary>
    public string? StackTrace
    {
        get => GetValue(StackTraceProperty);
        set => SetValue(StackTraceProperty, value);
    }

    /// <summary>Gets or sets the formatted date/time when the crash occurred.</summary>
    public string? OccurredAt
    {
        get => GetValue(OccurredAtProperty);
        set => SetValue(OccurredAtProperty, value);
    }

    /// <summary>Gets or sets the screenshot bitmap (null hides the Screenshot tab).</summary>
    public Bitmap? Screenshot
    {
        get => GetValue(ScreenshotProperty);
        set => SetValue(ScreenshotProperty, value);
    }

    /// <summary>Gets or sets whether the screenshot should be included in the report.</summary>
    public bool IncludeScreenshot
    {
        get => GetValue(IncludeScreenshotProperty);
        set => SetValue(IncludeScreenshotProperty, value);
    }

    /// <summary>Gets or sets whether a valid email address is required before sending.</summary>
    public bool IsEmailRequired
    {
        get => GetValue(IsEmailRequiredProperty);
        set => SetValue(IsEmailRequiredProperty, value);
    }

    /// <summary>Gets or sets whether the Screenshot tab is shown.</summary>
    public bool ShowScreenshotTab
    {
        get => GetValue(ShowScreenshotTabProperty);
        set => SetValue(ShowScreenshotTabProperty, value);
    }

    /// <summary>Gets or sets the status message shown in the sending/result state.</summary>
    public string? StatusMessage
    {
        get => GetValue(StatusMessageProperty);
        set => SetValue(StatusMessageProperty, value);
    }

    /// <summary>Gets or sets the open animation.</summary>
    public Animation? OpenAnimation
    {
        get => GetValue(OpenAnimationProperty);
        set => SetValue(OpenAnimationProperty, value);
    }

    /// <summary>Gets or sets the close animation.</summary>
    public Animation? CloseAnimation
    {
        get => GetValue(CloseAnimationProperty);
        set => SetValue(CloseAnimationProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Raised when the user clicks Send Report.
    /// The handler must call <see cref="SendReportEventArgs.ReportSuccess"/> or
    /// <see cref="SendReportEventArgs.ReportFailure"/> to advance the dialog state.
    /// </summary>
    public event EventHandler<SendReportEventArgs>? SendReportRequested;

    /// <summary>Raised when the user clicks Save Report.</summary>
    public event EventHandler<SaveReportEventArgs>? SaveReportRequested;

    /// <summary>Raised when the dialog closes.</summary>
    public event EventHandler? Closed;

    // ── Private state ─────────────────────────────────────────────────────────

    private Button?   _sendButton;
    private Button?   _saveButton;
    private Button?   _cancelButton;
    private TextBox?  _emailBox;
    private TextBox?  _userMessageBox;
    private CheckBox? _screenshotToggle;
    private ListBox?  _tabStrip;

    private Border?  _modalBackground;
    private Panel?   _panel;
    private bool     _isClosing;
    private CrashReportResult _result = CrashReportResult.Cancelled;

    private AvaloniaList<PleasantPopupElement>? _modalWindows;
    private IInputElement? _lastFocus;

    // Tab items — built once and reused.
    private readonly List<string> _tabs = new();

    // ── Convenience factory ───────────────────────────────────────────────────

    /// <summary>
    /// Creates a pre-populated <see cref="CrashReportDialog"/> from an <see cref="Exception"/>.
    /// </summary>
    public static CrashReportDialog FromException(
        Exception ex,
        string? applicationName    = null,
        string? applicationVersion = null,
        Bitmap? screenshot         = null)
    {
        var sb = new StringBuilder();
        if (ex.InnerException is not null)
            sb.AppendLine(ex.InnerException.ToString());
        sb.Append(ex.StackTrace);

        return new CrashReportDialog
        {
            ApplicationName    = applicationName,
            ApplicationVersion = applicationVersion,
            ExceptionType      = ex.GetType().FullName,
            ExceptionMessage   = ex.Message,
            ExceptionSource    = ex.Source,
            StackTrace         = sb.ToString(),
            OccurredAt         = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Screenshot         = screenshot,
            ShowScreenshotTab  = screenshot is not null
        };
    }

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();

        _sendButton       = e.NameScope.Find<Button>(PART_SendButton);
        _saveButton       = e.NameScope.Find<Button>(PART_SaveButton);
        _cancelButton     = e.NameScope.Find<Button>(PART_CancelButton);
        _emailBox         = e.NameScope.Find<TextBox>(PART_EmailBox);
        _userMessageBox   = e.NameScope.Find<TextBox>(PART_UserMessageBox);
        _screenshotToggle = e.NameScope.Find<CheckBox>(PART_ScreenshotToggle);
        _tabStrip         = e.NameScope.Find<ListBox>(PART_TabStrip);

        AttachHandlers();
        BuildTabs();
        UpdatePseudoClasses();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ScreenshotProperty)
        {
            PseudoClasses.Set(PC_HasScreenshot, change.NewValue is not null);
            BuildTabs();
        }
        else if (change.Property == ShowScreenshotTabProperty)
        {
            BuildTabs();
        }
        else if (change.Property == IsEmailRequiredProperty)
        {
            PseudoClasses.Set(PC_EmailRequired, change.GetNewValue<bool>());
        }
    }

    // ── Show / Close API ──────────────────────────────────────────────────────

    /// <summary>Shows the dialog and returns the result when it closes.</summary>
    public Task<CrashReportResult> ShowAsync(TopLevel? topLevel = null)
        => ShowCoreAsync(topLevel);

    /// <summary>Shows the dialog on a <see cref="Window"/>.</summary>
    public Task<CrashReportResult> ShowAsync(Window window)
        => ShowCoreAsync(window);

    /// <summary>Shows the dialog on an <see cref="IPleasantWindow"/>.</summary>
    public Task<CrashReportResult> ShowAsync(IPleasantWindow pleasantWindow)
        => ShowCoreAsync(pleasantWindow as TopLevel);

    /// <summary>Closes the dialog programmatically.</summary>
    public async Task CloseAsync()
    {
        if (_isClosing) return;
        _isClosing = true;

        IsHitTestVisible = false;

        if (_modalBackground is not null)
            _modalBackground.IsHitTestVisible = false;

        if (CloseAnimation is not null)
            await CloseAnimation.RunAsync(this);

        _modalWindows?.Remove(this);

        if (_lastFocus is not null)
        {
            _lastFocus.Focus();
            _lastFocus = null;
        }

        base.DeleteCoreForTopLevel();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<CrashReportResult> ShowCoreAsync(TopLevel? topLevel)
    {
        _modalWindows = WindowHelper.GetModalWindows(topLevel);

        var tcs = new TaskCompletionSource<CrashReportResult>();

        _panel = new Panel();

        _modalBackground = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#3A000000")),
            Opacity    = 0
        };

        _panel.Children.Add(_modalBackground);
        _panel.Children.Add(this);

        Host ??= new ModalWindowHost();
        Host.Content = _panel;

        base.ShowCoreForTopLevel(topLevel);

        _modalWindows?.Add(this);
        _lastFocus = topLevel?.FocusManager?.GetFocusedElement();

        Closed += (_, _) =>
        {
            try   { tcs.TrySetResult(_result); }
            catch { tcs.TrySetResult(CrashReportResult.Cancelled); }
        };

        return await tcs.Task;
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        try
        {
            if (_modalBackground is not null)
            {
                var bgAnim = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(200),
                    FillMode = Avalonia.Animation.FillMode.Forward
                };
                var kf = new KeyFrame { Cue = new Cue(1.0) };
                kf.Setters.Add(new Setter(OpacityProperty, 1.0));
                bgAnim.Children.Add(kf);
                await bgAnim.RunAsync(_modalBackground);
            }

            if (OpenAnimation is not null)
                await OpenAnimation.RunAsync(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CrashReportDialog] Animation error: {ex.Message}");
        }

        _emailBox?.Focus();
    }

    private void AttachHandlers()
    {
        if (_sendButton   is not null) _sendButton.Click   += OnSendClicked;
        if (_saveButton   is not null) _saveButton.Click   += OnSaveClicked;
        if (_cancelButton is not null) _cancelButton.Click += OnCancelClicked;
        if (_tabStrip     is not null) _tabStrip.SelectionChanged += OnTabSelectionChanged;
    }

    private void DetachHandlers()
    {
        if (_sendButton   is not null) _sendButton.Click   -= OnSendClicked;
        if (_saveButton   is not null) _saveButton.Click   -= OnSaveClicked;
        if (_cancelButton is not null) _cancelButton.Click -= OnCancelClicked;
        if (_tabStrip     is not null) _tabStrip.SelectionChanged -= OnTabSelectionChanged;
    }

    private void BuildTabs()
    {
        _tabs.Clear();
        _tabs.Add("General");
        _tabs.Add("Exception");
        if (ShowScreenshotTab && Screenshot is not null)
            _tabs.Add("Screenshot");

        if (_tabStrip is not null)
        {
            _tabStrip.ItemsSource = _tabs;
            if (_tabStrip.SelectedIndex < 0 || _tabStrip.SelectedIndex >= _tabs.Count)
                _tabStrip.SelectedIndex = 0;
        }

        // Sync Tag so styles activate immediately.
        Tag = _tabs.Count > 0 ? _tabs[0] : "General";
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_HasScreenshot, Screenshot is not null);
        PseudoClasses.Set(PC_EmailRequired, IsEmailRequired);
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void OnSendClicked(object? s, RoutedEventArgs e)
    {
        var email = _emailBox?.Text?.Trim() ?? string.Empty;

        // Validate email.
        if (string.IsNullOrEmpty(email))
        {
            if (IsEmailRequired)
            {
                PseudoClasses.Set(PC_EmailInvalid, true);
                _emailBox?.Focus();
                return;
            }
        }
        else if (!EmailRegex.IsMatch(email))
        {
            PseudoClasses.Set(PC_EmailInvalid, true);
            _emailBox?.Focus();
            return;
        }

        PseudoClasses.Set(PC_EmailInvalid, false);

        var userMessage       = _userMessageBox?.Text?.Trim() ?? string.Empty;
        var includeScreenshot = _screenshotToggle?.IsChecked == true;

        // Transition to sending state.
        PseudoClasses.Set(PC_Sending, true);
        StatusMessage = "Sending report…";

        SetButtonsEnabled(false);

        var args = new SendReportEventArgs(email, userMessage, includeScreenshot)
        {
            ReportSuccess = () => Dispatcher.UIThread.Post(() =>
            {
                PseudoClasses.Set(PC_Sending, false);
                PseudoClasses.Set(PC_Success, true);
                StatusMessage = "Report sent successfully. Thank you!";
                _result = CrashReportResult.Sent;
                SetButtonsEnabled(true);
            }),
            ReportFailure = msg => Dispatcher.UIThread.Post(() =>
            {
                PseudoClasses.Set(PC_Sending, false);
                PseudoClasses.Set(PC_Failure, true);
                StatusMessage = $"Failed to send report: {msg}";
                _result = CrashReportResult.Cancelled;
                SetButtonsEnabled(true);
            })
        };

        SendReportRequested?.Invoke(this, args);
    }

    private void OnSaveClicked(object? s, RoutedEventArgs e)
    {
        var userMessage = _userMessageBox?.Text?.Trim() ?? string.Empty;
        _result = CrashReportResult.Saved;
        SaveReportRequested?.Invoke(this, new SaveReportEventArgs(userMessage));
    }

    private void OnCancelClicked(object? s, RoutedEventArgs e)
    {
        _result = CrashReportResult.Cancelled;
        _ = CloseAsync();
    }

    private void OnTabSelectionChanged(object? s, SelectionChangedEventArgs e)
    {
        // Drive tab visibility via the Tag property so AXAML styles can react.
        var selected = _tabStrip?.SelectedItem as string ?? "General";
        Tag = selected;
    }

    private void SetButtonsEnabled(bool enabled)
    {
        if (_sendButton   is not null) _sendButton.IsEnabled   = enabled;
        if (_saveButton   is not null) _saveButton.IsEnabled   = enabled;
        if (_cancelButton is not null) _cancelButton.IsEnabled = enabled;
    }
}
