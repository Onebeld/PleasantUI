using Avalonia.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Pages;

public class HomePage : IPage
{
    public string Title { get; } = "Home";

    public bool ShowTitle { get; } = false;

    public Control Content
    {
        get { return new HomePageView(); }
    }
}