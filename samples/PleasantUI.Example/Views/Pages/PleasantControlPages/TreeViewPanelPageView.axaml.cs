using Avalonia.Controls;
using PleasantUI.Controls;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class TreeViewPanelPageView : LocalizedUserControl
{
    public TreeViewPanelPageView()
    {
        InitializeComponent();
        WireHandlers();
        PopulateSections();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
        PopulateSections();
    }

    private void WireHandlers()
    {
        Tree.SelectionChanged -= OnSelectionChanged;
        Tree.SelectionChanged += OnSelectionChanged;
    }

    private void PopulateSections()
    {
        if (Tree.Sections.Count < 3) return;

        Tree.Sections[0].ItemsSource = new[] { "Report Q1.docx", "Report Q2.docx", "Budget 2025.xlsx", "Presentation.pptx" };
        Tree.Sections[1].ItemsSource = new[] { "Screenshot.png", "Logo.svg", "Banner.jpg" };
        Tree.Sections[2].ItemsSource = new[] { "Setup.exe", "Archive.zip", "Installer.msi" };
    }

    private void OnSelectionChanged(object? s, SelectionChangedEventArgs e)
        => SelectedLabel.Text = Tree.SelectedItem?.ToString() ?? "—";
}
