using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Enums;

namespace PleasantUI.Example;

public partial class MainWindow : PleasantWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
        Closing += OnClosing;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (OperatingSystem.IsMacOS())
        {
            EnableTitleBarMargin = true;
            TitleBarType = PleasantTitleBarType.Classic;
        }
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e) => PleasantSettings.Instance.Save();
}