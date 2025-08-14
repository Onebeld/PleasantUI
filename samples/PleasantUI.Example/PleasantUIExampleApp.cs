using System.Resources;
using Avalonia;
using Avalonia.Controls;
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

    public static AppViewModel ViewModel { get; } = null!;

    public static TopLevel? TopLevel { get; protected set; }

    static PleasantUiExampleApp()
    {
        if (!Design.IsDesignMode)
            ViewModel = new AppViewModel(new EventAggregator());
    }

    public PleasantUiExampleApp()
    {
        if (!Design.IsDesignMode)
            DataContext = ViewModel;

        Localizer.AddRes(new ResourceManager(typeof(Properties.Localizations.App)));
        Localizer.AddRes(new ResourceManager(typeof(Properties.Localizations.Library)));

        Localizer.ChangeLang("en");
    }

    public static string LanguageKey { get; set; } = "en";

    public static readonly Language[] Languages =
    [
        new("English (English)", "en"),
        new("Русский (Russian)", "ru")
    ];
}