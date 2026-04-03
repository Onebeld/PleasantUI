using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using PleasantUI.Controls;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class SelectionListPageView : LocalizedUserControl
{
    public ObservableCollection<SelectionListItem> Items { get; }
    public ObservableCollection<SelectionListItem> ChipItems { get; }
    public ObservableCollection<object> SelectedItems { get; } = [];

    public SelectionListPageView()
    {
        Items = CreateItems();
        ChipItems = CreateChipItems();
        InitializeComponent();
    }

    protected override void ReinitializeComponent() => InitializeComponent();

    private static ObservableCollection<SelectionListItem> CreateItems()
    {
        // Each item gets a colored placeholder panel as its ImageTemplate
        // since we have no bundled photo assets — this demonstrates the image slot robustly
        (string title, string subtitle, string timestamp, Color color, string initial)[] data =
        [
            ("Mountain Sunrise",  "Landscape · Nature",   "2024-03-15 08:30", Color.FromRgb(0x4C, 0xAF, 0x50), "M"),
            ("City at Night",     "Urban · Architecture", "2024-02-20 22:10", Color.FromRgb(0x21, 0x96, 0xF3), "C"),
            ("Forest Path",       "Nature · Hiking",      "2024-01-05 14:45", Color.FromRgb(0x00, 0x96, 0x88), "F"),
            ("Ocean Waves",       "Seascape · Travel",    "2023-12-10 16:00", Color.FromRgb(0x03, 0xA9, 0xF4), "O"),
            ("Desert Dunes",      "Landscape · Arid",     "2023-11-22 11:20", Color.FromRgb(0xFF, 0x98, 0x00), "D"),
        ];

        var items = new ObservableCollection<SelectionListItem>();
        foreach (var (title, subtitle, timestamp, color, initial) in data)
        {
            var item = new SelectionListItem
            {
                Title     = title,
                Subtitle  = subtitle,
                Timestamp = timestamp,
            };

            // Build a colored placeholder as the ImageTemplate content
            var brush = new SolidColorBrush(color);
            item.ImageTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object>((_, _) =>
                new Border
                {
                    Background = brush,
                    Child = new TextBlock
                    {
                        Text                = initial,
                        FontSize            = 28,
                        FontWeight          = Avalonia.Media.FontWeight.Bold,
                        Foreground          = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center,
                    }
                });

            items.Add(item);
        }
        return items;
    }

    private static ObservableCollection<SelectionListItem> CreateChipItems() =>
    [
        new() { Title = "Nature" },
        new() { Title = "Urban" },
        new() { Title = "Travel" },
        new() { Title = "Architecture" },
        new() { Title = "Portraits" },
        new() { Title = "Abstract" },
    ];
}
