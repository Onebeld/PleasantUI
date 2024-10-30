using CommunityToolkit.Mvvm.ComponentModel;
using PleasantUI.Core;
using PleasantUI.Core.Models;
using PleasantUI.ToolKit;

namespace PleasantUI.Example.ViewModels.Pages;

public class SettingsViewModel : ObservableObject
{
    public Theme? SelectedTheme
    {
        get => PleasantTheme.Themes.FirstOrDefault(theme => theme.Name == PleasantSettings.Instance.Theme);
        set => PleasantSettings.Instance.Theme = value?.Name ?? "System";
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