using System.Collections.ObjectModel;
using PleasantUI.Controls;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class SelectionListPageView : LocalizedUserControl
{
    public ObservableCollection<SelectionListItem> Items { get; } =
    [
        new() { Title = "Mountain Sunrise",  Subtitle = "Landscape · Nature",   Timestamp = "2024-03-15 08:30" },
        new() { Title = "City at Night",     Subtitle = "Urban · Architecture", Timestamp = "2024-02-20 22:10" },
        new() { Title = "Forest Path",       Subtitle = "Nature · Hiking",      Timestamp = "2024-01-05 14:45" },
        new() { Title = "Ocean Waves",       Subtitle = "Seascape · Travel",    Timestamp = "2023-12-10 16:00" },
        new() { Title = "Desert Dunes",      Subtitle = "Landscape · Arid",     Timestamp = "2023-11-22 11:20" },
    ];

    public ObservableCollection<SelectionListItem> ChipItems { get; } =
    [
        new() { Title = "Nature" },
        new() { Title = "Urban" },
        new() { Title = "Travel" },
        new() { Title = "Architecture" },
        new() { Title = "Portraits" },
        new() { Title = "Abstract" },
    ];

    public ObservableCollection<object> SelectedItems { get; } = [];

    public SelectionListPageView()
    {
        InitializeComponent();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
    }
}
