using System.ComponentModel;
using System.Diagnostics;
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

    // Strong references to all LocalizeKeyObservable instances — prevents GC from
    // collecting them and silently killing their LocalizationChanged subscriptions.
    private static readonly List<object> AliveObservables = [];
    private static readonly object ObservableLock = new();

    private List<ResourceManager>? _resources;
    private int _isChangingLanguage;

    /// <summary>
    /// Gets the singleton instance of the <see cref="Localizer" /> class.
    /// </summary>
    public static Localizer Instance { get; } = new();

    /// <summary>
    /// Gets the current language code (e.g. "en", "ru").
    /// Updated by <see cref="ChangeLanguage"/> and used as a reactive trigger
    /// in dynamic-key localize bindings.
    /// </summary>
    public string CurrentLanguage { get; private set; } = DefaultLanguage;

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

    public static string TrDefault(string? key, string? defaultString = null, string? context = null, params object[] args)
    {
        if (key is null)
            return defaultString ?? string.Empty;

        if (context is not null)
            key = $"{context}/{key}";
            
        if (!Instance.TryGetString(key, out string text))
            text = defaultString ?? string.Empty;

        return string.Format(text, args);
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
    /// Resets the localizer to a clean state. Intended for app "soft restarts"
    /// (hot reload / restart without full process shutdown) where static singletons
    /// and event subscriptions may survive.
    /// </summary>
    public static void Reset() => Instance.ResetInternal();


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
        if (ResourceManagers == null) return;

        // Deduplicate by base name to prevent accumulation on hot reload / multiple calls
        bool alreadyAdded = ResourceManagers.Any(r =>
            string.Equals(r.BaseName, resourceManager.BaseName, StringComparison.Ordinal));

        if (alreadyAdded)
            return;

        ResourceManagers.Add(resourceManager);

        // Make AddRes safe to call at any time: update active resource list immediately.
        // Without this, views/VMs created before the first ChangeLanguage() can resolve
        // keys against an empty _resources list and never re-evaluate.
        LoadLanguage();

        // Prime the newly-added manager for the current culture so the first GetString
        // after registration doesn't race satellite assembly loading.
        try
        {
            var culture = CultureInfo.CurrentUICulture;
            resourceManager.GetResourceSet(culture, createIfNotExists: true, tryParents: true);
        }
        catch
        {
            // ignore
        }

        // If we're not currently in ChangeLanguage(), force a refresh so bindings
        // already on-screen re-evaluate with the newly available resources.
        if (Interlocked.CompareExchange(ref _isChangingLanguage, 0, 0) == 0)
            LocalizationChanged?.Invoke(CurrentLanguage);
    }

    /// <summary>
    /// Keeps a strong reference to a <see cref="LocalizeKeyObservable"/> so it is never
    /// garbage-collected while this singleton is alive.
    /// </summary>
    internal static void RegisterObservable(object observable)
    {
        lock (ObservableLock)
            AliveObservables.Add(observable);
    }

    private void ResetInternal()
    {
        // Prevent re-entrancy / inconsistent state.
        Interlocked.Exchange(ref _isChangingLanguage, 1);

        try
        {
            // Clear resource managers & resolved list.
            ResourceManagers?.Clear();
            _resources = null;

            // Drop strong references to observables so old bindings can be collected.
            lock (ObservableLock)
                AliveObservables.Clear();

            // Drop event subscribers that might reference old visual trees.
            LocalizationChanged = null;
            PropertyChanged = null;

            CurrentLanguage = DefaultLanguage;

            // Force any remaining bindings that still reference this instance to re-evaluate.
            InvalidateEvents();
        }
        finally
        {
            Interlocked.Exchange(ref _isChangingLanguage, 0);
        }
    }

    /// <inheritdoc />
    public void ChangeLanguage(string language = "en")
    {
        if (string.IsNullOrEmpty(language))
            language = DefaultLanguage;

        Interlocked.Exchange(ref _isChangingLanguage, 1);
        Debug.WriteLine($"[Localizer] ChangeLanguage → \"{language}\" (subscribers: {LocalizationChanged?.GetInvocationList().Length ?? 0}, observables: {AliveObservables.Count})");

        CultureInfo culture = new(language);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        CurrentLanguage = language;
        LoadLanguage();

        // Force-prime every ResourceManager with the new culture so satellite assemblies
        // are loaded NOW — before we fire LocalizationChanged. Without this, the first
        // GetString call happens inside a PropertyChanged handler, which triggers lazy
        // satellite DLL loading AFTER the handler returns, so the UI reads stale values.
        // GetResourceSet with createIfNotExists=true forces the satellite DLL to load immediately.
        if (ResourceManagers != null)
        {
            foreach (ResourceManager rm in ResourceManagers)
            {
                try { rm.GetResourceSet(culture, createIfNotExists: true, tryParents: true); } catch { /* ignore */ }
            }
        }

        Debug.WriteLine($"[Localizer] Resources primed for \"{language}\", firing LocalizationChanged");

        LocalizationChanged?.Invoke(language);

        Debug.WriteLine($"[Localizer] ChangeLanguage done → \"{language}\"");
        Interlocked.Exchange(ref _isChangingLanguage, 0);
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