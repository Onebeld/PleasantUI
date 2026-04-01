using Avalonia.Controls;
using PleasantUI.Core.Localization;
using PleasantUI.ToolKit;

namespace PleasantUI.Example.Views.Pages.ToolkitPages;

public partial class MessageBoxPageView : UserControl
{
    public MessageBoxPageView()
    {
        InitializeComponent();
        
        DefaultMBButton.Click += (_, _) => MessageBox.Show(
            PleasantUiExampleApp.Main,
            Localizer.TrDefault("MessageBoxTitle", "MessageBox"),
            Localizer.TrDefault("MessageBoxText", "This is a sample message box dialog."));
    }
}