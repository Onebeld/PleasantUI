using System.Resources;
using Avalonia;
using Avalonia.Controls;
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

        // Load language from settings if available, otherwise use static LanguageKey
        if (PleasantSettings.Current is not null && !string.IsNullOrEmpty(PleasantSettings.Current.Language))
        {
            LanguageKey = PleasantSettings.Current.Language;
        }

        // IMPORTANT: don't force "en" here.
        // This app can be re-created without the process fully shutting down (hot reload / restart),
        // so static state (LanguageKey, Localizer singleton) may persist. Always sync Localizer to
        // the current LanguageKey so the Settings combobox and actual UI language can't diverge.
        Localizer.ChangeLang(LanguageKey);

        // CRITICAL: construct a fresh VM *after* localization is initialized.
        // On soft restarts (without full process shutdown), static singletons can survive.
        // Recreating the AppViewModel here prevents "welcome stuck in English" caused by
        // VM properties being initialized before resources/culture are ready.
        if (!Design.IsDesignMode)
        {
            ViewModel = new AppViewModel(new EventAggregator());
            DataContext = ViewModel;
        }
    }

    public static string LanguageKey { get; set; } = "en";

    public static readonly Language[] Languages =
    [
        new("English (English)", "en"),
        new("Русский (Russian)", "ru")
    ];
}