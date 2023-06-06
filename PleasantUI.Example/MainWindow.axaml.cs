using Avalonia.Controls;
using PleasantUI.Controls;
using PleasantUI.Core;

namespace PleasantUI.Example;

public partial class MainWindow : PleasantWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e) => PleasantSettings.Instance.Save();
}