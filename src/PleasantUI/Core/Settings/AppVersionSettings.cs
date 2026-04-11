using System.Runtime.Serialization;

namespace PleasantUI.Core.Settings;

/// <summary>
/// Stores application-specific version information inside <see cref="PleasantSettings"/>.
/// Reuses <see cref="PleasantVersionType"/> for release-channel semantics while keeping
/// app versioning fully separate from the PleasantUI library version.
/// </summary>
public class AppVersionSettings : ViewModelBase
{
    private string _version = string.Empty;
    private PleasantVersionType _versionType = PleasantVersionType.Stable;
    private string _versionLabel = "Version";

    /// <summary>
    /// Gets or sets the application version string (e.g., "1.0.0", "2.3.1-beta").
    /// </summary>
    [DataMember]
    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    /// <summary>
    /// Gets or sets the release channel / version type for the application.
    /// Reuses <see cref="PleasantVersionType"/> — Stable, BugFix, Alpha, Beta, RC, Canary.
    /// </summary>
    [DataMember]
    public PleasantVersionType VersionType
    {
        get => _versionType;
        set
        {
            if (SetProperty(ref _versionType, value))
                RaisePropertyChanged(nameof(VersionTypeDescription));
        }
    }

    /// <summary>
    /// Gets or sets a custom label shown next to the version string (e.g., "Version", "App Version").
    /// Defaults to "Version".
    /// </summary>
    [DataMember]
    public string VersionLabel
    {
        get => _versionLabel;
        set => SetProperty(ref _versionLabel, value);
    }

    /// <summary>
    /// Gets a display-friendly description of <see cref="VersionType"/>.
    /// Derived — not serialized.
    /// </summary>
    public string VersionTypeDescription => VersionType switch
    {
        PleasantVersionType.Stable           => "Stable Release",
        PleasantVersionType.BugFix           => "Bug Fix Release",
        PleasantVersionType.Alpha            => "Alpha Pre-Release",
        PleasantVersionType.Beta             => "Beta Pre-Release",
        PleasantVersionType.ReleaseCandidate => "Release Candidate",
        PleasantVersionType.Canary           => "Canary Build",
        _                                    => "Unknown"
    };
}
