using Avalonia.Controls;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ComboBoxPageView : LocalizedUserControl
{
    public ComboBoxPageView()
    {
        InitializeComponent();
        SetupAutoCompleteItems();
    }
    // Complex constructor — don't re-run InitializeComponent

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        SetupAutoCompleteItems();
    }

    private void SetupAutoCompleteItems()
    {
        this.FindControl<AutoCompleteBox>("FruitAutoComplete")!.ItemsSource =
            new[] { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
    }
}
