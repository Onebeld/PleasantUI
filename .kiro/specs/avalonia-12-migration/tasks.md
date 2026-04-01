# Avalonia 12 Migration — Task List

## Tasks

- [x] 1 Section 1 — Package References & Target Frameworks
  - [x] 1.1 Bump Avalonia and Avalonia.Skia to 12.x in src/PleasantUI/PleasantUI.csproj
  - [x] 1.2 Bump Avalonia, Avalonia.Controls.ColorPicker, Avalonia.Skia to 12.x in src/PleasantUI.ToolKit/PleasantUI.ToolKit.csproj
  - [x] 1.3 Bump Avalonia.Controls.DataGrid to 12.x in src/PleasantUI.DataGrid/PleasantUI.DataGrid.csproj
  - [x] 1.4 Bump Avalonia to 12.x and change TargetFramework from netstandard2.0 to net8.0 in src/PleasantUI.MaterialIcons/PleasantUI.MaterialIcons.csproj
  - [x] 1.5 Bump Avalonia and Avalonia.Desktop to 12.x and replace Avalonia.Diagnostics with AvaloniaUI.DiagnosticsSupport 2.2.0-beta3 in samples/PleasantUI.Example.Desktop/PleasantUI.Example.Desktop.csproj
  - [x] 1.6 Bump Avalonia.Browser to 12.x in samples/PleasantUI.Example.Browser/PleasantUI.Example.Browser.csproj
  - [x] 1.7 Bump Avalonia and Avalonia.Desktop to 12.x and replace Avalonia.Diagnostics with AvaloniaUI.DiagnosticsSupport 2.2.0-beta3 in designer/PleasantUI.Designer/PleasantUI.Designer.csproj
  - [x] 1.8 Bump Avalonia.Android to 12.x in samples/PleasantUI.Example.Android/PleasantUI.Example.Android.csproj

- [x] 2 Section 2 — Window Decoration & Chrome API
  - [x] 2.1 Remove ExtendClientAreaChromeHints and ExtendClientAreaTitleBarHeightHint assignments and replace SystemDecorations with WindowDecorations in PleasantWindow.ChangeDecorations
  - [x] 2.2 Replace ExtendClientAreaToDecorationsHint in PleasantWindow.OnPropertyChanged and ChangeDecorations with the Avalonia 12 equivalent
  - [x] 2.3 Remove ExtendClientAreaTitleBarHeightHint observer and replace ExtendClientAreaToDecorationsHint in PleasantMiniWindow.OnApplyTemplate

- [x] 3 Section 3 — Clipboard API
  - [x] 3.1 Replace GetTextAsync() with TryGetTextAsync() in ThemeService.PasteColorAsync
  - [x] 3.2 Replace GetTextAsync() with TryGetTextAsync() in ThemeService.PasteThemeAsync

- [x] 4 Section 4 — Drag-and-Drop API
  - [x] 4.1 Replace DataFormats.Files with DataFormat.Files, e.Data with e.DataTransfer, and GetFiles() with await GetFilesAsync() in ThemeEditorWindow DragEnter handler; make Drop handler async

- [x] 5 Section 5 — Gesture Events
  - [x] 5.1 Replace Gestures.ScrollGestureEvent with InputElement.ScrollGestureEvent in SmoothScrollContentPresenter constructor

- [-] 6 Section 6 — Renamed & Removed Members
  - [x] 6.1 Rename Watermark to PlaceholderText, PART_Watermark to PART_Placeholder, and PART_FloatingWatermark to PART_FloatingPlaceholder in all 8 affected XAML files
  - [x] 6.2 Rename UseFloatingWatermark to UseFloatingPlaceholder in TextBox.axaml and CalendarDatePicker.axaml
  - [x] 6.3 Update style selector from [UseFloatingWatermark=true]/PART_FloatingWatermark to [UseFloatingPlaceholder=true]/PART_FloatingPlaceholder in TextBox.axaml
  - [x] 6.4 Replace VisualRoot property access with TopLevel.GetTopLevel(this) in PleasantTitleBar.cs, ShadowBorder.cs, NavigationView.cs, and ModalWindowHost.cs
  - [-] 6.5 Remove using Avalonia.Controls.Generators and replace all ItemContainerGenerator usages with Avalonia 12 VirtualizingPanel container APIs in VirtualizingWrapPanel.cs
  - [-] 6.6 Replace this.GetVisualRoot() is ILayoutRoot with TopLevel.GetTopLevel(this) is not null in VirtualizingWrapPanel.ScrollIntoView

- [ ] 7 Section 7 — Android App Initialization
  - [ ] 7.1 Change MainActivity to extend AvaloniaMainActivity (non-generic), remove CustomizeAppBuilder override, and add AndroidApp : AvaloniaAndroidApplication<App> class in MainActivity.cs
  - [ ] 7.2 Add IActivityApplicationLifetime check before ISingleViewApplicationLifetime in App.axaml.cs

- [ ] 8 Verify build
  - [ ] 8.1 Confirm the solution builds with zero compile errors against Avalonia 12 packages
