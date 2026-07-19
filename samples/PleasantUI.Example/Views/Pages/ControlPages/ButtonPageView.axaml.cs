using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ButtonPageView : LocalizedUserControl
{
    private int _values = 1;
    
    public ButtonPageView()
    {
        InitializeComponent();
        
        AddButton.Click += AddButtonOnClick;
    }

    private void AddButtonOnClick(object? sender, RoutedEventArgs e)
    {
        ListBox.Items.Add(new ListBoxItem
        {
            Content = "New content " + _values++,
        });
    }

    protected override void ReinitializeComponent() => InitializeComponent();
}
