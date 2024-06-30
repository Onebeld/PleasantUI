using PleasantUI.Core;
using PleasantUI.Core.Enums;

namespace PleasantUI.Example.ViewModels.Pages;

public class SettingsViewModel : ViewModelBase
{
    public int SelectedIndexTheme
    {
        get
        {
            return PleasantSettings.Instance.Theme switch
            {
                Theme.Light => 1,
                Theme.Dark => 2,
                Theme.Mint => 3,
                Theme.Strawberry => 4,
                
                _ => 0
            };
        }
        set
        {
            PleasantSettings.Instance.Theme = value switch
            {
                1 => Theme.Light,
                2 => Theme.Dark,
                3 => Theme.Mint,
                4 => Theme.Strawberry,

                _ => Theme.System
            };
        }
    }
}