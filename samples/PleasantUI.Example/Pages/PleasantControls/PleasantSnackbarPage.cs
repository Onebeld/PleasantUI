using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages.PleasantControlPages;

namespace PleasantUI.Example.Pages.PleasantControls;

public class PleasantSnackbarPage : IPage
{
    public string Title { get; } = "PleasantSnackbar";
    public bool ShowTitle { get; } = true;

    public Control Content
    {
        get
        {
            return new PleasantSnackbarPageView();
        }
    }
}