using PleasantUI.Core;
using PleasantUI.Core.Localization;
using PleasantUI.Core.Models;
using PleasantUI.Example.Structures;
using PleasantUI.ToolKit;

namespace PleasantUI.Example.ViewModels.Pages;

public partial class SettingsViewModel
{
    public Language SelectedLanguage
    {
        get => PleasantUiExampleApp.Languages.First(language => language.Key == PleasantUiExampleApp.LanguageKey);
        set
        {
            PleasantUiExampleApp.LanguageKey = value.Key;
            Localizer.ChangeLang(value.Key);
        }
    }

    public Theme? SelectedTheme
    {
        get => PleasantSettings.Current?.Theme is { } themeName
            ? PleasantTheme.Themes.FirstOrDefault(theme => theme.Name == themeName)
            : null;

        set
        {
            if (PleasantSettings.Current is not null)
                PleasantSettings.Current.Theme = value?.Name ?? "System";
        }
    }

    public CustomTheme? SelectedCustomTheme
    {
        get => PleasantTheme.SelectedCustomTheme;
        set => PleasantTheme.SelectedCustomTheme = value;
    }

    public async Task CreateThemeAsync()
    {
        CustomTheme? newCustomTheme = await ThemeEditorWindow.EditTheme(PleasantUiExampleApp.Main, null);

        if (newCustomTheme is null)
            return;

        PleasantTheme.CustomThemes.Add(newCustomTheme);
    }

    public async Task EditThemeAsync(CustomTheme customTheme)
    {
        CustomTheme? newCustomTheme = await ThemeEditorWindow.EditTheme(PleasantUiExampleApp.Main, customTheme);

        if (newCustomTheme is null)
            return;

        PleasantUiExampleApp.PleasantTheme.EditCustomTheme(customTheme, newCustomTheme);
    }

    public void DeleteTheme(CustomTheme customTheme)
    {
        PleasantTheme.CustomThemes.Remove(customTheme);
    }
}