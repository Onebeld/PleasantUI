using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Pages;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Views.Pages;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ButtonPage.Click += (_, _) => PleasantUIExampleApp.ViewModel.ChangePage(new ButtonPage());
        CheckBoxPage.Click += (_, _) => PleasantUIExampleApp.ViewModel.ChangePage(new CheckBoxPage());
    }
}