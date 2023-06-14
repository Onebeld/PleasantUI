using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PleasantUI.Controls;
using PleasantUI.Windows;

namespace PleasantUI.Example.ViewModels;

public class ApplicationViewModel : ViewModelBase
{
    public void ShowModalWindow()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MessageBox.Show(desktop.MainWindow as PleasantWindow, "Test", "This is Test MessageBox", MessageBoxButtons.Ok);
        }
    }

    public void ShowColorPicker()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ColorPickerWindow.SelectColor(desktop.MainWindow as PleasantWindow);
        }
    }
}