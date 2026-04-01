using Avalonia.Controls;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ComboBoxPageView : UserControl
{
    public ComboBoxPageView()
    {
        InitializeComponent();
        this.FindControl<AutoCompleteBox>("FruitAutoComplete")!.ItemsSource =
            new[] { "Apple", "Banana", "Cherry", "Date", "Elderberry" };
    }
}