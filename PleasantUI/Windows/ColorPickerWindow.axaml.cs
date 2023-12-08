using Avalonia.Input;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Windows;

public partial class ColorPickerWindow : ContentDialog
{
    public ColorPickerWindow() => InitializeComponent();

    /// <summary>
    /// Opens a color picker window for the user to select a color.
    /// </summary>
    /// <param name="parent">The parent window to attach the color picker to.</param>
    /// <param name="defaultColor">The optional default color to pre-select in the color picker.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation. The task result is
    /// the selected color if the user confirms the selection, or <c>null</c> if the user cancels.
    /// </returns>
    public static Task<Color?> SelectColor(IPleasantWindow parent, uint? defaultColor = null)
    {
        bool cancel = true;
        ColorPickerWindow window = new()
        {
            ColorView =
            {
                Color = Color.FromUInt32(defaultColor ?? 0xFFFFFFFF)
            }
        };

        window.CancelButton.Click += (_, _) => { window.Close(); };
        window.OkButton.Click += (_, _) =>
        {
            cancel = false;
            window.Close();
        };
        window.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;

            cancel = false;
            window.Close();
        };

        TaskCompletionSource<Color?> taskCompletionSource = new();

        window.Closed += (_, _) =>
        {
            taskCompletionSource.TrySetResult(!cancel ? window.ColorView.Color : null);
        };
        window.Show(parent);

        return taskCompletionSource.Task;
    }
}