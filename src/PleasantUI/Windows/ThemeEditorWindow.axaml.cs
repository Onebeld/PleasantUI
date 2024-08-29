using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PleasantUI.Controls;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Models;
using PleasantUI.Core.Structures;
using PleasantUI.Extensions;
using PleasantUI.Windows.ViewModels;

namespace PleasantUI.Windows;

public partial class ThemeEditorWindow : ContentDialog
{
	public ThemeEditorWindow()
	{
		InitializeComponent();
		
		ButtonJson.Click += (_, _) => { Carousel.Next(); };
		ButtonColors.Click += (_, _) => { Carousel.Previous(); };

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			SetupDragAndDrop();
		else DragAndDropPanel.IsVisible = false;
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

	public static Task<CustomTheme?> EditTheme(IPleasantWindow parent, CustomTheme? customTheme)
	{
		bool cancel = true;
		ThemeEditorWindow window = new();
		ThemeEditorViewModel viewModel = new(parent, window, customTheme);

		window.DataContext = viewModel;
		
		window.CancelButton.Click += (_, _) => { window.Close(); };
		window.OkButton.Click += (_, _) =>
		{
			if (string.IsNullOrWhiteSpace(viewModel.ThemeName))
				return;
			
			cancel = false;
			window.Close();
		};
		
		TaskCompletionSource<CustomTheme?> taskCompletionSource = new();

		window.Closed += (_, _) =>
		{
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
		window.Show(parent);

		return taskCompletionSource.Task;
	}
}