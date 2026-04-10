using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PleasantUI.ToolKit.Controls;
using PleasantUI.Example.Views.Pages;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class StepDialogPageView : LocalizedUserControl
{
    public StepDialogPageView()
    {
        InitializeComponent();
        WireHandlers();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        OpenBasicBtn.Click -= OnOpenBasic;
        OpenAuthBtn.Click  -= OnOpenAuth;
        OpenBasicBtn.Click += OnOpenBasic;
        OpenAuthBtn.Click  += OnOpenAuth;
    }

    private async void OnOpenBasic(object? s, RoutedEventArgs e)
    {
        var dialog = new StepDialog
        {
            Title             = Localizer.Tr("StepDialog/SetupWizardTitle"),
            Description       = Localizer.Tr("StepDialog/SetupWizardDescription"),
            SecondaryButtonText = Localizer.Tr("StepDialog/Cancel"),
            PrimaryButtonText   = Localizer.Tr("StepDialog/Finish"),
            StatusMessage     = Localizer.Tr("StepDialog/WaitingForConfirmation")
        };

        dialog.Steps.Add(new StepItem { Header = Localizer.Tr("StepDialog/AcceptLicenseHeader"),
            Description = Localizer.Tr("StepDialog/AcceptLicenseDescription") });
        dialog.Steps.Add(new StepItem { Header = Localizer.Tr("StepDialog/ChooseDirectoryHeader"),
            Content = new TextBox { Watermark = Localizer.Tr("StepDialog/DefaultDirectoryPath"), Width = 280 } });
        dialog.Steps.Add(new StepItem { Header = Localizer.Tr("StepDialog/CompleteInstallationHeader"),
            Description = Localizer.Tr("StepDialog/CompleteInstallationDescription") });

        dialog.PrimaryButtonClicked   += (_, _) => { ResultLabel.Text = Localizer.Tr("StepDialog/Finished"); _ = dialog.CloseAsync(); };
        dialog.SecondaryButtonClicked += (_, _) => { ResultLabel.Text = Localizer.Tr("StepDialog/Cancelled"); _ = dialog.CloseAsync(); };

        var topLevel = TopLevel.GetTopLevel(this);
        await dialog.ShowAsync(topLevel);
    }

    private async void OnOpenAuth(object? s, RoutedEventArgs e)
    {
        var dialog = new StepDialog
        {
            Title               = Localizer.Tr("StepDialog/SignInAzureTitle"),
            Description         = Localizer.Tr("StepDialog/SignInAzureDescription"),
            SecondaryButtonText = Localizer.Tr("StepDialog/Cancel"),
            StatusMessage       = Localizer.Tr("StepDialog/WaitingForAuth")
        };

        dialog.Steps.Add(new StepItem
        {
            Header  = Localizer.Tr("StepDialog/OpenAuthPageHeader"),
            Content = new Button
            {
                Content = Localizer.Tr("StepDialog/OpenAuthPageButton"),
                Theme   = Application.Current?.FindResource("AccentButtonTheme") as Avalonia.Styling.ControlTheme,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            }
        });

        dialog.Steps.Add(new StepItem
        {
            Header  = Localizer.Tr("StepDialog/EnterCodeHeader"),
            Content = new Border
            {
                Background    = Application.Current?.FindResource("BackgroundColor3") as Avalonia.Media.IBrush,
                CornerRadius  = new Avalonia.CornerRadius(8),
                Padding       = new Avalonia.Thickness(12, 8),
                Child         = new TextBlock
                {
                    Text       = Localizer.Tr("StepDialog/DeviceCode"),
                    FontFamily = new Avalonia.Media.FontFamily("Consolas,Menlo,Monaco,monospace"),
                    FontSize   = 18,
                    FontWeight = Avalonia.Media.FontWeight.Bold
                }
            }
        });

        dialog.Steps.Add(new StepItem { Header = Localizer.Tr("StepDialog/CompleteSignInHeader") });

        dialog.SecondaryButtonClicked += (_, _) => { ResultLabel.Text = Localizer.Tr("StepDialog/AuthCancelled"); _ = dialog.CloseAsync(); };

        var topLevel = TopLevel.GetTopLevel(this);
        await dialog.ShowAsync(topLevel);
        if (ResultLabel.Text == "—") ResultLabel.Text = Localizer.Tr("StepDialog/AuthClosed");
    }
}
