using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ButtonPage : UserControl, IPage
{
    public string Title { get; } = "Button";
    
    public ButtonPage()
    {
        InitializeComponent();
    }
}