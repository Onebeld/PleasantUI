using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PleasantUI.Core;
using PleasantUI.Core.Models;
using PleasantUI.ToolKit;

namespace PleasantUI.Example.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject
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

    [RelayCommand]
    private async Task CreateThemeAsync()
    {
        CustomTheme? newCustomTheme = await ThemeEditorWindow.EditTheme(PleasantUiExampleApp.Main, null);
        
        if (newCustomTheme is null)
            return;
        
        PleasantTheme.CustomThemes.Add(newCustomTheme);
    }

    [RelayCommand]
    private async Task EditThemeAsync(CustomTheme customTheme)
    {
        CustomTheme? newCustomTheme = await ThemeEditorWindow.EditTheme(PleasantUiExampleApp.Main, customTheme);
        
        if (newCustomTheme is null)
            return;
        
        PleasantUiExampleApp.PleasantTheme.EditCustomTheme(customTheme, newCustomTheme);
    }

    [RelayCommand]
    private void DeleteTheme(CustomTheme customTheme)
    {
        PleasantTheme.CustomThemes.Remove(customTheme);
    }
}