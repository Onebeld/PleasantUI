using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Example.Pages;

namespace PleasantUI.Example.Android;

public partial class PleasantMainView : PleasantView
{
    public PleasantMainView()
    {
        InitializeComponent();
        
        // Since the program compiled by AOT does not work, we have to use these lines
        HomeScreenPage.FuncControl += () => new HomePage();
        SettingsPage.FuncControl += () => new SettingsPage();
        ControlsPage.FuncControl += () => new ControlsPage();
        
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        PleasantSettings.Instance.Save();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        App.ViewModel.NotificationManager = new PleasantNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };
    }
}