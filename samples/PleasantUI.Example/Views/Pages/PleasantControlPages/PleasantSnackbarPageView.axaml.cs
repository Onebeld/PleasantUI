using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class PleasantSnackbarPageView : UserControl
{
    private static string T(string key, string fallback) =>
        Localizer.TrDefault(key, fallback, "Snackbar");

    public PleasantSnackbarPageView()
    {
        InitializeComponent();

        BtnInformation.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("InformationMsg", "This is an informational message."))
            {
                Icon             = MaterialIcons.InformationOutline,
                NotificationType = NotificationType.Information
            });

        BtnSuccess.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("SuccessMsg", "Operation completed successfully."))
            {
                Icon             = MaterialIcons.CheckCircleOutline,
                NotificationType = NotificationType.Success
            });

        BtnWarning.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("WarningMsg", "Disk space is running low."))
            {
                Icon             = MaterialIcons.AlertOutline,
                NotificationType = NotificationType.Warning
            });

        BtnError.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("ErrorMsg", "Failed to save the file."))
            {
                Icon             = MaterialIcons.CloseCircleOutline,
                NotificationType = NotificationType.Error
            });

        BtnWithTitle.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("WithTitleMsg", "Your changes have been saved to the cloud."))
            {
                Title            = T("WithTitleTitle", "Saved"),
                Icon             = MaterialIcons.CloudCheckOutline,
                NotificationType = NotificationType.Success
            });

        BtnWithAction.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("WithActionMsg", "Item moved to trash."))
            {
                Icon             = MaterialIcons.DeleteOutline,
                NotificationType = NotificationType.Information,
                ButtonText       = T("Undo", "Undo"),
                ButtonAction     = () => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
                    new PleasantSnackbarOptions(T("UndoneMsg", "Action undone."))
                    {
                        Icon             = MaterialIcons.UndoVariant,
                        NotificationType = NotificationType.Success
                    })
            });

        BtnWithCustomAction.Click += (_, _) =>
        {
            var actionBtn = new Button
            {
                Content      = T("ViewDetails", "View"),
                Theme        = Application.Current!.TryFindResource("AccentButtonTheme", out object? t)
                               ? t as Avalonia.Styling.ControlTheme : null,
                CornerRadius = new Avalonia.CornerRadius(99),
                Padding      = new Avalonia.Thickness(10, 4)
            };
            actionBtn.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
                new PleasantSnackbarOptions(T("ViewedMsg", "Opening details…"))
                {
                    NotificationType = NotificationType.Information
                });

            PleasantSnackbar.Show(PleasantUiExampleApp.Main,
                new PleasantSnackbarOptions(T("WithCustomActionMsg", "New update available."))
                {
                    Icon             = MaterialIcons.AccessPointCheck,
                    NotificationType = NotificationType.Information,
                    ActionButton     = actionBtn
                });
        };

        BtnClosable.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("ClosableMsg", "This snackbar has a close button."))
            {
                Title            = T("ClosableTitle", "Dismissable"),
                Icon             = MaterialIcons.InformationOutline,
                NotificationType = NotificationType.Information,
                IsClosable       = true,
                TimeSpan         = TimeSpan.FromSeconds(10)
            });

        BtnWithEvents.Click += (_, _) => PleasantSnackbar.Show(PleasantUiExampleApp.Main,
            new PleasantSnackbarOptions(T("WithEventsMsg", "Tap or wait to dismiss — events are tracked."))
            {
                Icon             = MaterialIcons.BellOutline,
                NotificationType = NotificationType.Information,
                IsClosable       = true,
                Closing          = (_, args) =>
                {
                    LastEventText.Text = $"Closing ({args.Reason})";
                },
                Closed           = (_, args) =>
                {
                    LastEventText.Text = $"Closed ({args.Reason})";
                }
            });
    }
}
