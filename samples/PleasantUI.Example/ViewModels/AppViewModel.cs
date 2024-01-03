using Avalonia.Controls.Notifications;
using PleasantUI.Example.Interfaces;
using HomePage = PleasantUI.Example.Views.Pages.HomePage;

namespace PleasantUI.Example.ViewModels;

public class AppViewModel : ViewModelBase
{
    private IPage _page = null!;
    
    public IManagedNotificationManager? NotificationManager { get; set; }
    
    public IPage Page
    {
        get => _page;
        set => RaiseAndSet(ref _page, value);
    }

    public bool IsHomePage
    {
        get => Page is HomePage;
    }

    public AppViewModel()
    {
        Page = new HomePage();
    }

    /// <summary>
    /// Changes the current page
    /// </summary>
    /// <param name="page">The new page</param>
    public void ChangePage(IPage page)
    {
        Page = page;
        RaisePropertyChanged(nameof(IsHomePage));
    }

    /// <summary>
    /// Goes back to the home page
    /// </summary>
    public void BackToHomePage()
    {
        Page = new HomePage();
        RaisePropertyChanged(nameof(IsHomePage));
    }
}