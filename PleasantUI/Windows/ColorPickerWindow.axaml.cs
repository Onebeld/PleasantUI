using Avalonia.Input;
using Avalonia.Media;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;

namespace PleasantUI.Windows;

public partial class ColorPickerWindow : ContentDialog
{
    public ColorPickerWindow() => InitializeComponent();

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