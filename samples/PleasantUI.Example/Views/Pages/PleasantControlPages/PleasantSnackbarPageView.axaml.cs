using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Material.Icons;
using Material.Icons.Avalonia;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class PleasantSnackbarPageView : LocalizedUserControl
{
    private static string T(string key, string fallback) =>
        Localizer.TrDefault(key, fallback, "Snackbar");

    public PleasantSnackbarPageView()
    {
        InitializeComponent();
        WireHandlers();
    }
    // Complex constructor — don't re-run InitializeComponent

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        BtnInformation.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("InformationMsg", "This is an informational message."))
            { Icon = CreateIcon(MaterialIconKind.InformationOutline), NotificationType = NotificationType.Information });

        BtnSuccess.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("SuccessMsg", "Operation completed successfully."))
            { Icon = CreateIcon(MaterialIconKind.CheckCircleOutline), NotificationType = NotificationType.Success });

        BtnWarning.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("WarningMsg", "Disk space is running low."))
            { Icon = CreateIcon(MaterialIconKind.AlertOutline), NotificationType = NotificationType.Warning });

        BtnError.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("ErrorMsg", "Failed to save the file."))
            { Icon = CreateIcon(MaterialIconKind.CloseCircleOutline), NotificationType = NotificationType.Error });

        BtnLongMessage.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("LongMsg", "Your export has finished. The file has been saved to your Downloads folder and is ready to open."))
            { Icon = CreateIcon(MaterialIconKind.FileExportOutline), NotificationType = NotificationType.Information, IsClosable = true, TimeSpan = TimeSpan.FromSeconds(8) });

        BtnWithTitle.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("WithTitleMsg", "Your changes have been saved to the cloud."))
            { Title = T("WithTitleTitle", "Saved"), Icon = CreateIcon(MaterialIconKind.CloudCheckOutline), NotificationType = NotificationType.Success });

        BtnWithAction.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("WithActionMsg", "Item moved to trash."))
            {
                Icon = CreateIcon(MaterialIconKind.DeleteOutline), NotificationType = NotificationType.Information,
                ButtonText = T("Undo", "Undo"),
                ButtonAction = () => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
                    new PleasantSnackbarOptions(T("UndoneMsg", "Action undone."))
                    { Icon = CreateIcon(MaterialIconKind.UndoVariant), NotificationType = NotificationType.Success })
            });

        BtnWithCustomAction.Click += (_, _) =>
        {
            var actionBtn = new Button
            {
                Content = T("ViewDetails", "View"),
                Theme = Application.Current!.TryFindResource("AccentButtonTheme", out object? t) ? t as Avalonia.Styling.ControlTheme : null,
                CornerRadius = new CornerRadius(99),
                Padding = new Thickness(10, 4)
            };
            actionBtn.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
                new PleasantSnackbarOptions(T("ViewedMsg", "Opening details…")) { NotificationType = NotificationType.Information });

            PleasantSnackbar.Show(PleasantUiExampleApp.Main,
                new PleasantSnackbarOptions(T("WithCustomActionMsg", "New update available."))
                { Icon = CreateIcon(MaterialIconKind.AccessPointCheck), NotificationType = NotificationType.Information, ActionButton = actionBtn });
        };

        BtnClosable.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("ClosableMsg", "This snackbar has a close button."))
            { Title = T("ClosableTitle", "Dismissable"), Icon = CreateIcon(MaterialIconKind.InformationOutline), NotificationType = NotificationType.Information, IsClosable = true, TimeSpan = TimeSpan.FromSeconds(10) });

        BtnWithEvents.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("WithEventsMsg", "Tap or wait to dismiss — events are tracked."))
            {
                Icon = CreateIcon(MaterialIconKind.BellOutline), NotificationType = NotificationType.Information, IsClosable = true,
                Closing = (_, args) => { LastEventText.Text = $"Closing ({args.Reason})"; },
                Closed  = (_, args) => { LastEventText.Text = $"Closed ({args.Reason})"; }
            });
    }
    
    private MaterialIcon CreateIcon(MaterialIconKind kind)
    {
        return new MaterialIcon()
        {
            Kind = kind
        };
    }
}
