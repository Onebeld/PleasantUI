using System.Resources;
using Avalonia;
using Avalonia.Controls;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Localization;
using PleasantUI.Example.Structures;
using PleasantUI.Example.ViewModels;
using PleasantUI.ToolKit.Services;

namespace PleasantUI.Example;

public class PleasantUiExampleApp : Application
{
    public static PleasantTheme PleasantTheme { get; protected set; } = null!;

    public static IPleasantWindow Main { get; protected set; } = null!;

    public static AppViewModel ViewModel { get; private set; } = null!;

    public static TopLevel? TopLevel { get; protected set; }

    public PleasantUiExampleApp()
    {
        // Soft restarts can keep static singletons alive. Start from a clean localization state
        // so we can't end up with mixed old/new subscribers and stale resources.
        Localizer.Reset();

        Localizer.AddRes(new ResourceManager(typeof(Properties.Localizations.App)));
        Localizer.AddRes(new ResourceManager(typeof(Properties.Localizations.Library)));

        // NOTE: PleasantSettings.Current is NOT yet set here — PleasantTheme (which loads
        // settings from disk) is initialized later during Initialize() → AvaloniaXamlLoader.Load().
        // Language loading from persisted settings is deferred to InitializeFromSettings(),
        // which is called from OnFrameworkInitializationCompleted() after PleasantTheme is ready.
        Localizer.ChangeLang(LanguageKey);
    }

    /// <summary>
    /// Called from OnFrameworkInitializationCompleted() after PleasantTheme has been
    /// initialized and PleasantSettings.Current has been loaded from disk.
    /// Applies the persisted language and constructs the AppViewModel.
    /// </summary>
    protected void InitializeFromSettings()
    {
        if (Design.IsDesignMode) return;

        // Now PleasantSettings.Current is available — apply persisted language.
        if (PleasantSettings.Current is not null && !string.IsNullOrEmpty(PleasantSettings.Current.Language))
        {
            LanguageKey = PleasantSettings.Current.Language;
        }

        // Re-apply language now that we know the persisted value.
        // This fires LocalizationChanged so all already-subscribed bindings update.
        Localizer.ChangeLang(LanguageKey);

        // Construct VM after localization is fully initialized with the correct language.
        ViewModel = new AppViewModel(new EventAggregator());
        DataContext = ViewModel;
    }

    public static string LanguageKey { get; set; } = "en";

    public static NavigationViewPosition NavPosition { get; set; } = NavigationViewPosition.Left;

    public static event Action<NavigationViewPosition>? NavPositionChanged;

    public static void SetNavPosition(NavigationViewPosition position)
    {
        NavPosition = position;
        NavPositionChanged?.Invoke(position);
    }

    public static readonly Language[] Languages =
    [
        new("English (English)", "en"),
        new("Русский (Russian)", "ru")
    ];
}