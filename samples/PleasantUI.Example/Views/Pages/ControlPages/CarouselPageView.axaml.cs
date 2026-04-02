namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class CarouselPageView : LocalizedUserControl
{
    public CarouselPageView()
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
        PrevButton.Click += (_, _) =>
        {
            if (DemoCarousel.SelectedIndex > 0)
                DemoCarousel.SelectedIndex--;
        };

        NextButton.Click += (_, _) =>
        {
            if (DemoCarousel.SelectedIndex < DemoCarousel.ItemCount - 1)
                DemoCarousel.SelectedIndex++;
        };
    }
}
