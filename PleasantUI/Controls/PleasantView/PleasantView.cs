using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

public class PleasantView : ContentControl, IPleasantWindow
{
    private Panel _modalWindowsPanel = null!;
    
    public AvaloniaList<PleasantModalWindow> OpenedModalWindows { get; } = new();

    public VisualLayerManager VisualLayerManager { get; private set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindowsPanel = e.NameScope.Get<Panel>("PART_ModalWindowsPanel");
        VisualLayerManager = e.NameScope.Get<VisualLayerManager>("PART_VisualLayerManager");
    }

    public void AddModalWindow(PleasantModalWindow modalWindow)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(modalWindow.ModalBackground);
        windowPanel.Children.Add(modalWindow);
        
        OpenedModalWindows.Add(modalWindow);
        
        _modalWindowsPanel.Children.Add(windowPanel);
    }

    public void RemoveModalWindow(PleasantModalWindow modalWindow)
    {
        OpenedModalWindows.Remove(modalWindow);
        _modalWindowsPanel.Children.Remove(modalWindow.Parent as Panel ?? throw new NullReferenceException());
    }
}