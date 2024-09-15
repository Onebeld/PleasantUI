using PleasantUI.Controls;
using PleasantUI.Core;

namespace PleasantUI.Example.Android;

public partial class PleasantMainView : PleasantView
{
    public PleasantMainView()
    {
        InitializeComponent();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        PleasantSettings.Save();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        /*App.ViewModel.NotificationManager = new PleasantNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };*/
    }
}