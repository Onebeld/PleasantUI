using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using PleasantUI.Windows;

namespace PleasantUI.Example.ViewModels;

public class ApplicationViewModel : ViewModelBase
{
    public int SelectedIndexTheme
    {
        get
        {
            return PleasantSettings.Instance.Theme switch
            {
                Theme.Light => 1,
                Theme.Dark => 2,
                
                _ => 0
            };
        }
        set
        {
            PleasantSettings.Instance.Theme = value switch
            {
                1 => Theme.Light,
                2 => Theme.Dark,

                _ => Theme.System
            };
            
            App.PleasantTheme.UpdateTheme();
        }
    }
    
    public void ShowModalWindow()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MessageBox.Show(desktop.MainWindow as PleasantWindow, "Test", "This is Test MessageBox");
        }
    }

    public void ShowColorPicker()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ColorPickerWindow.SelectColor(desktop.MainWindow as PleasantWindow);
        }
    }
    
    public void ShowMiniWindow()
    {
        PleasantMiniWindow pleasantMiniWindow = new()
        {
            Content = "Hello world!"
        };

        pleasantMiniWindow.Show();
    }
}