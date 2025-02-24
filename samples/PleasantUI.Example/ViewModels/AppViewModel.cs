using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using PleasantUI.Example.Fabrics;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Models;
using PleasantUI.Example.Pages.BasicControls;

namespace PleasantUI.Example.ViewModels;

public partial class AppViewModel : ObservableObject
{
    /// <summary>
    /// The current page
    /// </summary>
    [ObservableProperty]
    private IPage _page = null!;
    
    /// <summary>
    /// Indicates whether the animation should be forward or backward
    /// </summary>
    [ObservableProperty]
    private bool _isForwardAnimation = true;

    public AvaloniaList<ControlPageCard> BasicControlPageCards { get; }
    
    public AvaloniaList<ControlPageCard> PleasantControlPageCards { get; }
    
    public AvaloniaList<ControlPageCard> ToolKitPageCards { get; }

    public AppViewModel()
    {
        ControlPageCardsFactory factory = new();
        
        BasicControlPageCards = factory.CreateBasicControlPageCards();
        PleasantControlPageCards = factory.CreatePleasantControlPageCards();
        ToolKitPageCards = factory.CreateToolkitControlPageCards();
        
        Page = new HomePage();
        
        WeakReferenceMessenger.Default.Register<AppViewModel, ValueChangedMessage<IPage>>(this, (recipient, message) =>
        {
            recipient.ChangePageCommand.Execute(message.Value);
        });
    }

    /// <summary>
    /// Changes the current page
    /// </summary>
    /// <param name="page">The new page</param>
    /// <param name="forward">The direction of the animation</param>
    [RelayCommand]
    private void ChangePage(IPage page)
    {
        IsForwardAnimation = true;
        Page = page;
    }

    /// <summary>
    /// Goes back to the home page
    /// </summary>
    [RelayCommand]
    private void BackToHomePage()
    {
        IsForwardAnimation = false;
        Page = new HomePage();
    }
}