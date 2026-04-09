using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PleasantUI.Controls;
using PleasantUI.Example.Views.Pages;

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
            Title             = "Setup wizard",
            Description       = "Follow these steps to complete the setup.",
            SecondaryButtonText = "Cancel",
            PrimaryButtonText   = "Finish",
            StatusMessage     = "Waiting for confirmation…"
        };

        dialog.Steps.Add(new StepItem { Header = "Accept the license agreement",
            Description = "Read and accept the terms before continuing." });
        dialog.Steps.Add(new StepItem { Header = "Choose installation directory",
            Content = new TextBox { Watermark = "C:\\Program Files\\MyApp", Width = 280 } });
        dialog.Steps.Add(new StepItem { Header = "Complete installation",
            Description = "Click Finish to apply the settings." });

        dialog.PrimaryButtonClicked   += (_, _) => { ResultLabel.Text = "Finished"; _ = dialog.CloseAsync(); };
        dialog.SecondaryButtonClicked += (_, _) => { ResultLabel.Text = "Cancelled"; _ = dialog.CloseAsync(); };

        var topLevel = TopLevel.GetTopLevel(this);
        await dialog.ShowAsync(topLevel);
    }

    private async void OnOpenAuth(object? s, RoutedEventArgs e)
    {
        var dialog = new StepDialog
        {
            Title               = "Sign in with Azure",
            Description         = "Browser authentication failed. Use the device code flow instead.",
            SecondaryButtonText = "Cancel",
            StatusMessage       = "Waiting for authentication to complete…"
        };

        dialog.Steps.Add(new StepItem
        {
            Header  = "Open the authentication page",
            Content = new Button
            {
                Content = "Open microsoft.com/devicelogin",
                Theme   = Application.Current?.FindResource("AccentButtonTheme") as Avalonia.Styling.ControlTheme,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            }
        });

        dialog.Steps.Add(new StepItem
        {
            Header  = "Enter this code",
            Content = new Border
            {
                Background    = Application.Current?.FindResource("BackgroundColor3") as Avalonia.Media.IBrush,
                CornerRadius  = new Avalonia.CornerRadius(8),
                Padding       = new Avalonia.Thickness(12, 8),
                Child         = new TextBlock
                {
                    Text       = "ABCD-1234",
                    FontFamily = new Avalonia.Media.FontFamily("Consolas,Menlo,Monaco,monospace"),
                    FontSize   = 18,
                    FontWeight = Avalonia.Media.FontWeight.Bold
                }
            }
        });

        dialog.Steps.Add(new StepItem { Header = "Complete sign-in in your browser" });

        dialog.SecondaryButtonClicked += (_, _) => { ResultLabel.Text = "Auth cancelled"; _ = dialog.CloseAsync(); };

        var topLevel = TopLevel.GetTopLevel(this);
        await dialog.ShowAsync(topLevel);
        if (ResultLabel.Text == "—") ResultLabel.Text = "Auth closed";
    }
}
