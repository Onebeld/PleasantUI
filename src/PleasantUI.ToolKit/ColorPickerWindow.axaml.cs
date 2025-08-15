using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.ToolKit;

/// <summary>
/// Represents a window that can be used to pick a color.
/// </summary>
/// <seealso cref="PleasantUI.Controls.ContentDialog" />
public sealed partial class ColorPickerWindow : ContentDialog
{
    private bool _cancel = true;
    
    private ColorPickerWindow() => InitializeComponent();

    /// <summary>
    /// Opens a color picker window for the user to select a color.
    /// </summary>
    /// <param name="parent">The parent window to attach the color picker to.</param>
    /// <param name="defaultColor">The optional default color to pre-select in the color picker.</param>
    /// <returns>
    /// A <see cref="Task" /> that represents the asynchronous operation. The task result is
    /// the selected color if the user confirms the selection, or <c>null</c> if the user cancels.
    /// </returns>
    public static Task<Color?> SelectColor(IPleasantWindow parent, uint? defaultColor = null)
    {
        TaskCompletionSource<Color?> taskCompletionSource = new();
        
        ColorPickerWindow window = new()
        {
            ColorView =
            {
                Color = Color.FromUInt32(defaultColor ?? 0xFFFFFFFF)
            }
        };
        
        void CancelButtonOnClick(object? sender, RoutedEventArgs e)
        {
            _ = window.CloseAsync();
        }

        void OkButtonOnClick(object? sender, RoutedEventArgs e)
        {
            window._cancel = false;
            _ = window.CloseAsync();
        }
        
        void WindowOnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            window._cancel = false;
            _ = window.CloseAsync();
        }

        void WindowOnClosed(object? sender, EventArgs e)
        {
            taskCompletionSource.TrySetResult(!window._cancel ? window.ColorView.Color : null);

            window.CancelButton.Click -= CancelButtonOnClick;
            window.OkButton.Click -= OkButtonOnClick;
            window.KeyDown -= WindowOnKeyDown;
            window.Closed -= WindowOnClosed;
        }
        
        window.CancelButton.Click += CancelButtonOnClick;
        window.OkButton.Click += OkButtonOnClick;
        window.KeyDown += WindowOnKeyDown;
        window.Closed += WindowOnClosed;
        
        window.ShowAsync(parent);

        return taskCompletionSource.Task;
    }
}