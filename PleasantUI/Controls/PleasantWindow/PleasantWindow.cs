using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Controls;

public class PleasantWindow : Window, IPleasantWindow, IStyleable
{
    private Panel _modalWindowsPanel = null!;
    
    Type IStyleable.StyleKey => typeof(PleasantWindow);
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _modalWindowsPanel = e.NameScope.Get<Panel>("PART_ModalWindowsPanel");
    }

    public void AddModalWindow(PleasantModalWindow modalWindow, Animation animation)
    {
        Panel windowPanel = new()
        {
            IsHitTestVisible = modalWindow.IsHitTestVisible
        };
        windowPanel.Children.Add(modalWindow.Background);
        windowPanel.Children.Add(modalWindow);
        
        _modalWindowsPanel.Children.Add(windowPanel);
        
    }

    public void RemoveModalWindow(PleasantModalWindow modalWindow)
    {
        _modalWindowsPanel.Children.Remove(modalWindow.Parent as Panel ?? throw new NullReferenceException());
    }
}