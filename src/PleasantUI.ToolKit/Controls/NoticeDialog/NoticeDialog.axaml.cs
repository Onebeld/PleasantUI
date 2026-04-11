using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using PleasantUI.Core;
using PleasantUI.Core.Localization;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// A modal dialog for displaying notices, warnings, or work-in-progress messages.
/// Features a severity-based header with icon, message body, optional footer text,
/// and customizable action buttons. All text properties support localization keys.
/// </summary>
public partial class NoticeDialog
{
    private const double ProgressBarVisibleHeight = 20.0;

    // ── Severity icon geometries ───────────────────────────────────────────────

    private static readonly Geometry InfoIconGeometry = Geometry.Parse("M11 2a9 9 0 1 0 0 18 9 9 0 0 0 0-18zm0 16a7 7 0 1 1 0-14 7 7 0 0 1 0 14zm1-11h-2v2h2V7zm0 4h-2v4h2v-4z");
    private static readonly Geometry WarningIconGeometry = Geometry.Parse("M12 2L1 21h22L12 2zm0 3.99L19.53 19H4.47L12 5.99zM11 10v4h2v-4h-2zm0 6v2h2v-2h-2z");
    private static readonly Geometry ErrorIconGeometry = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zM8.5 8.5l7 7M15.5 8.5l-7 7");
    private static readonly Geometry SuccessIconGeometry = Geometry.Parse("M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z");
    private static readonly Geometry WipIconGeometry = Geometry.Parse("M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z");

    // ── Severity brushes ───────────────────────────────────────────────────────

    private static IBrush GetSeverityHeaderBrush(NoticeSeverity severity)
    {
        // Use the solid SystemFillColor tokens (which exist in all themes) for the accent stripe.
        // These are opaque colors suitable for a full-bleed header background.
        return severity switch
        {
            NoticeSeverity.Info => Application.Current!.TryFindResource("SystemFillColorNeutral", out object? ib) && ib is Color cib
                ? new SolidColorBrush(cib) : new SolidColorBrush(Color.Parse("#4B6ACA")),
            NoticeSeverity.Warning => Application.Current!.TryFindResource("SystemFillColorCaution", out object? wb) && wb is Color cwb
                ? new SolidColorBrush(cwb) : new SolidColorBrush(Color.Parse("#BF8500")),
            NoticeSeverity.Error => Application.Current!.TryFindResource("SystemFillColorCritical", out object? eb) && eb is Color ceb
                ? new SolidColorBrush(ceb) : new SolidColorBrush(Color.Parse("#C42B1C")),
            NoticeSeverity.Success => Application.Current!.TryFindResource("SystemFillColorSuccess", out object? sb) && sb is Color csb
                ? new SolidColorBrush(csb) : new SolidColorBrush(Color.Parse("#0F7B0F")),
            NoticeSeverity.WorkInProgress => Application.Current!.TryFindResource("SystemFillColorCaution", out object? wib) && wib is Color cwib
                ? new SolidColorBrush(cwib) : new SolidColorBrush(Color.Parse("#BF8500")),
            _ => Brushes.Transparent
        };
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UpdateSeverity();
        UpdateLocalization();
        UpdateVersionBadge();
        UpdateButtons();
        
        // Ensure version badge background is set after template is applied as failsafe
        if (!string.IsNullOrEmpty(VersionType))
        {
            var versionTypeBadge = this.FindControl<Border>("VersionTypeBadge");
            if (versionTypeBadge is not null)
            {
                var brush = VersionTypeEnum.HasValue
                    ? GetVersionTypeBrushFromEnum(VersionTypeEnum.Value)
                    : GetVersionTypeBrushFromString(VersionType);
                VersionTypeBadgeBackground = brush;
                versionTypeBadge.Background = brush;
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SeverityProperty)
            UpdateSeverity();
        else if (change.Property == TitleProperty || change.Property == MessageProperty || change.Property == NoticeFooterTextProperty)
            UpdateLocalization();
        else if (change.Property == VersionProperty || change.Property == VersionTypeProperty || change.Property == VersionLabelProperty || change.Property == VersionTypeEnumProperty)
            UpdateVersionBadge();
        else if (change.Property == VersionTypeBadgeBackgroundProperty)
        {
            var versionTypeBadge = this.FindControl<Border>("VersionTypeBadge");
            if (versionTypeBadge is not null && VersionTypeBadgeBackground is not null)
                versionTypeBadge.Background = VersionTypeBadgeBackground;
        }
        else if (change.Property == PrimaryButtonTextProperty || change.Property == SecondaryButtonTextProperty)
            UpdateButtons();
    }

    // ── Update methods ─────────────────────────────────────────────────────────

    private void UpdateSeverity()
    {
        var headerBorder = this.FindControl<Border>("HeaderBorder");
        var severityIcon = this.FindControl<PathIcon>("SeverityIcon");
        var titleText = this.FindControl<TextBlock>("TitleText");

        if (headerBorder is null || severityIcon is null || titleText is null)
            return;

        var severity = Severity;
        headerBorder.Background = GetSeverityHeaderBrush(severity);

        severityIcon.Data = severity switch
        {
            NoticeSeverity.Info => InfoIconGeometry,
            NoticeSeverity.Warning => WarningIconGeometry,
            NoticeSeverity.Error => ErrorIconGeometry,
            NoticeSeverity.Success => SuccessIconGeometry,
            NoticeSeverity.WorkInProgress => WipIconGeometry,
            _ => InfoIconGeometry
        };
    }

    private void UpdateLocalization()
    {
        var titleText = this.FindControl<TextBlock>("TitleText");
        var messageText = this.FindControl<TextBlock>("MessageText");
        var footerText = this.FindControl<TextBlock>("FooterText");

        if (titleText is not null && Title is not null)
            titleText.Text = Localizer.Instance.TryGetString(Title, out string rt) ? rt : Title;

        if (messageText is not null && Message is not null)
            messageText.Text = Localizer.Instance.TryGetString(Message, out string rm) ? rm : Message;

        if (footerText is not null)
        {
            if (!string.IsNullOrEmpty(NoticeFooterText))
            {
                footerText.Text = Localizer.Instance.TryGetString(NoticeFooterText, out string rf) ? rf : NoticeFooterText;
                footerText.IsVisible = true;
            }
            else
            {
                footerText.IsVisible = false;
            }
        }
    }

    private void UpdateVersionBadge()
    {
        var versionPanel = this.FindControl<Border>("VersionPanel");
        var versionLabelText = this.FindControl<TextBlock>("VersionLabelText");
        var versionText = this.FindControl<TextBlock>("VersionText");
        var versionTypeBadge = this.FindControl<Border>("VersionTypeBadge");
        var versionTypeText = this.FindControl<TextBlock>("VersionTypeText");

        if (versionPanel is null || versionLabelText is null || versionText is null || versionTypeBadge is null || versionTypeText is null)
            return;

        if (!string.IsNullOrEmpty(Version))
        {
            versionPanel.IsVisible = true;
            versionText.Text = Version;

            if (!string.IsNullOrEmpty(VersionLabel))
                versionLabelText.Text = Localizer.Instance.TryGetString(VersionLabel, out string vl) ? vl : VersionLabel;
            else
                versionLabelText.Text = string.Empty;

            if (!string.IsNullOrEmpty(VersionType))
            {
                versionTypeText.Text = VersionType;
                // Prefer strongly-typed enum for accurate color resolution; fall back to string normalization
                var brush = VersionTypeEnum.HasValue
                    ? GetVersionTypeBrushFromEnum(VersionTypeEnum.Value)
                    : GetVersionTypeBrushFromString(VersionType);
                VersionTypeBadgeBackground = brush;
                versionTypeBadge.Background = brush;
                versionTypeBadge.IsVisible = true;
            }
            else
            {
                versionTypeBadge.IsVisible = false;
            }
        }
        else
        {
            versionPanel.IsVisible = false;
        }
    }

    private static IBrush GetVersionTypeBrushFromEnum(PleasantVersionType versionType)
    {
        return versionType switch
        {
            PleasantVersionType.Stable => new SolidColorBrush(Color.Parse("#107C10")),
            PleasantVersionType.BugFix => new SolidColorBrush(Color.Parse("#008272")),
            PleasantVersionType.Alpha => new SolidColorBrush(Color.Parse("#E81123")),
            PleasantVersionType.Beta => new SolidColorBrush(Color.Parse("#FF8C00")),
            PleasantVersionType.ReleaseCandidate => new SolidColorBrush(Color.Parse("#8A2BE2")),
            PleasantVersionType.Canary => new SolidColorBrush(Color.Parse("#FFB900")),
            _ => new SolidColorBrush(Color.Parse("#605E5C"))
        };
    }

    private static IBrush GetVersionTypeBrushFromString(string versionType)
    {
        // Normalize: lowercase, strip spaces and hyphens
        var normalized = versionType.ToLowerInvariant().Replace(" ", "").Replace("-", "");

        return normalized switch
        {
            "stable" or "stablerelease" => new SolidColorBrush(Color.Parse("#107C10")),
            "bugfix" or "fix" or "bugfixrelease" => new SolidColorBrush(Color.Parse("#008272")),
            "alpha" or "alphaprerelease" or "alphaprerelease" => new SolidColorBrush(Color.Parse("#E81123")),
            "beta" or "betaprerelease" => new SolidColorBrush(Color.Parse("#FF8C00")),
            "rc" or "releasecandidate" => new SolidColorBrush(Color.Parse("#8A2BE2")),
            "canary" or "canarybuild" => new SolidColorBrush(Color.Parse("#FFB900")),
            _ => new SolidColorBrush(Color.Parse("#605E5C"))
        };
    }

    private void UpdateButtons()
    {
        var buttonsHost = this.FindControl<UniformGrid>("ButtonsHost");
        if (buttonsHost is null)
            return;

        buttonsHost.Children.Clear();

        bool hasPrimary = !string.IsNullOrEmpty(PrimaryButtonText);
        bool hasSecondary = !string.IsNullOrEmpty(SecondaryButtonText);

        int buttonCount = (hasPrimary ? 1 : 0) + (hasSecondary ? 1 : 0);
        buttonsHost.Columns = buttonCount == 0 ? 1 : buttonCount;

        if (hasSecondary)
        {
            var secondaryButton = new Button
            {
                Content = Localizer.Instance.TryGetString(SecondaryButtonText!, out string rs) ? rs : SecondaryButtonText!,
                Margin = Thickness.Parse("5"),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            secondaryButton.Click += (_, _) => SecondaryButtonClicked?.Invoke(this, EventArgs.Empty);
            buttonsHost.Children.Add(secondaryButton);
        }

        if (hasPrimary)
        {
            var primaryButton = new Button
            {
                Content = Localizer.Instance.TryGetString(PrimaryButtonText!, out string rp) ? rp : PrimaryButtonText!,
                Theme = Severity switch
                {
                    NoticeSeverity.Error when Application.Current!.TryFindResource("DangerButtonTheme", out object? dt) && dt is ControlTheme dct => dct,
                    _ when Application.Current!.TryFindResource("AccentButtonTheme", out object? at) && at is ControlTheme act => act,
                    _ => null
                },
                Margin = Thickness.Parse("5"),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            primaryButton.Click += (_, _) => PrimaryButtonClicked?.Invoke(this, EventArgs.Empty);
            buttonsHost.Children.Add(primaryButton);
        }

        if (buttonCount == 0)
        {
            buttonsHost.Children.Add(new Panel());
        }
    }
}
