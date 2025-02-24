using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Core.Helpers;

/// <summary>
/// Provides helper methods for managing application-level functionalities, such as retrieving
/// the Snackbar queue manager and modal windows for a given <see cref="TopLevel"/>.
/// </summary>
public static class ApplicationHelper
{
    /// <summary>
    /// Gets the Snackbar queue manager from the given <see cref="TopLevel"/>.
    /// </summary>
    /// <param name="topLevel">The window to get the queue manager from.</param>
    /// <returns>The Snackbar queue manager.</returns>
    public static SnackbarQueueManager<PleasantSnackbar>? GetSnackbarQueueManager(TopLevel? topLevel) => GetPleasantWindow(topLevel)?.SnackbarQueueManager;

    /// <summary>
    /// Retrieves the list of modal windows for the given <see cref="TopLevel"/>.
    /// </summary>
    /// <param name="topLevel">The top-level window to get modal windows from. If null, the current active window is used.</param>
    /// <returns>A list of <see cref="PleasantPopupElement"/> representing the modal windows, or null if no modal windows are available.</returns>
    public static AvaloniaList<PleasantPopupElement>? GetModalWindows(TopLevel? topLevel) => GetPleasantWindow(topLevel)?.ModalWindows;

    private static IPleasantWindow? GetPleasantWindow(TopLevel? topLevel)
    {
        if (topLevel is not null)
            return topLevel as IPleasantWindow;
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            IReadOnlyList<Window> windows = desktopLifetime.Windows;

            for (int i = 0; i < windows.Count; i++)
            {
                if (!windows[i].IsActive) continue;
                    
                topLevel = windows[i];
                break;
            }

            topLevel ??= desktopLifetime.MainWindow;

            return topLevel as IPleasantWindow;
        }

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            return singleViewLifetime.MainView as IPleasantWindow;
        }

        return null;
    }
}