using Avalonia.Controls;
using PleasantUI.Controls;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class TimelinePageView : LocalizedUserControl
{
    public TimelinePageView()
    {
        InitializeComponent();
        WireRadioButtons();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireRadioButtons();
    }

    private void WireRadioButtons()
    {
        ModeLeft.IsCheckedChanged     += (_, _) => ApplyMode();
        ModeRight.IsCheckedChanged    += (_, _) => ApplyMode();
        ModeCenter.IsCheckedChanged   += (_, _) => ApplyMode();
        ModeAlternate.IsCheckedChanged += (_, _) => ApplyMode();
        ApplyMode();
    }

    private void ApplyMode()
    {
        DemoTimeline.Mode = (ModeLeft.IsChecked, ModeRight.IsChecked, ModeCenter.IsChecked, ModeAlternate.IsChecked) switch
        {
            (true, _, _, _) => TimelineDisplayMode.Left,
            (_, true, _, _) => TimelineDisplayMode.Right,
            (_, _, true, _) => TimelineDisplayMode.Center,
            _               => TimelineDisplayMode.Alternate,
        };
    }
}
