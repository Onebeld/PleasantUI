using Avalonia.Controls;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class CheckBoxPage : UserControl, IPage
{
    public string Title { get; } = "CheckBox";
    
    public CheckBoxPage()
    {
        InitializeComponent();
    }

}