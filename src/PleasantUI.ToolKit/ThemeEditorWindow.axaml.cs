using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PleasantUI.Controls;
using PleasantUI.Core.Extensions;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Models;
using PleasantUI.ToolKit.ViewModels;

namespace PleasantUI.ToolKit;

/// <summary>
/// Represents a dialog that allows the user to edit a theme.
/// </summary>
/// <seealso cref="PleasantUI.Controls.ContentDialog" />
public sealed partial class ThemeEditorWindow : ContentDialog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeEditorWindow"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is only intended to be used by Avalonia's XAML parser.
    /// </remarks>
    private ThemeEditorWindow()
    {
        InitializeComponent();

        ButtonJson.Click += (_, _) => { Carousel.Next(); };
        ButtonColors.Click += (_, _) => { Carousel.Previous(); };

        ThemeNameTextBox.LostFocus += ThemeNameTextBoxOnLostFocus;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            SetupDragAndDrop();
        else DragAndDropPanel.IsVisible = false;
    }
    
    /// <summary>
    /// Opens a theme editor window for the user to edit a theme.
    /// </summary>
    /// <param name="parent">The parent window to attach the theme editor window to.</param>
    /// <param name="customTheme">The optional custom theme to edit.</param>
    /// <returns>
    /// A <see cref="Task" /> that represents the asynchronous operation. The task result is
    /// the edited theme if the user confirms the changes, or <c>null</c> if the user cancels.
    /// </returns>
    public static Task<CustomTheme?> EditTheme(IPleasantWindow parent, CustomTheme? customTheme)
    {
        bool cancel = true;
        ThemeEditorWindow window = new();
        ThemeEditorViewModel viewModel = new(parent, window, customTheme);

        window.DataContext = viewModel;

        window.CancelButton.Click += (_, _) => { _ = window.CloseAsync(); };
        window.OkButton.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(viewModel.ThemeName))
                return;

            cancel = false;
            _ = window.CloseAsync();
        };

        TaskCompletionSource<CustomTheme?> taskCompletionSource = new();

        window.Closed += (_, _) =>
        {
            viewModel.Dispose();
            
            if (cancel)
            {
                taskCompletionSource.TrySetResult(null);
            }
            else
            {
                viewModel.CustomTheme.Name = viewModel.ThemeName;
                viewModel.CustomTheme.Colors = viewModel.ResourceDictionary.ToDictionary<string, Color>();

                taskCompletionSource.TrySetResult(viewModel.CustomTheme);
            }

            taskCompletionSource.TrySetResult(null);
        };
        window.ShowAsync(parent);

        return taskCompletionSource.Task;
    }

    private void ThemeNameTextBoxOnLostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ThemeNameTextBox.Text))
            ThemeNameTextBox.Text = ((ThemeEditorViewModel)DataContext!).ThemeName;
    }

    private void SetupDragAndDrop()
    {
        void DragLeave(object? s, DragEventArgs e)
        {
            DragAndDropPanel.Classes.Set("DragDrop", false);
        }

        void DragEnter(object? s, DragEventArgs e)
        {
            e.DragEffects = DragDropEffects.None;

            if (!e.Data.Contains(DataFormats.Files))
                return;

            IStorageItem? file = e.Data.GetFiles()?.First();

            if (file is null || !file.Name.EndsWith(".json"))
                return;

            e.DragEffects = DragDropEffects.Link | DragDropEffects.Copy;

            DragAndDropPanel.Classes.Set("DragDrop", true);
        }

        void Drop(object? s, DragEventArgs e)
        {
            if (!e.Data.Contains(DataFormats.Files))
                return;

            IStorageItem? file = e.Data.GetFiles()?.First();

            if (file is not null && file.Name.EndsWith(".json"))
                ((ThemeEditorViewModel)DataContext!).DropFile(file);

            DragAndDropPanel.Classes.Set("DragDrop", false);
        }

        DragAndDropPanel.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        DragAndDropPanel.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        DragAndDropPanel.AddHandler(DragDrop.DropEvent, Drop);
    }
}