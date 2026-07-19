using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// A modal dialog for displaying notices, warnings, or work-in-progress messages.
/// Features a severity-based header with icon, message body, optional footer text,
/// and customizable action buttons. All text properties support localization keys.
/// </summary>
public partial class NoticeDialog
{

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        Button? primaryButton = e.NameScope.Find<Button>("PART_PrimaryButton");
        Button? secondaryButton = e.NameScope.Find<Button>("PART_SecondaryButton");
        
        primaryButton?.Click += (_, _) => PrimaryButtonClicked?.Invoke(this, EventArgs.Empty);
        secondaryButton?.Click += (_, _) => SecondaryButtonClicked?.Invoke(this, EventArgs.Empty);
        
        UpdateSeverity();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SeverityProperty)
            UpdateSeverity();
    }

    // ── Update methods ─────────────────────────────────────────────────────────

    private void UpdateSeverity()
    {
        PseudoClasses.Set(":info", Severity is NoticeSeverity.Info);
        PseudoClasses.Set(":warning", Severity is NoticeSeverity.Warning);
        PseudoClasses.Set(":error", Severity is NoticeSeverity.Error);
        PseudoClasses.Set(":success", Severity is NoticeSeverity.Success);
    }
}
