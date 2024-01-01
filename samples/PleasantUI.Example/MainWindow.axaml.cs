using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Interfaces;
using PleasantUI.Example.Pages;

namespace PleasantUI.Example;

public partial class MainWindow : PleasantWindow
{
    public MainWindow()
    {
        InitializeComponent();

        // Since the program compiled by AOT does not work, we have to use these lines
        HomeScreenPage.FuncControl += () => new HomePage();
        SettingsPage.FuncControl += () => new SettingsPage();
        ControlsPage.FuncControl += () => new ControlsPage();
        
        Closing += OnClosing;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (OperatingSystem.IsMacOS())
        {
            EnableTitleBarMargin = true;
            TitleBarType = PleasantTitleBarType.Classic;
        }
        
        App.ViewModel.NotificationManager = new PleasantNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            ZIndex = 1
        };
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e) => PleasantSettings.Instance.Save();
}