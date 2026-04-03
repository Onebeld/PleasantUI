using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class InstallWizardPageView : LocalizedUserControl
{
    public InstallWizardPageView()
    {
        InitializeComponent();
        ShowModalButton.Click  += OnShowModal;
        ShowWindowButton.Click += OnShowWindow;
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        ShowModalButton.Click  += OnShowModal;
        ShowWindowButton.Click += OnShowWindow;
    }

    private async void OnShowModal(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;
        await InstallWizard.ShowAsModalAsync(BuildWizard(), topLevel);
    }

    private async void OnShowWindow(object? sender, RoutedEventArgs e)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        await InstallWizard.ShowAsWindowAsync(BuildWizard(), owner);
    }

    private static InstallWizard BuildWizard()
    {
        // Read localized strings at call time so modal/window modes respect the current language
        var loc = PleasantUI.Core.Localization.Localizer.Instance;
        string Tr(string key) => loc.TryGetString(key, out var v) ? v : key;

        var wizard = new InstallWizard
        {
            AppName    = Tr("InstallWizard/AppName"),
            FooterText = Tr("InstallWizard/FooterText"),
        };

        wizard.Steps.Add(new WizardStep
        {
            Header      = Tr("InstallWizard/Step1Header"),
            Description = Tr("InstallWizard/Step1Desc"),
            ContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object?>((_, _) =>
                new StackPanel
                {
                    Spacing  = 12,
                    Children =
                    {
                        new TextBlock { Text = Tr("InstallWizard/Step1Body"), TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new CheckBox  { Content = Tr("InstallWizard/Step1Check") }
                    }
                })
        });

        wizard.Steps.Add(new WizardStep
        {
            Header      = Tr("InstallWizard/Step2Header"),
            Description = Tr("InstallWizard/Step2Desc"),
            ContentTemplate = new FuncDataTemplate<object?>((_, _) =>
                new StackPanel
                {
                    Spacing  = 12,
                    Children =
                    {
                        new Border
                        {
                            MinHeight = 80,
                            MaxHeight = 260,
                            Padding   = new Avalonia.Thickness(12),
                            Child     = new ScrollViewer
                            {
                                VerticalScrollBarVisibility   = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                                Content = new TextBlock
                                {
                                    Text         = Tr("InstallWizard/LicenseText"),
                                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                    FontSize     = 12
                                }
                            }
                        },
                        new CheckBox { Content = Tr("InstallWizard/Step2Check") }
                    }
                })
        });

        wizard.Steps.Add(new WizardStep
        {
            Header      = Tr("InstallWizard/Step3Header"),
            Description = Tr("InstallWizard/Step3Desc"),
            ContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object?>((_, _) =>
                new StackPanel
                {
                    Spacing  = 8,
                    Children =
                    {
                        new RadioButton { Content = Tr("InstallWizard/TypeStandard"), IsChecked = true, GroupName = "ModalInstallType" },
                        new TextBlock   { Text = Tr("InstallWizard/TypeStandardDesc"), Margin = new Avalonia.Thickness(24,0,0,8), FontSize = 12, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new RadioButton { Content = Tr("InstallWizard/TypeCustom"), GroupName = "ModalInstallType" },
                        new TextBlock   { Text = Tr("InstallWizard/TypeCustomDesc"), Margin = new Avalonia.Thickness(24,0,0,0), FontSize = 12, TextWrapping = Avalonia.Media.TextWrapping.Wrap }
                    }
                })
        });

        wizard.Steps.Add(new WizardStep
        {
            Header      = Tr("InstallWizard/Step4Header"),
            Description = Tr("InstallWizard/Step4Desc"),
            ContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object?>((_, _) =>
                new StackPanel
                {
                    Spacing  = 12,
                    Children =
                    {
                        new TextBlock   { Text = Tr("InstallWizard/InstallingStatus"), FontSize = 12 },
                        new ProgressBar { IsIndeterminate = true, Height = 4 },
                        new TextBlock   { Text = Tr("InstallWizard/InstallingFiles"), FontSize = 11 }
                    }
                })
        });

        wizard.Steps.Add(new WizardStep
        {
            Header      = Tr("InstallWizard/Step5Header"),
            Description = Tr("InstallWizard/Step5Desc"),
            ContentTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object?>((_, _) =>
                new StackPanel
                {
                    Spacing             = 12,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Children            =
                    {
                        new Border
                        {
                            Width               = 56,
                            Height              = 56,
                            CornerRadius        = new Avalonia.CornerRadius(28),
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Background          = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2E7D32")),
                            Child               = new Avalonia.Controls.Shapes.Path
                            {
                                Data                = Avalonia.Media.Geometry.Parse("M10,18 L5,13 L6.4,11.6 L10,15.2 L17.6,7.6 L19,9"),
                                Stroke              = Avalonia.Media.Brushes.White,
                                StrokeThickness     = 2.5,
                                StrokeLineCap       = Avalonia.Media.PenLineCap.Round,
                                StrokeJoin          = Avalonia.Media.PenLineJoin.Round,
                                Stretch             = Avalonia.Media.Stretch.Uniform,
                                Width               = 28,
                                Height              = 28,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                VerticalAlignment   = Avalonia.Layout.VerticalAlignment.Center
                            }
                        },
                        new TextBlock
                        {
                            Text                = Tr("InstallWizard/SuccessMessage"),
                            FontSize            = 16,
                            FontWeight          = Avalonia.Media.FontWeight.SemiBold,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text                = Tr("InstallWizard/SuccessDetail"),
                            FontSize            = 12,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            TextWrapping        = Avalonia.Media.TextWrapping.Wrap
                        }
                    }
                })
        });

        return wizard;
    }
}
