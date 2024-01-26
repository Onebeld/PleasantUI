using Avalonia;
using Avalonia.Controls;
using PleasantUI.Core.Interfaces;
using PleasantUI.Example.ViewModels;

namespace PleasantUI.Example;

public class PleasantUIExampleApp : Application
{
    public static PleasantTheme PleasantTheme { get; protected set; } = null!;

    public static IPleasantWindow Main { get; protected set; } = null!;
    
    public static AppViewModel ViewModel { get; }
    
    public static TopLevel? TopLevel { get; protected set; }

    static PleasantUIExampleApp()
    {
        ViewModel = new AppViewModel();
    }

    public PleasantUIExampleApp()
    {
        DataContext = ViewModel;
    }
}