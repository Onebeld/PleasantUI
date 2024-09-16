using PleasantUI.Core;
using PleasantUI.Core.Models;
using PleasantUI.ToolKit;

namespace PleasantUI.Example.ViewModels.Pages;

public class SettingsViewModel : ViewModelBase
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

    public async void CreateTheme()
    {
        CustomTheme? newCustomTheme = await ThemeEditorWindow.EditTheme(PleasantUiExampleApp.Main, null);
        
        if (newCustomTheme is null)
            return;
        
        PleasantTheme.CustomThemes.Add(newCustomTheme);
    }

    public async void EditTheme(CustomTheme customTheme)
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