using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Localization;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// Severity level for the notice dialog.
/// </summary>
public enum NoticeSeverity
{
    /// <summary>Informational notice (blue).</summary>
    Info,
    /// <summary>Warning notice (yellow/orange).</summary>
    Warning,
    /// <summary>Error notice (red).</summary>
    Error,
    /// <summary>Success notice (green).</summary>
    Success,
    /// <summary>Work in progress notice (yellow).</summary>
    WorkInProgress
}

/// <summary>
/// A modal dialog for displaying notices, warnings, or work-in-progress messages.
/// Features a severity-based header with icon, message body, optional footer text,
/// and customizable action buttons. All text properties support localization keys.
/// </summary>
public partial class NoticeDialog : ContentDialog
{
    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(Title));

    /// <summary>Defines the <see cref="Message"/> property.</summary>
    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(Message));

    /// <summary>Defines the <see cref="NoticeFooterText"/> property.</summary>
    public static readonly StyledProperty<string?> NoticeFooterTextProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(NoticeFooterText));

    /// <summary>Defines the <see cref="PrimaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(PrimaryButtonText));

    /// <summary>Defines the <see cref="SecondaryButtonText"/> property.</summary>
    public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(SecondaryButtonText));

    /// <summary>Defines the <see cref="Severity"/> property.</summary>
    public static readonly StyledProperty<NoticeSeverity> SeverityProperty =
        AvaloniaProperty.Register<NoticeDialog, NoticeSeverity>(nameof(Severity), defaultValue: NoticeSeverity.Info);

    /// <summary>Defines the <see cref="Version"/> property.</summary>
    public static readonly StyledProperty<string?> VersionProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(Version));

    /// <summary>Defines the <see cref="VersionType"/> property.</summary>
    public static readonly StyledProperty<string?> VersionTypeProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(VersionType));

    /// <summary>Defines the <see cref="VersionLabel"/> property.</summary>
    public static readonly StyledProperty<string?> VersionLabelProperty =
        AvaloniaProperty.Register<NoticeDialog, string?>(nameof(VersionLabel));

    /// <summary>Defines the <see cref="VersionTypeBadgeBackground"/> property.</summary>
    public static readonly StyledProperty<IBrush?> VersionTypeBadgeBackgroundProperty =
        AvaloniaProperty.Register<NoticeDialog, IBrush?>(nameof(VersionTypeBadgeBackground));

    /// <summary>Defines the <see cref="VersionTypeEnum"/> property.</summary>
    public static readonly StyledProperty<PleasantVersionType?> VersionTypeEnumProperty =
        AvaloniaProperty.Register<NoticeDialog, PleasantVersionType?>(nameof(VersionTypeEnum));

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the dialog title. Supports localization keys.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the main message text. Supports localization keys.</summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Gets or sets optional footer text (e.g., attribution or additional info). Supports localization keys.</summary>
    public string? NoticeFooterText
    {
        get => GetValue(NoticeFooterTextProperty);
        set => SetValue(NoticeFooterTextProperty, value);
    }

    /// <summary>Gets or sets the text of the primary action button. Null hides the button. Supports localization keys.</summary>
    public string? PrimaryButtonText
    {
        get => GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Gets or sets the text of the secondary action button. Null hides the button. Supports localization keys.</summary>
    public string? SecondaryButtonText
    {
        get => GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Gets or sets the severity level, which affects the icon and header color.</summary>
    public NoticeSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>Gets or sets the version string to display (e.g., "1.0.0").</summary>
    public string? Version
    {
        get => GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    /// <summary>Gets or sets the version type description (e.g., "Beta", "Alpha").</summary>
    public string? VersionType
    {
        get => GetValue(VersionTypeProperty);
        set => SetValue(VersionTypeProperty, value);
    }

    /// <summary>Gets or sets the label text for the version field (e.g., "Version"). Supports localization keys.</summary>
    public string? VersionLabel
    {
        get => GetValue(VersionLabelProperty);
        set => SetValue(VersionLabelProperty, value);
    }

    /// <summary>Gets or sets the background brush for the version type badge.</summary>
    public IBrush? VersionTypeBadgeBackground
    {
        get => GetValue(VersionTypeBadgeBackgroundProperty);
        set => SetValue(VersionTypeBadgeBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the strongly-typed version type enum. When set, badge color is resolved
    /// from the enum directly (same logic as AboutView), bypassing string normalization.
    /// </summary>
    public PleasantVersionType? VersionTypeEnum
    {
        get => GetValue(VersionTypeEnumProperty);
        set => SetValue(VersionTypeEnumProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the primary button is clicked.</summary>
    public event EventHandler? PrimaryButtonClicked;

    /// <summary>Raised when the secondary button is clicked.</summary>
    public event EventHandler? SecondaryButtonClicked;

    // ── Constructor ───────────────────────────────────────────────────────────

    public NoticeDialog() => InitializeComponent();

    // ── Static factory ────────────────────────────────────────────────────────

    /// <summary>
    /// Shows a <see cref="NoticeDialog"/> with the specified parameters.
    /// </summary>
    /// <param name="parent">The parent window that hosts the dialog.</param>
    /// <param name="title">The dialog title text or localization key.</param>
    /// <param name="message">The main message text or localization key.</param>
    /// <param name="noticeFooterText">Optional footer text or localization key.</param>
    /// <param name="primaryButtonText">Primary button text or localization key. Null hides the button.</param>
    /// <param name="secondaryButtonText">Secondary button text or localization key. Null hides the button.</param>
    /// <param name="severity">Severity level affecting icon and header color.</param>
    /// <param name="version">Optional version string to display.</param>
    /// <param name="versionType">Optional version type description (e.g., "Beta", "Alpha").</param>
    /// <param name="versionLabel">Optional label for version field (e.g., "Version"). Supports localization keys.</param>
    public static Task Show(
        PleasantUI.Core.Interfaces.IPleasantWindow parent,
        string title,
        string message,
        string? noticeFooterText = null,
        string? primaryButtonText = null,
        string? secondaryButtonText = null,
        NoticeSeverity severity = NoticeSeverity.Info,
        string? version = null,
        string? versionType = null,
        string? versionLabel = null)
    {
        var dialog = new NoticeDialog
        {
            Title = title,
            Message = message,
            NoticeFooterText = noticeFooterText,
            PrimaryButtonText = primaryButtonText,
            SecondaryButtonText = secondaryButtonText,
            Severity = severity,
            Version = version,
            VersionType = versionType,
            VersionLabel = versionLabel
        };

        var tcs = new TaskCompletionSource();
        dialog.Closed += (_, _) => tcs.TrySetResult();
        dialog.ShowAsync(parent);
        return tcs.Task;
    }
}
