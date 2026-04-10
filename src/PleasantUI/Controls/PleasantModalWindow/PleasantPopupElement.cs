using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using PleasantUI.Core.Settings.Providers.Interfaces;

namespace PleasantUI.Controls;

/// <summary>
/// Represents an abstract base class for a modal popup element in an Avalonia application.
/// </summary>
public abstract class PleasantPopupElement : ContentControl
{
    /// <summary>
    /// The host that contains the modal window.
    /// </summary>
    protected ModalWindowHost? Host;

    /// <summary>
    /// The top-level parent control where the popup is displayed.
    /// </summary>
    protected TopLevel? TopLevel;

    static PleasantPopupElement()
    {
        CornerRadiusProperty.Changed.AddClassHandler<PleasantPopupElement>((x, e) => x.OnCornerRadiusChanged(e));
    }

    private void OnCornerRadiusChanged(AvaloniaPropertyChangedEventArgs e)
    {
        try
        {
            // Apply VGUI corner radius override if VGUI theme is active
            if (IsVGUIActive())
            {
                System.Diagnostics.Debug.WriteLine("[PleasantPopupElement] VGUI theme active, setting CornerRadius to 0");
                SetValue(CornerRadiusProperty, new CornerRadius(0));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PleasantPopupElement] Error in OnCornerRadiusChanged: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the VGUI theme is currently active.
    /// </summary>
    /// <returns>True if VGUI theme is active, false otherwise.</returns>
    protected static bool IsVGUIActive()
    {
        try
        {
            var isActive = PleasantUI.Core.PleasantSettings.Current?.Theme == "VGUI";
            System.Diagnostics.Debug.WriteLine($"[PleasantPopupElement] IsVGUIActive: {isActive}");
            return isActive;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PleasantPopupElement] Error checking VGUI status: {ex.Message}");
            return false;
        }
    }

    public override void ApplyTemplate()
    {
        base.ApplyTemplate();
        
        try
        {
            // Apply VGUI corner radius override when template is applied
            if (IsVGUIActive())
            {
                System.Diagnostics.Debug.WriteLine("[PleasantPopupElement] ApplyTemplate: VGUI theme active, setting CornerRadius to 0");
                CornerRadius = new CornerRadius(0);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PleasantPopupElement] Error in ApplyTemplate: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays the popup in the specified <see cref="TopLevel"/> container.
    /// </summary>
    /// <param name="topLevel">The <see cref="TopLevel"/> instance where the popup should be shown.</param>
    /// <exception cref="NotSupportedException">Thrown if no valid <see cref="TopLevel"/> is found.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the overlay layer cannot be found.</exception>
    protected virtual void ShowCoreForTopLevel(TopLevel? topLevel)
    {
        OverlayLayer? overlayLayer;

        if (topLevel is not null)
        {
            overlayLayer = OverlayLayer.GetOverlayLayer(topLevel);
        }
        else
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
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
            else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                topLevel = TopLevel.GetTopLevel(singleViewLifetime.MainView);
                overlayLayer = OverlayLayer.GetOverlayLayer(topLevel ?? throw new InvalidOperationException("TopLevel is null"));
            }
            else
            {
                throw new InvalidOperationException(
                    "No TopLevel found for ContentDialog and no ApplicationLifetime is set. " +
                    "Please either supply a valid ApplicationLifetime or TopLevel to ShowAsync()");
            }
        }

        TopLevel = topLevel;

        if (overlayLayer is null)
            throw new InvalidOperationException("Unable to find OverlayLayer from given TopLevel");

        overlayLayer.Children.Add(Host ?? throw new InvalidOperationException("Host is null"));
    }

    /// <summary>
    /// Removes the popup from its parent <see cref="TopLevel"/>.
    /// </summary>
    protected virtual void DeleteCoreForTopLevel()
    {

        OverlayLayer? overlayLayer = OverlayLayer.GetOverlayLayer(
            Host ?? throw new InvalidOperationException("Host is null")
        ) ?? throw new InvalidOperationException("Unable to find OverlayLayer from given Host.");


        if (overlayLayer is null)
            return;

        overlayLayer.Children.Remove(Host);

        Host.Content = null;
    }
}