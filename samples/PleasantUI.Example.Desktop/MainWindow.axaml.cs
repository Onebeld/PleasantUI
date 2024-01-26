using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls;
using PleasantUI.Core;

namespace PleasantUI.Example.Desktop;

public partial class MainWindow : PleasantWindow
{
    public MainWindow() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        PleasantUIExampleApp.ViewModel.NotificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };
    }
}