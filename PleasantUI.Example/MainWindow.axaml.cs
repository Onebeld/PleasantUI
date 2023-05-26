using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PleasantUI.Controls;
using PleasantUI.Core;

namespace PleasantUI.Example;

public partial class MainWindow : PleasantWindow
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        PleasantSettings.Instance.Save();
    }
}