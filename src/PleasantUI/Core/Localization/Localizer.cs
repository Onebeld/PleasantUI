using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace PleasantUI.Core.Localization;

/// <summary>
/// Provides localization functionality for the application.
/// </summary>
public class Localizer : ILocalizer, INotifyPropertyChanged
{
    private const string DefaultLanguage = "en";

    private const string IndexerName = "Item";
    private const string IndexerArrayName = "Item[]";

    private static readonly List<ResourceManager>? ResourceManagers = new();

    private List<ResourceManager>? _resources;

    /// <summary>
    /// Gets the singleton instance of the <see cref="Localizer" /> class.
    /// </summary>
    public static Localizer Instance { get; } = new();

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the localization changes.
    /// </summary>
    public event Action<string>? LocalizationChanged;

    /// <summary>
    /// Gets the localized string for the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The localized string, or an error message if the key is not found or the resources are empty.</returns>
    public string this[string key]
    {
        get
        {
            if (_resources == null || !_resources.Any())
                return "<ERROR! LANGUAGE Resources is empty>";

            string? row = GetExpression(key);

            if (string.IsNullOrEmpty(row))
                return $"<ERROR! Not found key \"{key}\">";

            string? ret = row?.Replace(@"\\n", "\n");

            if (string.IsNullOrEmpty(ret))
                ret = $"Localize:{key}";

            return ret!;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Localizer" /> class.
    /// </summary>
    public Localizer()
    {
        LoadLanguage();
    }

    /// <summary>
    /// Translates the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="context">The context of the translation.</param>
    /// <param name="args">The arguments to pass to the translation.</param>
    /// <returns>The translated string.</returns>
    public static string Tr(string? key, string? context = null, params object[] args)
    {
        if (key is null)
            return string.Empty;

        if (context is not null)
            key = $"{context}/{key}";

        string expression = Instance[key];

        return string.Format(expression, args);
    }

    /// <summary>
    /// Adds a resource manager to the current localization instance.
    /// </summary>
    /// <param name="resourceManager">The <see cref="ResourceManager"/> to be added.</param>
    public static void AddRes(ResourceManager resourceManager) => Instance.AddResourceManager(resourceManager);

    /// <summary>
    /// Changes the current language of the application.
    /// </summary>
    /// <param name="language">The language code to switch to (e.g., "en", "fr").</param>
    public static void ChangeLang(string language) => Instance.ChangeLanguage(language);


    /// <summary>
    /// Attempts to get the localized string for the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="expression">The localized string, or null if the key is not found or the resources are empty.</param>
    /// <returns>True if the key is found, false otherwise.</returns>
    public bool TryGetString(string key, out string expression)
    {
        if (_resources == null || !_resources.Any())
        {
            expression = "<ERROR! LANGUAGE Resources is empty>";
            return false;
        }

        string? row = GetExpression(key);

        if (string.IsNullOrEmpty(row))
        {
            expression = $"<ERROR! Not found key \"{key}\">";
            return false;
        }

        string? ret = row?.Replace(@"\\n", "\n");

        if (string.IsNullOrEmpty(ret))
            ret = $"Localize:{key}";

        expression = ret!;
        return true;
    }

    /// <inheritdoc />
    public void EditLanguage(string language)
    {
        Instance.ChangeLanguage(language);
    }

    /// <inheritdoc />
    public void AddResourceManager(ResourceManager resourceManager)
    {
        ResourceManagers?.Add(resourceManager);
    }

    /// <inheritdoc />
    public void ChangeLanguage(string language = "en")
    {
        if (string.IsNullOrEmpty(language))
            language = DefaultLanguage;

        CultureInfo.CurrentUICulture = new CultureInfo(language);
        Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture;
        LoadLanguage();

        LocalizationChanged?.Invoke(language);
    }

    /// <inheritdoc />
    public string? GetExpression(string key)
    {
        if (_resources == null) return string.Empty;
        foreach (ResourceManager? resource in _resources)
        {
            string? row;

            try
            {
                row = resource?.GetString(key);
            }
            catch (MissingManifestResourceException)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(row))
                return row;
        }

        return string.Empty;
    }

    /// <summary>
    /// Returns an observable for the specified localization key.
    /// </summary>
    /// <param name="key">The key for which to get the observable.</param>
    /// <returns>An observable that emits localization changes for the specified key.</returns>
    public static IObservable<string> GetObservable(string key)
    {
        return new LocalizeObservable(key);
    }

    private void LoadLanguage()
    {
        if (ResourceManagers != null)
            _resources = new List<ResourceManager>(ResourceManagers);

        InvalidateEvents();
    }

    private void InvalidateEvents()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
    }
}