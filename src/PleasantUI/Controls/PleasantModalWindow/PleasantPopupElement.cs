using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;

namespace PleasantUI.Controls;

/// <summary>
/// Represents a modal window that can be used to display content to the user.
/// </summary>
public abstract class PleasantPopupElement : ContentControl
{
    protected ModalWindowHost? Host;
    protected TopLevel? Parent;

    protected virtual void ShowCoreForTopLevel(TopLevel? topLevel)
    {
        OverlayLayer? overlayLayer;

        if (topLevel is not null)
        {
            overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
        }
        else
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                IReadOnlyList<Window> windows = desktopLifetime.Windows;

                for (int i = 0; i < windows.Count; i++)
                {
                    if (!windows[i].IsActive) continue;
                    
                    topLevel = windows[i];
                    break;
                }

                topLevel ??= desktopLifetime.MainWindow ?? throw new NotSupportedException("No TopLevel root found to parent ContentDialog");
                
                overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
            }
            else if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                topLevel = TopLevel.GetTopLevel(singleViewLifetime.MainView);
                overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
            }
            else
            {
                throw new InvalidOperationException(
                    "No TopLevel found for ContentDialog and no ApplicationLifetime is set. " +
                    "Please either supply a valid ApplicationLifetime or TopLevel to ShowAsync()");
            }
        }

        Parent = topLevel;

        if (overlayLayer is null)
            throw new InvalidOperationException("Unable to find OverlayLayer from given TopLevel");
        
        overlayLayer.Children.Add(Host);
    }

    protected virtual void DeleteCoreForTopLevel()
    {
        OverlayLayer overlayLayer = OverlayLayer.GetOverlayLayer(Host);

        if (overlayLayer is null)
            return;
        
        overlayLayer.Children.Remove(Host);

        Host.Content = null;
    }
}