using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class ItemListPanelPageView : LocalizedUserControl
{
    private static readonly string[] SampleItems =
    [
        "Report_Q1_2025.docx", "Budget_2025.xlsx", "Presentation.pptx",
        "Logo_Final.svg", "Screenshot_001.png", "Archive_backup.zip",
        "Setup_v2.exe", "README.md", "config.json", "database.db"
    ];

    private readonly ObservableCollection<string> _items = new(SampleItems);
    private bool _loading;

    public ItemListPanelPageView()
    {
        InitializeComponent();
        WireHandlers();
        ListPanel.ItemsSource = _items;
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
        ListPanel.ItemsSource = _items;
    }

    private void WireHandlers()
    {
        MultiSelectToggle.IsCheckedChanged -= OnMultiSelectChanged;
        LoadingToggleBtn.Click             -= OnLoadingToggle;
        ClearBtn.Click                     -= OnClear;
        ReloadBtn.Click                    -= OnReload;
        ListPanel.SelectionChanged         -= OnSelectionChanged;

        MultiSelectToggle.IsCheckedChanged += OnMultiSelectChanged;
        LoadingToggleBtn.Click             += OnLoadingToggle;
        ClearBtn.Click                     += OnClear;
        ReloadBtn.Click                    += OnReload;
        ListPanel.SelectionChanged         += OnSelectionChanged;
    }

    private void OnMultiSelectChanged(object? s, RoutedEventArgs e)
        => ListPanel.IsMultiSelectMode = MultiSelectToggle.IsChecked == true;

    private void OnLoadingToggle(object? s, RoutedEventArgs e)
        => ListPanel.IsLoading = _loading = !_loading;

    private void OnClear(object? s, RoutedEventArgs e)  => _items.Clear();

    private void OnReload(object? s, RoutedEventArgs e)
    {
        _items.Clear();
        foreach (var item in SampleItems) _items.Add(item);
    }

    private void OnSelectionChanged(object? s, SelectionChangedEventArgs e)
    {
        if (ListPanel.IsMultiSelectMode)
        {
            int count = ListPanel.SelectedCount;
            SelectedLabel.Text       = count > 0 ? $"{count} item(s)" : "—";
            SelectionCountLabel.Text = $"{count} selected";
        }
        else
        {
            SelectedLabel.Text = ListPanel.SelectedItem?.ToString() ?? "—";
        }
    }
}
