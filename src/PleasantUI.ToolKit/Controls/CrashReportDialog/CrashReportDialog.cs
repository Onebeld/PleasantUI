using System.Text;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.ToolKit.Controls;

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

    public SendReportEventArgs(string email, string userMessage, bool includeScreenshot)
    {
        Email = email;
        UserMessage = userMessage;
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

    public SaveReportEventArgs(string userMessage) => UserMessage = userMessage;
}

/// <summary>
/// A Fluent-styled modal crash-report dialog.
/// Displays three tabs — General, Exception, and Screenshot — and provides
/// Send Report, Save Report, and Cancel actions.
/// Extends <see cref="PleasantPopupElement"/> so it integrates with the
/// PleasantUI overlay system exactly like <see cref="ContentDialog"/>.
/// </summary>
[TemplatePart(PART_SendButton, typeof(Button))]
[TemplatePart(PART_SaveButton, typeof(Button))]
[TemplatePart(PART_CancelButton, typeof(Button))]
[TemplatePart(PART_EmailBox, typeof(TextBox))]
[TemplatePart(PART_UserMessageBox, typeof(TextBox))]
[TemplatePart(PART_ScreenshotToggle, typeof(CheckBox))]
[TemplatePart(PART_TabStrip, typeof(ListBox))]
[TemplatePart(PART_TabContent, typeof(ContentPresenter))]
[PseudoClasses(PC_Sending, PC_Success, PC_Failure, PC_HasScreenshot, PC_EmailRequired, PC_EmailInvalid)]
public partial class CrashReportDialog : ContentDialog
{
    private const string PART_SendButton = "PART_SendButton";
    private const string PART_SaveButton = "PART_SaveButton";
    private const string PART_CancelButton = "PART_CancelButton";
    private const string PART_EmailBox = "PART_EmailBox";
    private const string PART_UserMessageBox = "PART_UserMessageBox";
    private const string PART_ScreenshotToggle = "PART_ScreenshotToggle";
    private const string PART_TabStrip = "PART_TabStrip";
    private const string PART_TabContent = "PART_TabContent";

    private const string PC_Sending = ":sending";
    private const string PC_Success = ":success";
    private const string PC_Failure = ":failure";
    private const string PC_HasScreenshot = ":hasScreenshot";
    private const string PC_EmailRequired = ":emailRequired";
    private const string PC_EmailInvalid = ":emailInvalid";
    private const string PC_TabGeneral = ":tab-general";
    private const string PC_TabException = ":tab-exception";
    private const string PC_TabScreenshot = ":tab-screenshot";
    
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "ru-RU")]
    private static partial Regex MyRegex();

    private static readonly Regex EmailRegex = MyRegex();
    
    private Button? _sendButton;
    private Button? _saveButton;
    private Button? _cancelButton;
    private TextBox? _emailBox;
    private TextBox? _userMessageBox;
    private CheckBox? _screenshotToggle;
    private ListBox? _tabStrip;

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

    /// <summary>Defines the <see cref="AutoCloseOnSuccess"/> property.</summary>
    public static readonly StyledProperty<bool> AutoCloseOnSuccessProperty =
        AvaloniaProperty.Register<CrashReportDialog, bool>(nameof(AutoCloseOnSuccess), defaultValue: false);

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

    /// <summary>Gets or sets whether the dialog automatically closes after successful report sending.</summary>
    public bool AutoCloseOnSuccess
    {
        get => GetValue(AutoCloseOnSuccessProperty);
        set => SetValue(AutoCloseOnSuccessProperty, value);
    }
    
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CrashReportDialog);

    /// <summary>
    /// Raised when the user clicks Send Report.
    /// The handler must call <see cref="SendReportEventArgs.ReportSuccess"/> or
    /// <see cref="SendReportEventArgs.ReportFailure"/> to advance the dialog state.
    /// </summary>
    public event EventHandler<SendReportEventArgs>? SendReportRequested;

    /// <summary>Raised when the user clicks Save Report.</summary>
    public event EventHandler<SaveReportEventArgs>? SaveReportRequested;

    /// <summary>
    /// Creates a pre-populated <see cref="CrashReportDialog"/> from an <see cref="Exception"/>.
    /// </summary>
    public static CrashReportDialog FromException(
        Exception ex,
        string? applicationName = null,
        string? applicationVersion = null,
        Bitmap? screenshot = null)
    {
        StringBuilder sb = new();
        if (ex.InnerException is not null)
            sb.AppendLine(ex.InnerException.ToString());
        sb.Append(ex.StackTrace);

        return new CrashReportDialog
        {
            ApplicationName = applicationName,
            ApplicationVersion = applicationVersion,
            ExceptionType = ex.GetType().FullName,
            ExceptionMessage = ex.Message,
            ExceptionSource = ex.Source,
            StackTrace = sb.ToString(),
            OccurredAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            Screenshot = screenshot,
            ShowScreenshotTab = screenshot is not null
        };
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();

        _sendButton = e.NameScope.Find<Button>(PART_SendButton);
        _saveButton = e.NameScope.Find<Button>(PART_SaveButton);
        _cancelButton = e.NameScope.Find<Button>(PART_CancelButton);
        _emailBox = e.NameScope.Find<TextBox>(PART_EmailBox);
        _userMessageBox = e.NameScope.Find<TextBox>(PART_UserMessageBox);
        _screenshotToggle = e.NameScope.Find<CheckBox>(PART_ScreenshotToggle);
        _tabStrip = e.NameScope.Find<ListBox>(PART_TabStrip);

        AttachHandlers();
        UpdatePseudoClasses();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ScreenshotProperty)
        {
            PseudoClasses.Set(PC_HasScreenshot, change.NewValue is not null);
        }
        else if (change.Property == IsEmailRequiredProperty)
        {
            PseudoClasses.Set(PC_EmailRequired, change.GetNewValue<bool>());
        }
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _emailBox?.Focus();
    }

    private void AttachHandlers()
    {
        if (_sendButton is not null) _sendButton.Click += OnSendClicked;
        if (_saveButton is not null) _saveButton.Click += OnSaveClicked;
        if (_cancelButton is not null) _cancelButton.Click += OnCancelClicked;
        if (_tabStrip is not null) _tabStrip.SelectionChanged += OnTabSelectionChanged;
    }

    private void DetachHandlers()
    {
        if (_sendButton is not null) _sendButton.Click -= OnSendClicked;
        if (_saveButton is not null) _saveButton.Click -= OnSaveClicked;
        if (_cancelButton is not null) _cancelButton.Click -= OnCancelClicked;
        if (_tabStrip is not null) _tabStrip.SelectionChanged -= OnTabSelectionChanged;
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(PC_HasScreenshot, Screenshot is not null);
        PseudoClasses.Set(PC_EmailRequired, IsEmailRequired);
    }

    private void OnSendClicked(object? s, RoutedEventArgs e)
    {
        string email = _emailBox?.Text?.Trim() ?? string.Empty;

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

        string userMessage = _userMessageBox?.Text?.Trim() ?? string.Empty;
        bool includeScreenshot = _screenshotToggle?.IsChecked == true;

        // Transition to sending state.
        PseudoClasses.Set(PC_Sending, true);
        StatusMessage = Localizer.TrDefault("ToolKit/CrashReportDialog/SendingReport", "Sending report…");

        SetButtonsEnabled(false);

        SendReportEventArgs args = new(email, userMessage, includeScreenshot)
        {
            ReportSuccess = () => Dispatcher.UIThread.Post(async void () =>
            {
                PseudoClasses.Set(PC_Sending, false);
                PseudoClasses.Set(PC_Success, true);
                StatusMessage = Localizer.TrDefault("ToolKit/CrashReportDialog/SuccessMessage", "Report sent successfully. Thank you!");
                SetButtonsEnabled(true);

                if (AutoCloseOnSuccess)
                {
                    await Task.Delay(1500); // Wait briefly to show success message
                    await CloseAsync(CrashReportResult.Sent);
                }
            }),
            ReportFailure = msg => Dispatcher.UIThread.Post(() =>
            {
                PseudoClasses.Set(PC_Sending, false);
                PseudoClasses.Set(PC_Failure, true);
                StatusMessage = $"{Localizer.TrDefault("ToolKit/CrashReportDialog/FailureMessagePrefix", "Failed to send report: ")}{msg}";
                SetButtonsEnabled(true);
                _ = CloseAsync(CrashReportResult.Cancelled);
            })
        };

        SendReportRequested?.Invoke(this, args);
    }

    private void OnSaveClicked(object? s, RoutedEventArgs e)
    {
        string userMessage = _userMessageBox?.Text?.Trim() ?? string.Empty;
        SaveReportRequested?.Invoke(this, new SaveReportEventArgs(userMessage));
        
        _ = CloseAsync(CrashReportResult.Saved);
    }

    private void OnCancelClicked(object? s, RoutedEventArgs e)
    {
        _ = CloseAsync(CrashReportResult.Cancelled);
    }

    private void OnTabSelectionChanged(object? s, SelectionChangedEventArgs e)
    {
        // Drive tab visibility via the Tag property so AXAML styles can react.

        if (_tabStrip?.SelectedItem is not ListBoxItem selected)
            return;
        
        PseudoClasses.Set(PC_TabGeneral, ReferenceEquals(selected.Tag, "general"));
        PseudoClasses.Set(PC_TabException, ReferenceEquals(selected.Tag, "exception"));
        PseudoClasses.Set(PC_TabScreenshot, ReferenceEquals(selected.Tag, "screenshot"));
    }

    private void SetButtonsEnabled(bool enabled)
    {
        _sendButton?.IsEnabled = enabled;
        _saveButton?.IsEnabled = enabled;
        _cancelButton?.IsEnabled = enabled;
    }
}