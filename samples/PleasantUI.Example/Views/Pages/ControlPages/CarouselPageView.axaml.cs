using Avalonia.Controls;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class CarouselPageView : UserControl
{
    public CarouselPageView()
    {
        InitializeComponent();

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