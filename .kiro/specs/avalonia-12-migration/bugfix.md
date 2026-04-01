# Bugfix Requirements Document

## Introduction

PleasantUI is an Avalonia-based UI controls library currently targeting Avalonia 11.3.3. Avalonia 12
introduces a significant number of breaking API changes that prevent the library from compiling and
running correctly. This document captures the full set of defects caused by those breaking changes,
with exact file paths, exact current code, and exact required replacements for every affected location.

Affected projects:
- `src/PleasantUI` — core library
- `src/PleasantUI.ToolKit` — toolkit/dialogs
- `src/PleasantUI.DataGrid` — DataGrid styling
- `src/PleasantUI.MaterialIcons` — icon library
- `designer/PleasantUI.Designer` — designer host
- `samples/PleasantUI.Example.Desktop` — desktop sample
- `samples/PleasantUI.Example.Browser` — browser sample
- `samples/PleasantUI.Example.Android` — Android sample

---

## Bug Analysis

### Current Behavior (Defect)

---

#### Section 1 — Package References & Target Frameworks

**1.1** WHEN any project references Avalonia packages at version `11.3.3` (or `11.2.0-beta1` for Android)
THEN the system fails to resolve Avalonia 12 APIs, causing build errors across all projects.

Affected files and current `<PackageReference>` versions:

| File | Package | Current Version |
|------|---------|----------------|
| `src/PleasantUI/PleasantUI.csproj` | `Avalonia`, `Avalonia.Skia` | `11.3.3` |
| `src/PleasantUI.ToolKit/PleasantUI.ToolKit.csproj` | `Avalonia`, `Avalonia.Controls.ColorPicker`, `Avalonia.Skia` | `11.3.3` |
| `src/PleasantUI.DataGrid/PleasantUI.DataGrid.csproj` | `Avalonia.Controls.DataGrid` | `11.3.3` |
| `src/PleasantUI.MaterialIcons/PleasantUI.MaterialIcons.csproj` | `Avalonia` | `11.3.3` |
| `samples/PleasantUI.Example.Desktop/PleasantUI.Example.Desktop.csproj` | `Avalonia`, `Avalonia.Desktop` | `11.3.3` |
| `samples/PleasantUI.Example.Browser/PleasantUI.Example.Browser.csproj` | `Avalonia.Browser` | `11.3.3` |
| `designer/PleasantUI.Designer/PleasantUI.Designer.csproj` | `Avalonia`, `Avalonia.Desktop` | `11.3.3` |
| `samples/PleasantUI.Example.Android/PleasantUI.Example.Android.csproj` | `Avalonia.Android` | `11.2.0-beta1` |

**1.2** WHEN `src/PleasantUI.MaterialIcons/PleasantUI.MaterialIcons.csproj` specifies
`<TargetFramework>netstandard2.0</TargetFramework>` THEN the system fails to compile against
Avalonia 12, which requires .NET 8 or higher as the minimum target.

Current code in `src/PleasantUI.MaterialIcons/PleasantUI.MaterialIcons.csproj`:
```xml
<TargetFramework>netstandard2.0</TargetFramework>
```

**1.3** WHEN `samples/PleasantUI.Example.Desktop/PleasantUI.Example.Desktop.csproj` and
`designer/PleasantUI.Designer/PleasantUI.Designer.csproj` reference `Avalonia.Diagnostics`
THEN the system fails to compile because `Avalonia.Diagnostics` has been removed in Avalonia 12.

Current code in both `.csproj` files:
```xml
<PackageReference Condition="'$(Configuration)' == 'Debug'" Include="Avalonia.Diagnostics" Version="11.3.3" />
```

---

#### Section 2 — Window Decoration & Chrome API

**2.1** WHEN `PleasantWindow.ChangeDecorations()` sets `ExtendClientAreaChromeHints` using
`Avalonia.Platform.ExtendClientAreaChromeHints` enum values THEN the system fails to compile
because `ExtendClientAreaChromeHints` has been removed in Avalonia 12.

File: `src/PleasantUI/Controls/PleasantWindow/PleasantWindow.cs`
Method: `ChangeDecorations(bool enableCustomTitleBar, WindowState windowState)`

Current code:
```csharp
if (windowState == WindowState.FullScreen)
{
    ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default;
    ExtendClientAreaTitleBarHeightHint = 0;
    SystemDecorations = SystemDecorations.Full;
}
else
{
    if (OverrideMacOSCaption)
    {
        ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.OSXThickTitleBar;
    }
    else
    {
        ExtendClientAreaTitleBarHeightHint = -1;
        ExtendClientAreaChromeHints = TitleBarType == PleasantTitleBar.Type.Classic
            ? Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome
            : Avalonia.Platform.ExtendClientAreaChromeHints.PreferSystemChrome |
              Avalonia.Platform.ExtendClientAreaChromeHints.OSXThickTitleBar;
        SystemDecorations = SystemDecorations.Full;
    }
}
```

All three broken APIs in this block:
- `ExtendClientAreaChromeHints` — enum and property removed in Avalonia 12
- `ExtendClientAreaTitleBarHeightHint` — property removed in Avalonia 12
- `SystemDecorations = SystemDecorations.Full` — `SystemDecorations` renamed to `WindowDecorations` in Avalonia 12

**2.2** WHEN `PleasantWindow.OnPropertyChanged` sets `ExtendClientAreaToDecorationsHint`
THEN the system fails to compile because this property has been removed in Avalonia 12.

File: `src/PleasantUI/Controls/PleasantWindow/PleasantWindow.cs`
Method: `OnPropertyChanged`

Current code:
```csharp
if (change.Property == EnableCustomTitleBarProperty)
    ExtendClientAreaToDecorationsHint = EnableCustomTitleBar;
```

Also in `ChangeDecorations`:
```csharp
ExtendClientAreaToDecorationsHint = enableCustomTitleBar;
```

**2.3** WHEN `PleasantMiniWindow.OnApplyTemplate` sets `ExtendClientAreaTitleBarHeightHint`
THEN the system fails to compile because this property has been removed in Avalonia 12.

File: `src/PleasantUI/Controls/PleasantMiniWindow/PleasantMiniWindow.cs`
Method: `OnApplyTemplate`

Current code:
```csharp
ExtendClientAreaToDecorationsHint = PleasantSettings.Current?.WindowSettings.EnableCustomTitleBar ?? false;
// ...
this.GetObservable(EnableCustomTitleBarProperty)
    .Subscribe(new AnonymousObserver<bool>(val => { ExtendClientAreaToDecorationsHint = val; }));
this.GetObservable(CanResizeProperty).Subscribe(new AnonymousObserver<bool>(canResize =>
{
    ExtendClientAreaTitleBarHeightHint = canResize ? 8 : 1;
}));
```

The `ExtendClientAreaTitleBarHeightHint` assignment is the broken line. The
`ExtendClientAreaToDecorationsHint` assignments are also broken (property removed in Avalonia 12).

---

#### Section 3 — Clipboard API

**3.1** WHEN `ThemeService.PasteColorAsync()` calls `_topLevel.Clipboard?.GetTextAsync()`
THEN the system fails to compile because `IClipboard.GetTextAsync()` has been replaced by
`TryGetTextAsync()` in Avalonia 12.

File: `src/PleasantUI.ToolKit/Services/ThemeService.cs`
Method: `PasteColorAsync()`

Current code:
```csharp
string? text = await _topLevel.Clipboard?.GetTextAsync();
```

**3.2** WHEN `ThemeService.PasteThemeAsync()` calls `_topLevel.Clipboard?.GetTextAsync()`
THEN the system fails to compile because `IClipboard.GetTextAsync()` has been replaced by
`TryGetTextAsync()` in Avalonia 12.

File: `src/PleasantUI.ToolKit/Services/ThemeService.cs`
Method: `PasteThemeAsync()`

Current code:
```csharp
string? data = await _topLevel.Clipboard?.GetTextAsync();
```

---

#### Section 4 — Drag-and-Drop API

**4.1** WHEN `ThemeEditorWindow.SetupDragAndDrop()` checks `e.Data.Contains(DataFormats.Files)`
THEN the system fails to compile because `DataFormats` (plural) has been renamed to `DataFormat`
(singular) and `DragEventArgs.Data` has been renamed to `DragEventArgs.DataTransfer` in Avalonia 12.

File: `src/PleasantUI.ToolKit/ThemeEditorWindow.axaml.cs`
Method: `SetupDragAndDrop()` — both `DragEnter` and `Drop` local handlers

Current code in `DragEnter` handler:
```csharp
if (!e.Data.Contains(DataFormats.Files))
    return;
IStorageItem? file = e.Data.GetFiles()?.First();
```

Current code in `Drop` handler:
```csharp
if (!e.Data.Contains(DataFormats.Files))
    return;
IStorageItem? file = e.Data.GetFiles()?.First();
```

Three broken APIs in each handler:
- `DataFormats.Files` → `DataFormat.Files` (class renamed, singular)
- `e.Data.Contains(...)` → `e.DataTransfer.Contains(...)` (property renamed)
- `e.Data.GetFiles()` → `await e.DataTransfer.GetFilesAsync()` (renamed + now async)

---

#### Section 5 — Gesture Events

**5.1** WHEN `SmoothScrollContentPresenter` constructor calls
`AddHandler(Gestures.ScrollGestureEvent, OnScrollGesture)` THEN the system fails to compile
because gesture events have been moved from the `Gestures` static class to `InputElement`
in Avalonia 12.

File: `src/PleasantUI/Controls/SmoothScrollViewer/SmoothScrollContentPresenter.cs`
Constructor: `SmoothScrollContentPresenter()`

Current code:
```csharp
AddHandler(Gestures.ScrollGestureEvent, OnScrollGesture);
```

---

#### Section 6 — Renamed & Removed Members

**6.1** WHEN XAML templates use `TextBox.Watermark` attribute THEN the system fails to
compile/render because `TextBox.Watermark` has been renamed to `TextBox.PlaceholderText`
in Avalonia 12.

Files and exact occurrences:

- `src/PleasantUI.ToolKit/ThemeEditorWindow.axaml` line ~46:
  ```xml
  Watermark="{Localize ThemeName, ...}"
  ```

- `src/PleasantUI/Styling/ControlThemes/PleasantControls/MarkedNumericUpDown.axaml` — multiple:
  ```xml
  Watermark="..."
  ```

- `src/PleasantUI/Styling/ControlThemes/PleasantControls/MarkedTextBox.axaml`:
  ```xml
  Watermark="Watermark"
  Watermark="{TemplateBinding Watermark}"
  ```

- `src/PleasantUI/Styling/ControlThemes/BasicControls/TextBox.axaml` — multiple occurrences:
  ```xml
  Watermark="..."
  Text="{TemplateBinding Watermark}"
  ```
  Also the part names `PART_Watermark` and `PART_FloatingWatermark` are renamed.

- `src/PleasantUI/Styling/ControlThemes/BasicControls/NumericUpDown.axaml`:
  ```xml
  Watermark="{TemplateBinding Watermark}"
  ```

- `src/PleasantUI/Styling/ControlThemes/BasicControls/CalendarDatePicker.axaml`:
  ```xml
  Watermark="{TemplateBinding Watermark}"
  ```

- `src/PleasantUI/Styling/ControlThemes/BasicControls/AutoCompleteBox.axaml`:
  ```xml
  Watermark="{TemplateBinding Watermark}"
  ```

- `samples/PleasantUI.Example/Views/Pages/PleasantControlPages/ProgressRingPageView.axaml`:
  ```xml
  Watermark="Value"
  ```

**6.2** WHEN XAML templates use `TextBox.UseFloatingWatermark` THEN the system fails to
compile/render because it has been renamed to `TextBox.UseFloatingPlaceholder` in Avalonia 12.

Files:

- `src/PleasantUI/Styling/ControlThemes/BasicControls/TextBox.axaml`:
  ```xml
  UseFloatingWatermark="{TemplateBinding UseFloatingWatermark}"
  ```

- `src/PleasantUI/Styling/ControlThemes/BasicControls/CalendarDatePicker.axaml`:
  ```xml
  UseFloatingWatermark="{TemplateBinding UseFloatingWatermark}"
  ```

**6.3** WHEN XAML style selectors reference `[UseFloatingWatermark=true]` THEN the system
fails to apply styles because the property has been renamed to `UseFloatingPlaceholder`
in Avalonia 12.

File: `src/PleasantUI/Styling/ControlThemes/BasicControls/TextBox.axaml`

Current selector:
```xml
Style Selector="^[UseFloatingWatermark=true]:not(:empty) /template/ TextBlock#PART_FloatingWatermark"
```

**6.4** WHEN `PleasantTitleBar.OnApplyTemplate` accesses `VisualRoot` as a property
THEN the system fails to compile because `VisualRoot` has been removed from `Visual`
in Avalonia 12.

File: `src/PleasantUI/Controls/Chrome/PleasantTitleBar.cs`
Method: `OnApplyTemplate`

Current code:
```csharp
if (VisualRoot is PleasantWindow window)
{
    _host = window;
    _captionButtons.Host = window;
    ...
}
```

Additional `VisualRoot` usages in other files:

- `src/PleasantUI/Controls/ShadowBorder/ShadowBorder.cs`:
  ```csharp
  if (VisualRoot is { } renderRoot)
      scale = renderRoot.RenderScaling;
  ```

- `src/PleasantUI/Controls/NavigationView/NavigationView.cs`:
  ```csharp
  if (VisualRoot is PleasantWindow window)
  {
      titleBarHeight = window.TitleBarHeight;
  ```

- `src/PleasantUI/Controls/ModalWindowHost/ModalWindowHost.cs`:
  ```csharp
  return VisualRoot switch
  {
      TopLevel topLevel => topLevel.ClientSize,
      ...
  ```

**6.5** WHEN `VirtualizingWrapPanel` uses `ItemContainerGenerator` directly THEN the system
fails to compile because `ItemContainerGenerator` and the `Avalonia.Controls.Generators`
namespace have been removed in Avalonia 12.

File: `src/PleasantUI/Controls/VirtualizingWrapPanel/VirtualizingWrapPanel.cs`

Current broken usages:
```csharp
using Avalonia.Controls.Generators;  // namespace removed

// In GetRecycledElement():
ItemContainerGenerator generator = ItemContainerGenerator!;
generator.PrepareItemContainer(...);
generator.ItemContainerPrepared(...);
generator.ClearItemContainer(...);

// In CreateElement():
generator.NeedsContainer(...);
generator.CreateContainer(...);
AddInternalChild(...);

// In UpdateElementIndex():
ItemContainerGenerator.ItemContainerIndexChanged(...);

// In RecycleElement() and RecycleElementOnItemRemoved():
ItemContainerGenerator!.ClearItemContainer(...);
```

**6.6** WHEN `VirtualizingWrapPanel.ScrollIntoView` checks `this.GetVisualRoot() is ILayoutRoot`
THEN the system fails to compile because `ILayoutRoot` interface has been removed in Avalonia 12.

File: `src/PleasantUI/Controls/VirtualizingWrapPanel/VirtualizingWrapPanel.cs`
Method: `ScrollIntoView(int index)`

Current code:
```csharp
if (this.GetVisualRoot() is ILayoutRoot)
```

---

#### Section 7 — Android App Initialization

**7.1** WHEN `MainActivity` extends `AvaloniaMainActivity<App>` THEN the system fails to
compile because `AvaloniaMainActivity<TApp>` generic form has been removed in Avalonia 12.
The new pattern splits initialization into `AvaloniaMainActivity` (activity) and
`AvaloniaAndroidApplication<TApp>` (application class).

File: `samples/PleasantUI.Example.Android/MainActivity.cs`

Current code:
```csharp
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder);
    }

    protected override void OnStop()
    {
        PleasantSettings.Save();
        base.OnStop();
    }
}
```

**7.2** WHEN `App.OnFrameworkInitializationCompleted` only checks `ISingleViewApplicationLifetime`
THEN the system fails to initialize correctly on Android in Avalonia 12, which requires
`IActivityApplicationLifetime` to be checked first.

File: `samples/PleasantUI.Example.Android/App.axaml.cs`

Current code:
```csharp
if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
{
    lifetime.MainView = new PleasantMainView { DataContext = ViewModel };
}
```

---

#### Section 8 — Source Generator Target Framework

**8.1** WHEN `PleasantUI.MaterialIcons.SourceGenerator` targets `netstandard2.0` THEN the
system compiles correctly — Roslyn analyzers/source generators MUST target `netstandard2.0`
and this file requires NO change.

File: `generators/PleasantUI.MaterialIcons.SourceGenerator/PleasantUI.MaterialIcons.SourceGenerator.csproj`

Current (correct, no change needed):
```xml
<TargetFramework>netstandard2.0</TargetFramework>
```

---

### Expected Behavior (Correct)

**1.1** WHEN any project references Avalonia packages THEN the system SHALL reference version
`12.x` packages. All `<PackageReference>` entries for `Avalonia`, `Avalonia.Skia`,
`Avalonia.Controls.ColorPicker`, `Avalonia.Controls.DataGrid`, `Avalonia.Desktop`,
`Avalonia.Browser`, and `Avalonia.Android` SHALL be bumped to the matching Avalonia 12 version.

**1.2** WHEN `src/PleasantUI.MaterialIcons/PleasantUI.MaterialIcons.csproj` specifies a target
framework THEN the system SHALL use `<TargetFramework>net8.0</TargetFramework>` (or net9.0 to
match the rest of the solution).

Required change:
```xml
<!-- Before -->
<TargetFramework>netstandard2.0</TargetFramework>
<!-- After -->
<TargetFramework>net8.0</TargetFramework>
```

**1.3** WHEN `samples/PleasantUI.Example.Desktop/PleasantUI.Example.Desktop.csproj` and
`designer/PleasantUI.Designer/PleasantUI.Designer.csproj` include a diagnostics package
THEN the system SHALL reference `AvaloniaUI.DiagnosticsSupport` instead of `Avalonia.Diagnostics`.

Required change in both files:
```xml
<!-- Before -->
<PackageReference Condition="'$(Configuration)' == 'Debug'" Include="Avalonia.Diagnostics" Version="11.3.3" />
<!-- After -->
<PackageReference Condition="'$(Configuration)' == 'Debug'" Include="AvaloniaUI.DiagnosticsSupport" Version="2.2.0-beta3" />
```

Note: No C# code calls `AttachDevTools()` in the current codebase (confirmed by grep), so no
code-level change is needed for the diagnostics API call.

**2.1** WHEN `PleasantWindow.ChangeDecorations()` configures window chrome THEN the system SHALL
remove all `ExtendClientAreaChromeHints` and `ExtendClientAreaTitleBarHeightHint` assignments,
and replace `SystemDecorations = SystemDecorations.Full` with `WindowDecorations = WindowDecorations.Full`.

File: `src/PleasantUI/Controls/PleasantWindow/PleasantWindow.cs`

Required replacement for the `ChangeDecorations` method body:
```csharp
private void ChangeDecorations(bool enableCustomTitleBar, WindowState windowState)
{
    ExtendClientAreaToDecorationsHint = enableCustomTitleBar;
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || !EnableCustomTitleBar) return;

    if (windowState == WindowState.FullScreen)
    {
        WindowDecorations = WindowDecorations.Full;
    }
    else
    {
        WindowDecorations = WindowDecorations.Full;
    }
}
```

**2.2** WHEN `PleasantWindow.OnPropertyChanged` configures client area extension THEN the system
SHALL use the Avalonia 12 replacement API. The `ExtendClientAreaToDecorationsHint` property has
been removed; the new API uses `WindowDrawnDecorations`.

File: `src/PleasantUI/Controls/PleasantWindow/PleasantWindow.cs`

Required: Remove or replace the `ExtendClientAreaToDecorationsHint` assignments with the
Avalonia 12 `WindowDrawnDecorations` API.

**2.3** WHEN `PleasantMiniWindow.OnApplyTemplate` configures client area THEN the system SHALL
remove the `ExtendClientAreaTitleBarHeightHint` assignment and replace
`ExtendClientAreaToDecorationsHint` with the Avalonia 12 equivalent.

File: `src/PleasantUI/Controls/PleasantMiniWindow/PleasantMiniWindow.cs`

Required: Remove the entire `CanResizeProperty` observer that sets
`ExtendClientAreaTitleBarHeightHint`, and update `ExtendClientAreaToDecorationsHint` usages.

**3.1** WHEN `ThemeService.PasteColorAsync()` reads clipboard text THEN the system SHALL call
`TryGetTextAsync()` instead of `GetTextAsync()`.

File: `src/PleasantUI.ToolKit/Services/ThemeService.cs`

Required change:
```csharp
// Before
string? text = await _topLevel.Clipboard?.GetTextAsync();
// After
string? text = await _topLevel.Clipboard?.TryGetTextAsync();
```

**3.2** WHEN `ThemeService.PasteThemeAsync()` reads clipboard text THEN the system SHALL call
`TryGetTextAsync()` instead of `GetTextAsync()`.

File: `src/PleasantUI.ToolKit/Services/ThemeService.cs`

Required change:
```csharp
// Before
string? data = await _topLevel.Clipboard?.GetTextAsync();
// After
string? data = await _topLevel.Clipboard?.TryGetTextAsync();
```

**4.1** WHEN `ThemeEditorWindow.SetupDragAndDrop()` checks for file data THEN the system SHALL
use `DataFormat` (singular), `e.DataTransfer`, and `await e.DataTransfer.GetFilesAsync()`.

File: `src/PleasantUI.ToolKit/ThemeEditorWindow.axaml.cs`

Required changes in both `DragEnter` and `Drop` handlers:
```csharp
// Before
if (!e.Data.Contains(DataFormats.Files))
    return;
IStorageItem? file = e.Data.GetFiles()?.First();

// After
if (!e.DataTransfer.Contains(DataFormat.Files))
    return;
IStorageItem? file = (await e.DataTransfer.GetFilesAsync())?.FirstOrDefault();
```

Note: `Drop` handler must become `async` to support `await`.

**4.2** WHEN `ThemeEditorWindow.SetupDragAndDrop()` retrieves dragged files THEN the system
SHALL access `DragEventArgs.DataTransfer` instead of `DragEventArgs.Data` (already covered
by 4.1 above).

**5.1** WHEN `SmoothScrollContentPresenter` registers scroll gesture handlers THEN the system
SHALL use `InputElement.ScrollGestureEvent` instead of `Gestures.ScrollGestureEvent`.

File: `src/PleasantUI/Controls/SmoothScrollViewer/SmoothScrollContentPresenter.cs`

Required change:
```csharp
// Before
AddHandler(Gestures.ScrollGestureEvent, OnScrollGesture);
// After
AddHandler(InputElement.ScrollGestureEvent, OnScrollGesture);
```

**6.1** WHEN XAML templates bind to the placeholder text property of `TextBox` THEN the system
SHALL use `PlaceholderText` instead of `Watermark`, and `PART_FloatingPlaceholder` instead of
`PART_FloatingWatermark`, across all affected XAML files listed in defect 6.1.

Required rename in all affected files:
- `Watermark=` → `PlaceholderText=`
- `{TemplateBinding Watermark}` → `{TemplateBinding PlaceholderText}`
- `PART_Watermark` → `PART_Placeholder`
- `PART_FloatingWatermark` → `PART_FloatingPlaceholder`

**6.2** WHEN XAML templates bind to the floating placeholder property THEN the system SHALL use
`UseFloatingPlaceholder` instead of `UseFloatingWatermark`.

Required rename in affected files:
- `UseFloatingWatermark=` → `UseFloatingPlaceholder=`
- `{TemplateBinding UseFloatingWatermark}` → `{TemplateBinding UseFloatingPlaceholder}`

**6.3** WHEN XAML style selectors target the floating placeholder state THEN the system SHALL
use `[UseFloatingPlaceholder=true]` and `#PART_FloatingPlaceholder`.

File: `src/PleasantUI/Styling/ControlThemes/BasicControls/TextBox.axaml`

Required change:
```xml
<!-- Before -->
Style Selector="^[UseFloatingWatermark=true]:not(:empty) /template/ TextBlock#PART_FloatingWatermark"
<!-- After -->
Style Selector="^[UseFloatingPlaceholder=true]:not(:empty) /template/ TextBlock#PART_FloatingPlaceholder"
```

**6.4** WHEN `PleasantTitleBar` or other controls need the visual root THEN the system SHALL
use `TopLevel.GetTopLevel(this)` instead of the `VisualRoot` property.

Files and required replacements:

- `src/PleasantUI/Controls/Chrome/PleasantTitleBar.cs`:
  ```csharp
  // Before
  if (VisualRoot is PleasantWindow window)
  // After
  if (TopLevel.GetTopLevel(this) is PleasantWindow window)
  ```

- `src/PleasantUI/Controls/ShadowBorder/ShadowBorder.cs`:
  ```csharp
  // Before
  if (VisualRoot is { } renderRoot)
      scale = renderRoot.RenderScaling;
  // After
  if (TopLevel.GetTopLevel(this) is { } topLevel)
      scale = topLevel.RenderScaling;
  ```

- `src/PleasantUI/Controls/NavigationView/NavigationView.cs`:
  ```csharp
  // Before
  if (VisualRoot is PleasantWindow window)
  // After
  if (TopLevel.GetTopLevel(this) is PleasantWindow window)
  ```

- `src/PleasantUI/Controls/ModalWindowHost/ModalWindowHost.cs`:
  ```csharp
  // Before
  return VisualRoot switch { TopLevel topLevel => topLevel.ClientSize, ... }
  // After
  return TopLevel.GetTopLevel(this) is { } tl ? tl.ClientSize : ...
  ```

**6.5** WHEN `VirtualizingWrapPanel` manages item containers THEN the system SHALL remove the
`using Avalonia.Controls.Generators;` import and replace all `ItemContainerGenerator` usages
with the Avalonia 12 `VirtualizingPanel` container management APIs
(`GetOrCreateElement`, `RecycleElement`, etc.).

File: `src/PleasantUI/Controls/VirtualizingWrapPanel/VirtualizingWrapPanel.cs`

Required: Remove `using Avalonia.Controls.Generators;` and migrate all
`ItemContainerGenerator.*` calls to the Avalonia 12 equivalents on `VirtualizingPanel`.

**6.6** WHEN `VirtualizingWrapPanel.ScrollIntoView` checks for a layout root THEN the system
SHALL use `TopLevel.GetTopLevel(this) is not null` instead of `this.GetVisualRoot() is ILayoutRoot`.

File: `src/PleasantUI/Controls/VirtualizingWrapPanel/VirtualizingWrapPanel.cs`

Required change:
```csharp
// Before
if (this.GetVisualRoot() is ILayoutRoot)
// After
if (TopLevel.GetTopLevel(this) is not null)
```

**7.1** WHEN `MainActivity` is initialized THEN the system SHALL extend `AvaloniaMainActivity`
(non-generic) and a separate `AvaloniaAndroidApplication<App>` class SHALL be added.

File: `samples/PleasantUI.Example.Android/MainActivity.cs`

Required replacement:
```csharp
[Activity(...)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnStop()
    {
        PleasantSettings.Save();
        base.OnStop();
    }
}

[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer) { }
}
```

**7.2** WHEN `App.OnFrameworkInitializationCompleted` runs on Android THEN the system SHALL
check `IActivityApplicationLifetime` before `ISingleViewApplicationLifetime`.

File: `samples/PleasantUI.Example.Android/App.axaml.cs`

Required replacement:
```csharp
// Before
if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
{
    lifetime.MainView = new PleasantMainView { DataContext = ViewModel };
}

// After
if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
    activityLifetime.MainViewFactory = () => new PleasantMainView { DataContext = ViewModel };
else if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
    lifetime.MainView = new PleasantMainView { DataContext = ViewModel };
```

**8.1** WHEN `PleasantUI.MaterialIcons.SourceGenerator` is used as a Roslyn analyzer THEN the
system SHALL CONTINUE TO target `netstandard2.0` — no change required.

---

### Unchanged Behavior (Regression Prevention)

3.1 WHEN the library is compiled and run against Avalonia 12 THEN the system SHALL CONTINUE TO
render all existing PleasantUI controls (PleasantWindow, PleasantMiniWindow, ContentDialog,
PleasantTabView, NavigationView, Snackbar, etc.) with the same visual appearance.

3.2 WHEN `PleasantTheme` is initialized in an application THEN the system SHALL CONTINUE TO
load theme resources, resolve accent colors, and apply the selected theme variant correctly.

3.3 WHEN `PleasantTheme.CustomThemes` is modified THEN the system SHALL CONTINUE TO update
the theme dictionaries and re-apply the theme.

3.4 WHEN `PleasantThemesLoader.Save()` and `PleasantThemesLoader.Load()` are called THEN the
system SHALL CONTINUE TO serialize and deserialize custom themes using `Color.ToUInt32()` and
`Color.FromUInt32()` (these methods are unchanged in Avalonia 12).

3.5 WHEN `MessageBox.Show()` is called THEN the system SHALL CONTINUE TO display a modal dialog
with the correct buttons and return the selected result string.

3.6 WHEN `ColorPickerWindow.SelectColor()` is called THEN the system SHALL CONTINUE TO display
a color picker dialog and return the selected color or null on cancel.

3.7 WHEN `ThemeEditorWindow.EditTheme()` is called THEN the system SHALL CONTINUE TO display
the theme editor dialog and return the edited theme or null on cancel.

3.8 WHEN `SmoothScrollViewer` is used THEN the system SHALL CONTINUE TO handle scroll gestures
and smooth scrolling behavior.

3.9 WHEN `VirtualizingWrapPanel` is used in an `ItemsControl` THEN the system SHALL CONTINUE TO
virtualize and recycle item containers correctly.

3.10 WHEN `LocalizeExtension` is used in XAML THEN the system SHALL CONTINUE TO provide
localized strings with optional binding-based string formatting.

3.11 WHEN `PleasantWindow` is used on macOS THEN the system SHALL CONTINUE TO handle macOS-specific
title bar and caption button behavior (OverrideMacOSCaption, FullScreen state transitions).

3.12 WHEN `ThemeEditorWindow` drag-and-drop is used on Windows THEN the system SHALL CONTINUE TO
accept `.json` theme files dropped onto the drag-and-drop panel.

3.13 WHEN `ThemeService.PasteColorAsync()` or `ThemeService.PasteThemeAsync()` is called THEN
the system SHALL CONTINUE TO read text from the clipboard and parse it as a color or theme JSON.

3.14 WHEN `PleasantUI.MaterialIcons` is used THEN the system SHALL CONTINUE TO provide the full
set of Material Icons as Avalonia geometry/path data.

3.15 WHEN `PleasantUI.DataGrid` styles are applied THEN the system SHALL CONTINUE TO style the
Avalonia DataGrid control with the PleasantUI theme.

---

## Bug Condition Pseudocode

```pascal
FUNCTION isBugCondition(X)
  INPUT: X of type ProjectOrSourceFile
  OUTPUT: boolean

  RETURN X references any of the following Avalonia 11.x APIs removed or renamed in Avalonia 12:
    - Package version 11.3.3 or 11.2.0-beta1 (instead of 12.x)
    - Avalonia.Platform.ExtendClientAreaChromeHints (enum removed)
    - Window.ExtendClientAreaTitleBarHeightHint (property removed)
    - Window.ExtendClientAreaToDecorationsHint (property removed)
    - Window.SystemDecorations / SystemDecorations enum (renamed to WindowDecorations)
    - IClipboard.GetTextAsync() (replaced by TryGetTextAsync())
    - DataFormats.Files (renamed to DataFormat.Files)
    - DragEventArgs.Data (renamed to DragEventArgs.DataTransfer)
    - IDataObject.GetFiles() (replaced by IAsyncDataTransfer.GetFilesAsync())
    - Gestures.ScrollGestureEvent (moved to InputElement.ScrollGestureEvent)
    - TextBox.Watermark / UseFloatingWatermark (renamed to PlaceholderText / UseFloatingPlaceholder)
    - Visual.VisualRoot property (removed; use TopLevel.GetTopLevel())
    - ItemContainerGenerator / Avalonia.Controls.Generators namespace (removed)
    - ILayoutRoot interface (removed)
    - AvaloniaMainActivity<TApp> generic form (replaced by AvaloniaMainActivity + AvaloniaAndroidApplication<TApp>)
    - ISingleViewApplicationLifetime without IActivityApplicationLifetime check on Android
    - Avalonia.Diagnostics package (replaced by AvaloniaUI.DiagnosticsSupport)
    - netstandard2.0 target for non-generator projects (must be net8.0+)
END FUNCTION

// Property: Fix Checking
FOR ALL X WHERE isBugCondition(X) DO
  result ← build(X with Avalonia 12 packages and updated API calls)
  ASSERT result = CompilationSuccess AND runtime_behavior_correct(result)
END FOR

// Property: Preservation Checking
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT behavior(X with Avalonia 11) = behavior(X with Avalonia 12)
END FOR
```
