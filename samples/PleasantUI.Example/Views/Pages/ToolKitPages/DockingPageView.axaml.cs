using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;

namespace PleasantUI.Example.Views.Pages.ToolkitPages;

public partial class DockingPageView : LocalizedUserControl
{
    private ToolItem? _explorerItem;
    private ToolItem? _propertiesItem;

    public DockingPageView()
    {
        InitializeComponent();
        Setup();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        Setup();
    }

    private void Setup()
    {
        _explorerItem   = MakeItem("Explorer",       "FolderRegular",      true);
        _propertiesItem = MakeItem("Properties",     "SettingsRegular",    true);

        _explorerItem.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolItem.IsVisible))
                ExplorerPanel.IsVisible = _explorerItem.IsVisible;
        };
        _propertiesItem.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolItem.IsVisible))
                PropertiesPanel.IsVisible = _propertiesItem.IsVisible;
        };

        LeftBar.UpperTopToolsSource = new AvaloniaList<ToolItem>
        {
            _explorerItem,
            MakeItem("Search",         "SearchRegular"),
            MakeItem("Source Control", "BranchRegular"),
        };
        LeftBar.LowerBottomToolsSource = new AvaloniaList<ToolItem>
        {
            MakeItem("Extensions", "PuzzlePieceRegular"),
        };

        RightBar.UpperTopToolsSource = new AvaloniaList<ToolItem>
        {
            _propertiesItem,
            MakeItem("Outline", "ListRegular"),
        };
    }

    private static ToolItem MakeItem(string label, string iconKey, bool isVisible = false)
    {
        // Resolve the Geometry from app resources at construction time
        Geometry? geo = null;
        if (Application.Current?.TryGetResource(iconKey, null, out var res) == true && res is Geometry g)
            geo = g;

        return new ToolItem(label, geo, isVisible);
    }
}

/// <summary>View-model for a sidebar tool button.</summary>
public sealed class ToolItem : INotifyPropertyChanged
{
    private bool _isVisible;

    public string Label { get; }
    public Geometry? IconData { get; }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value) return;
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    public ToolItem(string label, Geometry? iconData, bool isVisible = false)
    {
        Label    = label;
        IconData = iconData;
        _isVisible = isVisible;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
