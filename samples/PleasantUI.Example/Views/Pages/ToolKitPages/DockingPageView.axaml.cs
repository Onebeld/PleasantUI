using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.ToolkitPages;

public partial class DockingPageView : LocalizedUserControl
{
    private ToolItem? _explorerItem;
    private ToolItem? _propertiesItem;

    // Keep the lists alive across reinits so drag-moved items are not lost
    private AvaloniaList<ToolItem>? _leftUpperTop;
    private AvaloniaList<ToolItem>? _leftLowerBottom;
    private AvaloniaList<ToolItem>? _rightUpperTop;
    private AvaloniaList<ToolItem>? _rightLowerBottom;

    public DockingPageView()
    {
        InitializeComponent();
        Setup();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        // Re-apply the existing lists to the new template parts — do NOT recreate them
        // so any items moved between sidebars via drag-drop are preserved.
        ApplyLists();
        // Re-register content with the new Host instance produced by InitializeComponent.
        RegisterContent();
    }

    private void Setup()
    {
        _explorerItem   = MakeItem("Explorer",       "FolderRegular",      true);
        _propertiesItem = MakeItem("Properties",     "SettingsRegular",    true);

        _leftUpperTop = new AvaloniaList<ToolItem>
        {
            _explorerItem,
            MakeItem("Search",         "SearchRegular"),
            MakeItem("Source Control", "BranchRegular"),
        };
        _leftLowerBottom = new AvaloniaList<ToolItem>
        {
            MakeItem("Extensions", "PuzzlePieceRegular"),
        };
        _rightUpperTop = new AvaloniaList<ToolItem>
        {
            _propertiesItem,
            MakeItem("Outline", "ListRegular"),
        };
        _rightLowerBottom = new AvaloniaList<ToolItem>();

        ApplyLists();
        RegisterContent();
    }

    private void RegisterContent()
    {
        if (_explorerItem != null)
            Host.RegisterButtonContent(_explorerItem, CreateExplorerContent());
        if (_propertiesItem != null)
            Host.RegisterButtonContent(_propertiesItem, CreatePropertiesContent());
    }

    private void ApplyLists()
    {
        LeftBar.UpperTopToolsSource    = _leftUpperTop;
        LeftBar.LowerBottomToolsSource = _leftLowerBottom;
        RightBar.UpperTopToolsSource   = _rightUpperTop;
        RightBar.LowerBottomToolsSource = _rightLowerBottom;
    }

    private Control CreateExplorerContent()
    {
        return new Border
        {
            Background   = new SolidColorBrush(Colors.Transparent),
            BorderBrush  = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(0, 0, 1, 0),
            MinWidth     = 140,
            Child = new StackPanel
            {
                Margin  = new Thickness(10, 8),
                Spacing = 2,
                Children =
                {
                    new TextBlock
                    {
                        Text         = Localizer.Tr("Docking/ExplorerPanel"),
                        FontSize     = 10,
                        FontWeight   = FontWeight.SemiBold,
                        LetterSpacing = 1,
                        Margin       = new Thickness(0, 0, 0, 6)
                    },
                    new TextBlock { Text = "src/",          FontSize = 13 },
                    new TextBlock { Text = "Controls/",     FontSize = 13, Margin = new Thickness(12, 0, 0, 0) },
                    new TextBlock { Text = "Core/",         FontSize = 13, Margin = new Thickness(12, 0, 0, 0) },
                    new TextBlock { Text = "Styling/",      FontSize = 13, Margin = new Thickness(12, 0, 0, 0) },
                    new TextBlock { Text = "README.md",     FontSize = 13 },
                    new TextBlock { Text = "PleasantUI.sln",FontSize = 13 }
                }
            }
        };
    }

    private Control CreatePropertiesContent()
    {
        return new Border
        {
            Background      = new SolidColorBrush(Colors.Transparent),
            BorderBrush     = new SolidColorBrush(Colors.Transparent),
            BorderThickness = new Thickness(1, 0, 0, 0),
            MinWidth        = 160,
            Child = new StackPanel
            {
                Margin  = new Thickness(10, 8),
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text          = Localizer.Tr("Docking/PropertiesPanel"),
                        FontSize      = 10,
                        FontWeight    = FontWeight.SemiBold,
                        LetterSpacing = 1,
                        Margin        = new Thickness(0, 0, 0, 2)
                    },
                    new StackPanel { Spacing = 2, Children =
                    {
                        new TextBlock { Text = Localizer.Tr("Docking/PropName"),   FontSize = 11 },
                        new TextBlock { Text = "MyControl", FontWeight = FontWeight.SemiBold }
                    }},
                    new StackPanel { Spacing = 2, Children =
                    {
                        new TextBlock { Text = Localizer.Tr("Docking/PropWidth"),  FontSize = 11 },
                        new TextBlock { Text = "320", FontWeight = FontWeight.SemiBold }
                    }},
                    new StackPanel { Spacing = 2, Children =
                    {
                        new TextBlock { Text = Localizer.Tr("Docking/PropHeight"), FontSize = 11 },
                        new TextBlock { Text = "240", FontWeight = FontWeight.SemiBold }
                    }}
                }
            }
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
