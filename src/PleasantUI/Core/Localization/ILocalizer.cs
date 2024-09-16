using System.Resources;

namespace PleasantUI.Core.Localization;

/// <summary>
/// Interface describing functionality for localizer implementation.
/// </summary>
public interface ILocalizer
{
    /// <summary>
    /// Changes the current language.
    /// </summary>
    /// <param name="language">The language code to set.</param>
    void ChangeLanguage(string language);

    /// <summary>
    /// Gets the localized string for the specified key from the available resource managers.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The localized string, or an empty string if the key is not found.</returns>
    string? GetExpression(string key);

    /// <summary>
    /// Edits the current language.
    /// </summary>
    /// <param name="language">The language code to set.</param>
    void EditLanguage(string language);

    /// <summary>
    /// Adds a <see cref="ResourceManager" /> to the list of resource managers used for localization.
    /// </summary>
    /// <param name="resourceManager">The <see cref="ResourceManager" /> to add.</param>
    void AddResourceManager(ResourceManager resourceManager);
}