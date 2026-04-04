using Avalonia.Controls;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class PleasantDrawerPageView : LocalizedUserControl
{
    public PleasantDrawerPageView()
    {
        InitializeComponent();
        WireButtons();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireButtons();
    }

    private void WireButtons()
    {
        OpenRightButton.Click  += (_, _) => OpenDrawer(DrawerPosition.Right);
        OpenLeftButton.Click   += (_, _) => OpenDrawer(DrawerPosition.Left);
        OpenTopButton.Click    += (_, _) => OpenDrawer(DrawerPosition.Top);
        OpenBottomButton.Click += (_, _) => OpenDrawer(DrawerPosition.Bottom);
    }

    private async void OpenDrawer(DrawerPosition position)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        string Tr(string key) => Localizer.Instance.TryGetString(key, out var v) ? v : key;

        var drawer = new PleasantDrawer
        {
            Title         = Tr("PleasantDrawer/DrawerTitle"),
            Position      = position,
            PanelWidth    = position is DrawerPosition.Left or DrawerPosition.Right ? 320 : double.NaN,
            PanelHeight   = position is DrawerPosition.Top or DrawerPosition.Bottom ? 260 : double.NaN,
            FooterContent = new Button
            {
                Content             = Tr("PleasantDrawer/FooterClose"),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            }
        };

        // Wire footer close button
        if (drawer.FooterContent is Button closeBtn)
            closeBtn.Click += async (_, _) => await drawer.CloseAsync();

        drawer.Content = new StackPanel
        {
            Spacing  = 12,
            Children =
            {
                new TextBlock
                {
                    Text         = Tr("PleasantDrawer/DrawerContent"),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontSize     = 13
                },
                new Separator(),
                new CheckBox { Content = "Option A" },
                new CheckBox { Content = "Option B" },
                new CheckBox { Content = "Option C" },
                new Slider   { Minimum = 0, Maximum = 100, Value = 40 }
            }
        };

        await drawer.ShowAsync(topLevel);
    }
}
