using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using PleasantUI.Controls;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// Severity level for the notice dialog.
/// </summary>
public enum NoticeSeverity
{
    /// <summary>Informational notice.</summary>
    Info,
    /// <summary>Warning notice.</summary>
    Warning,
    /// <summary>Error notice.</summary>
    Error,
    /// <summary>Success notice.</summary>
    Success
}

/// <summary>
/// A modal dialog for displaying notices, warnings, or work-in-progress messages.
/// Features a severity-based header with icon, message body, optional footer text,
/// and customizable action buttons. All text properties support localization keys.
/// </summary>
[PseudoClasses(":info", ":warning", ":error", ":success")]
[TemplatePart("PART_PrimaryButton", typeof(Button))]
[TemplatePart("PART_SecondaryButton", typeof(Button))]
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

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the primary button is clicked.</summary>
    public event EventHandler? PrimaryButtonClicked;

    /// <summary>Raised when the secondary button is clicked.</summary>
    public event EventHandler? SecondaryButtonClicked;
    
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NoticeDialog);

    public NoticeDialog() => InitializeComponent();

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
    public static Task Show(
        Core.Interfaces.IPleasantWindow parent,
        string title,
        string message,
        string? noticeFooterText = null,
        string? primaryButtonText = null,
        string? secondaryButtonText = null,
        NoticeSeverity severity = NoticeSeverity.Info)
    {
        NoticeDialog dialog = new()
        {
            Title = title,
            Message = message,
            NoticeFooterText = noticeFooterText,
            PrimaryButtonText = primaryButtonText,
            SecondaryButtonText = secondaryButtonText,
            Severity = severity
        };

        TaskCompletionSource tcs = new();
        dialog.Closed += (_, _) => tcs.TrySetResult();
        dialog.ShowAsync(parent);
        
        return tcs.Task;
    }
}
