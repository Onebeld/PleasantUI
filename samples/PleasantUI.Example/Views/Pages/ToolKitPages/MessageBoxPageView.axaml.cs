using Avalonia.Controls;
using PleasantUI.ToolKit;

namespace PleasantUI.Example.Views.Pages.ToolkitPages;

public partial class MessageBoxPageView : UserControl
{
    public MessageBoxPageView()
    {
        InitializeComponent();
        
        DefaultMBButton.Click += (_, _) => MessageBox.Show(PleasantUiExampleApp.Main, "Title", "Hello, world!");
    }
}