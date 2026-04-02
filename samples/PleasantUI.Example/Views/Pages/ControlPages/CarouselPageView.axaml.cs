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
        // Unwire old handlers first
        PrevButton.Click -= OnPrevClick;
        NextButton.Click -= OnNextClick;

        // Wire new handlers
        PrevButton.Click += OnPrevClick;
        NextButton.Click += OnNextClick;
    }

    private void OnPrevClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DemoCarousel.SelectedIndex > 0)
            DemoCarousel.SelectedIndex--;
    }

    private void OnNextClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DemoCarousel.SelectedIndex < DemoCarousel.ItemCount - 1)
            DemoCarousel.SelectedIndex++;
    }
}
