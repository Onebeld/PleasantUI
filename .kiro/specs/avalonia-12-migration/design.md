# Avalonia 12 Migration Bugfix Design

## Overview

PleasantUI currently targets Avalonia 11.3.3. Avalonia 12 introduces breaking API changes across
package references, window decoration APIs, clipboard, drag-and-drop, gesture events, renamed
members, Android initialization, and target frameworks. This design formalizes each broken API
as a bug condition, specifies the exact replacement, and defines a testing strategy to verify
the fix compiles and preserves all existing runtime behavior.

The fix is purely mechanical: no logic changes, no new features. Every change is a direct
one-to-one API substitution or package version bump as specified in the requirements document.

## Glossary

- **Bug_Condition (C)**: The condition that triggers the bug — any source file or project file
  that references an Avalonia 11.x API that was removed or renamed in Avalonia 12.
- **Property (P)**: The desired outcome — the project compiles successfully against Avalonia 12
  and all existing runtime behaviors are preserved.
- **Preservation**: All existing PleasantUI control behaviors, theme loading, dialog flows,
  scroll gestures, virtualization, and drag-and-drop that must remain unchanged after the fix.
- **isBugCondition(X)**: Pseudocode function that returns true when file X contains a broken API.
- **WindowDecorations**: The Avalonia 12 replacement for the removed `SystemDecorations` enum
  and `ExtendClientAreaChromeHints`/`ExtendClientAreaTitleBarHeightHint` properties.
- **DataFormat**: Avalonia 12 singular rename of `DataFormats` (plural).
- **DataTransfer**: Avalonia 12 rename of `DragEventArgs.Data`.
- **TryGetTextAsync**: Avalonia 12 replacement for `IClipboard.GetTextAsync()`.
- **TopLevel.GetTopLevel(this)**: Avalonia 12 replacement for the removed `Visual.VisualRoot` property.
- **VirtualizingPanel container API**: Avalonia 12 replacement for the removed
  `ItemContainerGenerator` / `Avalonia.Controls.Generators` namespace.

## Bug Details

### Bug Condition

The build fails when any project file or source file references an Avalonia 11.x API that was
removed or renamed in Avalonia 12. There are 8 distinct categories of breakage spanning
package references, C# APIs, and XAML attributes.

**Formal Specification:**
```
FUNCTION isBugCondition(X)
  INPUT: X of type ProjectOrSourceFile
  OUTPUT: boolean

  RETURN X references ANY of:
    -- Section 1: Package / TFM
    PackageVersion IN ["11.3.3", "11.2.0-beta1"]
    OR TargetFramework = "netstandard2.0" IN non-generator project
    OR PackageReference = "Avalonia.Diagnostics"

    -- Section 2: Window decoration API
    OR "ExtendClientAreaChromeHints"
    OR "ExtendClientAreaTitleBarHeightHint"
    OR "ExtendClientAreaToDecorationsHint"
    OR "SystemDecorations" (as enum/property, not WindowDecorations)

    -- Section 3: Clipboard API
    OR "IClipboard.GetTextAsync()"

    -- Section 4: Drag-and-drop API
    OR "DataFormats.Files"
    OR "e.Data.Contains" / "e.Data.GetFiles()"

    -- Section 5: Gesture events
    OR "Gestures.ScrollGestureEvent"

    -- Section 6: Renamed/removed members
    OR "TextBox.Watermark" / "UseFloatingWatermark" (XAML attributes)
    OR "PART_Watermark" / "PART_FloatingWatermark" (XAML part names)
    OR "Visual.VisualRoot" property access
    OR "using Avalonia.Controls.Generators"
    OR "ItemContainerGenerator" usage
    OR "ILayoutRoot" interface

    -- Section 7: Android initialization
    OR "AvaloniaMainActivity<App>" (generic form)
    OR missing IActivityApplicationLifetime check
END FUNCTION
```

### Examples

- **Section 1**: `<PackageReference Include="Avalonia" Version="11.3.3" />` → build error: package
  not found / API mismatch.
- **Section 2**: `ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default`
  in `PleasantWindow.ChangeDecorations()` → CS0117 / CS1061 compile error.
- **Section 3**: `await _topLevel.Clipboard?.GetTextAsync()` in `ThemeService` → CS1061: method
  not found.
- **Section 4**: `e.Data.Contains(DataFormats.Files)` in `ThemeEditorWindow` → CS0103 / CS1061.
- **Section 5**: `Gestures.ScrollGestureEvent` in `SmoothScrollContentPresenter` → CS0117.
- **Section 6**: `Watermark="Value"` in XAML → runtime binding warning / property not found.
- **Section 6**: `if (VisualRoot is PleasantWindow window)` → CS1061: `VisualRoot` not a member
  of `Visual`.
- **Section 6**: `ItemContainerGenerator!.ClearItemContainer(element)` → CS0246: type not found.
- **Section 7**: `public class MainActivity : AvaloniaMainActivity<App>` → CS0305: generic type
  not found.

## Expected Behavior

### Preservation Requirements

**Unchanged Behaviors:**
- All PleasantUI controls (PleasantWindow, PleasantMiniWindow, ContentDialog, PleasantTabView,
  NavigationView, Snackbar, etc.) render with the same visual appearance.
- `PleasantTheme` loads resources, resolves accent colors, and applies theme variants correctly.
- `PleasantThemesLoader.Save()` / `Load()` serialize and deserialize custom themes correctly
  (`Color.ToUInt32()` / `Color.FromUInt32()` are unchanged in Avalonia 12).
- `MessageBox.Show()` displays a modal dialog and returns the selected result.
- `ColorPickerWindow.SelectColor()` displays a color picker and returns the selected color.
- `ThemeEditorWindow.EditTheme()` displays the theme editor and returns the edited theme.
- `SmoothScrollViewer` handles scroll gestures and smooth scrolling.
- `VirtualizingWrapPanel` virtualizes and recycles item containers correctly.
- `LocalizeExtension` provides localized strings in XAML.
- macOS-specific title bar and caption button behavior is preserved.
- Drag-and-drop of `.json` theme files onto `ThemeEditorWindow` continues to work.
- Clipboard paste in `ThemeService` reads text and parses it as color or theme JSON.
- `PleasantUI.MaterialIcons` provides the full set of Material Icons.
- `PleasantUI.DataGrid` styles apply correctly to the Avalonia DataGrid.

**Scope:**
All behaviors that do NOT involve the broken Avalonia 11.x APIs listed in the bug condition
must be completely unaffected by this fix. The changes are purely mechanical API substitutions.

## Hypothesized Root Cause

The root cause is straightforward: Avalonia 12 is a major version with intentional breaking
changes. The library has not been updated to track these changes. There is no logic error —
every broken site is a direct consequence of API removal or renaming in the upstream framework.

Specific causes by section:

1. **Package versions pinned to 11.3.3**: The `.csproj` files were never updated after the
   Avalonia 12 release. NuGet will fail to resolve Avalonia 12 APIs against 11.x packages.

2. **Window decoration API removal**: Avalonia 12 replaced the `ExtendClientArea*` family of
   properties with a unified `WindowDecorations` API. The old properties no longer exist on
   `Window`.

3. **Clipboard API rename**: `IClipboard.GetTextAsync()` was renamed to `TryGetTextAsync()` to
   better reflect that it may return null.

4. **Drag-and-drop API rename**: `DataFormats` → `DataFormat`, `DragEventArgs.Data` →
   `DragEventArgs.DataTransfer`, `GetFiles()` → `GetFilesAsync()` (now async).

5. **Gesture event relocation**: `Gestures.ScrollGestureEvent` was moved to
   `InputElement.ScrollGestureEvent` as part of a gesture API reorganization.

6. **Renamed/removed members**: `TextBox.Watermark` → `PlaceholderText`, `Visual.VisualRoot`
   removed in favor of `TopLevel.GetTopLevel()`, `ItemContainerGenerator` /
   `Avalonia.Controls.Generators` namespace removed in favor of direct `VirtualizingPanel`
   container APIs, `ILayoutRoot` removed.

7. **Android initialization split**: The generic `AvaloniaMainActivity<TApp>` was split into
   `AvaloniaMainActivity` (activity) + `AvaloniaAndroidApplication<TApp>` (application class)
   to align with Android lifecycle best practices.

## Correctness Properties

Property 1: Bug Condition — Avalonia 12 API Compatibility

_For any_ project file or source file where `isBugCondition` returns true (i.e., it references
a removed or renamed Avalonia 11.x API), the fixed version of that file SHALL compile
successfully against Avalonia 12 packages and produce no CS-series compile errors related to
the migrated APIs.

**Validates: Requirements 1.1, 1.2, 1.3, 2.1, 2.2, 2.3, 3.1, 3.2, 4.1, 5.1, 6.1, 6.2, 6.3,
6.4, 6.5, 6.6, 7.1, 7.2**

Property 2: Preservation — Runtime Behavior Unchanged

_For any_ input or user interaction that does NOT involve the broken Avalonia 11.x APIs (i.e.,
`isBugCondition` returns false for the code path), the fixed library SHALL produce exactly the
same runtime behavior as the original library compiled against Avalonia 11.3.3, preserving all
control rendering, theme loading, dialog flows, scroll behavior, virtualization, and
drag-and-drop functionality.

**Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10, 3.11, 3.12,
3.13, 3.14, 3.15**

## Fix Implementation

### Changes Required

All changes are mechanical one-to-one API substitutions. No logic changes are required.

---

**File**: `src/PleasantUI/PleasantUI.csproj`
**Change**: Bump `Avalonia` and `Avalonia.Skia` from `11.3.3` to `12.x`.

**File**: `src/PleasantUI.ToolKit/PleasantUI.ToolKit.csproj`
**Change**: Bump `Avalonia`, `Avalonia.Controls.ColorPicker`, `Avalonia.Skia` from `11.3.3` to `12.x`.

**File**: `src/PleasantUI.DataGrid/PleasantUI.DataGrid.csproj`
**Change**: Bump `Avalonia.Controls.DataGrid` from `11.3.3` to `12.x`.

**File**: `src/PleasantUI.MaterialIcons/PleasantUI.MaterialIcons.csproj`
**Changes**:
1. Bump `Avalonia` from `11.3.3` to `12.x`.
2. Change `<TargetFramework>netstandard2.0</TargetFramework>` → `<TargetFramework>net8.0</TargetFramework>`.

**File**: `samples/PleasantUI.Example.Desktop/PleasantUI.Example.Desktop.csproj`
**Changes**:
1. Bump `Avalonia`, `Avalonia.Desktop` from `11.3.3` to `12.x`.
2. Replace `Avalonia.Diagnostics` → `AvaloniaUI.DiagnosticsSupport` version `2.2.0-beta3`.

**File**: `samples/PleasantUI.Example.Browser/PleasantUI.Example.Browser.csproj`
**Change**: Bump `Avalonia.Browser` from `11.3.3` to `12.x`.

**File**: `designer/PleasantUI.Designer/PleasantUI.Designer.csproj`
**Changes**:
1. Bump `Avalonia`, `Avalonia.Desktop` from `11.3.3` to `12.x`.
2. Replace `Avalonia.Diagnostics` → `AvaloniaUI.DiagnosticsSupport` version `2.2.0-beta3`.

**File**: `samples/PleasantUI.Example.Android/PleasantUI.Example.Android.csproj`
**Change**: Bump `Avalonia.Android` from `11.2.0-beta1` to `12.x`.

---

**File**: `src/PleasantUI/Controls/PleasantWindow/PleasantWindow.cs`
**Function**: `ChangeDecorations` and `OnPropertyChanged`
**Changes**:
1. Remove all `ExtendClientAreaChromeHints` assignments.
2. Remove all `ExtendClientAreaTitleBarHeightHint` assignments.
3. Replace `SystemDecorations = SystemDecorations.Full` → `WindowDecorations = WindowDecorations.Full`.
4. Replace `ExtendClientAreaToDecorationsHint = ...` with the Avalonia 12 equivalent
   (`WindowDrawnDecorations` or remove if no direct replacement exists).

---

**File**: `src/PleasantUI/Controls/PleasantMiniWindow/PleasantMiniWindow.cs`
**Function**: `OnApplyTemplate`
**Changes**:
1. Remove the `CanResizeProperty` observer that sets `ExtendClientAreaTitleBarHeightHint`.
2. Replace `ExtendClientAreaToDecorationsHint = ...` with the Avalonia 12 equivalent.

---

**File**: `src/PleasantUI.ToolKit/Services/ThemeService.cs`
**Changes**:
1. `PasteColorAsync`: `GetTextAsync()` → `TryGetTextAsync()`.
2. `PasteThemeAsync`: `GetTextAsync()` → `TryGetTextAsync()`.

---

**File**: `src/PleasantUI.ToolKit/ThemeEditorWindow.axaml.cs`
**Function**: `SetupDragAndDrop` (both `DragEnter` and `Drop` handlers)
**Changes**:
1. `DataFormats.Files` → `DataFormat.Files`.
2. `e.Data.Contains(...)` → `e.DataTransfer.Contains(...)`.
3. `e.Data.GetFiles()?.First()` → `(await e.DataTransfer.GetFilesAsync())?.FirstOrDefault()`.
4. Make the `Drop` handler `async`.

---

**File**: `src/PleasantUI/Controls/SmoothScrollViewer/SmoothScrollContentPresenter.cs`
**Change**: `Gestures.ScrollGestureEvent` → `InputElement.ScrollGestureEvent`.

---

**Files** (XAML — Watermark → PlaceholderText):
- `src/PleasantUI.ToolKit/ThemeEditorWindow.axaml`
- `src/PleasantUI/Styling/ControlThemes/PleasantControls/MarkedNumericUpDown.axaml`
- `src/PleasantUI/Styling/ControlThemes/PleasantControls/MarkedTextBox.axaml`
- `src/PleasantUI/Styling/ControlThemes/BasicControls/TextBox.axaml`
- `src/PleasantUI/Styling/ControlThemes/BasicControls/NumericUpDown.axaml`
- `src/PleasantUI/Styling/ControlThemes/BasicControls/CalendarDatePicker.axaml`
- `src/PleasantUI/Styling/ControlThemes/BasicControls/AutoCompleteBox.axaml`
- `samples/PleasantUI.Example/Views/Pages/PleasantControlPages/ProgressRingPageView.axaml`

**Changes in each**:
- `Watermark=` → `PlaceholderText=`
- `{TemplateBinding Watermark}` → `{TemplateBinding PlaceholderText}`
- `PART_Watermark` → `PART_Placeholder`
- `PART_FloatingWatermark` → `PART_FloatingPlaceholder`
- `UseFloatingWatermark=` → `UseFloatingPlaceholder=`
- `{TemplateBinding UseFloatingWatermark}` → `{TemplateBinding UseFloatingPlaceholder}`
- Style selector `[UseFloatingWatermark=true]` → `[UseFloatingPlaceholder=true]`

---

**Files** (C# — VisualRoot → TopLevel.GetTopLevel):
- `src/PleasantUI/Controls/Chrome/PleasantTitleBar.cs`:
  `if (VisualRoot is PleasantWindow window)` → `if (TopLevel.GetTopLevel(this) is PleasantWindow window)`
- `src/PleasantUI/Controls/ShadowBorder/ShadowBorder.cs`:
  `if (VisualRoot is { } renderRoot)` → `if (TopLevel.GetTopLevel(this) is { } topLevel)`
- `src/PleasantUI/Controls/NavigationView/NavigationView.cs`:
  `if (VisualRoot is PleasantWindow window)` → `if (TopLevel.GetTopLevel(this) is PleasantWindow window)`
- `src/PleasantUI/Controls/ModalWindowHost/ModalWindowHost.cs`:
  `return VisualRoot switch { TopLevel tl => tl.ClientSize, ... }` →
  `return TopLevel.GetTopLevel(this) is { } tl ? tl.ClientSize : ...`

---

**File**: `src/PleasantUI/Controls/VirtualizingWrapPanel/VirtualizingWrapPanel.cs`
**Changes**:
1. Remove `using Avalonia.Controls.Generators;`.
2. In `GetRecycledElement`: replace `ItemContainerGenerator generator = ItemContainerGenerator!;`
   and `generator.PrepareItemContainer` / `generator.ItemContainerPrepared` calls with the
   Avalonia 12 `VirtualizingPanel` equivalents (`PrepareContainerForIndex`,
   `ContainerForIndex`, etc.).
3. In `CreateElement`: replace `generator.NeedsContainer`, `generator.CreateContainer`,
   `generator.PrepareItemContainer`, `generator.ItemContainerPrepared` with Avalonia 12 APIs.
4. In `RecycleElement` and `RecycleElementOnItemRemoved`: replace
   `ItemContainerGenerator!.ClearItemContainer(element)` with Avalonia 12 equivalent.
5. In `UpdateElementIndex`: replace `ItemContainerGenerator.ItemContainerIndexChanged` with
   Avalonia 12 equivalent or remove if no longer needed.
6. In `ScrollIntoView`: replace `this.GetVisualRoot() is ILayoutRoot` with
   `TopLevel.GetTopLevel(this) is not null`.

---

**File**: `samples/PleasantUI.Example.Android/MainActivity.cs`
**Changes**:
1. Change `public class MainActivity : AvaloniaMainActivity<App>` →
   `public class MainActivity : AvaloniaMainActivity`.
2. Remove `CustomizeAppBuilder` override (no longer needed).
3. Add new `AndroidApp : AvaloniaAndroidApplication<App>` class with `[Application]` attribute.

**File**: `samples/PleasantUI.Example.Android/App.axaml.cs`
**Change**: Add `IActivityApplicationLifetime` check before `ISingleViewApplicationLifetime`.

## Testing Strategy

### Validation Approach

The testing strategy follows a two-phase approach: first, surface counterexamples that
demonstrate the build failures on unfixed code, then verify the fix compiles and preserves
all existing runtime behavior.

### Exploratory Bug Condition Checking

**Goal**: Surface counterexamples that demonstrate the compilation failures BEFORE implementing
the fix. Confirm the root cause analysis (API removal/rename) by observing exact compiler errors.

**Test Plan**: Attempt to build the solution against Avalonia 12 packages without any code
changes. Observe and record every CS-series compile error. Each error maps directly to one of
the bug condition items in `isBugCondition`.

**Test Cases**:
1. **Package resolution test**: Add Avalonia 12 packages to `.csproj` files without updating
   any C# or XAML — observe CS0117/CS1061/CS0246 errors for every removed API.
2. **Window decoration test**: Compile `PleasantWindow.cs` and `PleasantMiniWindow.cs` against
   Avalonia 12 — observe errors on `ExtendClientAreaChromeHints`, `ExtendClientAreaTitleBarHeightHint`,
   `ExtendClientAreaToDecorationsHint`, `SystemDecorations`.
3. **Clipboard test**: Compile `ThemeService.cs` — observe CS1061 on `GetTextAsync`.
4. **Drag-and-drop test**: Compile `ThemeEditorWindow.axaml.cs` — observe CS0103 on `DataFormats`,
   CS1061 on `e.Data`.
5. **Gesture test**: Compile `SmoothScrollContentPresenter.cs` — observe CS0117 on
   `Gestures.ScrollGestureEvent`.
6. **VisualRoot test**: Compile `PleasantTitleBar.cs`, `ShadowBorder.cs`, `NavigationView.cs`,
   `ModalWindowHost.cs` — observe CS1061 on `VisualRoot`.
7. **ItemContainerGenerator test**: Compile `VirtualizingWrapPanel.cs` — observe CS0246 on
   `Avalonia.Controls.Generators` and `ItemContainerGenerator`.
8. **Android test**: Compile `MainActivity.cs` — observe CS0305 on `AvaloniaMainActivity<App>`.

**Expected Counterexamples**:
- Multiple CS0117, CS1061, CS0246, CS0305 errors confirming each removed/renamed API.
- These errors directly confirm the root cause: Avalonia 12 removed these APIs.

### Fix Checking

**Goal**: Verify that for all files where `isBugCondition` returns true, the fixed files
compile successfully against Avalonia 12.

**Pseudocode:**
```
FOR ALL file WHERE isBugCondition(file) DO
  result := build(fixedFile, avaloniaVersion = 12)
  ASSERT result.errors = []
  ASSERT result.warnings NOT CONTAIN "removed API" references
END FOR
```

### Preservation Checking

**Goal**: Verify that for all runtime behaviors where `isBugCondition` returns false (i.e.,
behaviors not involving the broken APIs), the fixed library behaves identically to the
original library.

**Pseudocode:**
```
FOR ALL behavior WHERE NOT isBugCondition(behavior.codeFile) DO
  ASSERT behavior(fixedLib, avaloniaVersion = 12) = behavior(originalLib, avaloniaVersion = 11)
END FOR
```

**Testing Approach**: Property-based testing is recommended for preservation checking because:
- It generates many test cases automatically across the input domain.
- It catches edge cases that manual unit tests might miss.
- It provides strong guarantees that behavior is unchanged for all non-buggy inputs.

**Test Plan**: Observe behavior on UNFIXED code (Avalonia 11) first for all non-API-breaking
paths, then write property-based tests capturing that behavior, then verify on fixed code.

**Test Cases**:
1. **Theme loading preservation**: Verify `PleasantTheme` loads and applies correctly.
2. **Control rendering preservation**: Verify PleasantWindow, PleasantMiniWindow, ContentDialog
   render with correct visual appearance.
3. **Scroll behavior preservation**: Verify `SmoothScrollViewer` handles scroll gestures.
4. **Virtualization preservation**: Verify `VirtualizingWrapPanel` recycles containers correctly.
5. **Clipboard preservation**: Verify `TryGetTextAsync()` returns the same text as
   `GetTextAsync()` did for non-null clipboard content.
6. **Drag-and-drop preservation**: Verify `.json` theme files dropped onto `ThemeEditorWindow`
   are still accepted and parsed.

### Unit Tests

- Test each migrated API call in isolation (clipboard, drag-and-drop, gesture handler).
- Test `VirtualizingWrapPanel` container lifecycle: create, recycle, remove, index update.
- Test `PleasantWindow.ChangeDecorations` with each `WindowState` value.
- Test `PleasantMiniWindow.OnApplyTemplate` with `CanResize` true and false.
- Test Android `MainActivity` and `App` initialization paths.

### Property-Based Tests

- Generate random clipboard text values and verify `TryGetTextAsync()` returns the same
  result as `GetTextAsync()` for non-null inputs.
- Generate random `DragEventArgs` with file payloads and verify `DataTransfer.GetFilesAsync()`
  returns the same files as `Data.GetFiles()` did.
- Generate random item collections and verify `VirtualizingWrapPanel` produces the same
  realized container set before and after migration.
- Generate random `WindowState` values and verify `ChangeDecorations` sets `WindowDecorations`
  correctly for each state.

### Integration Tests

- Build the full solution against Avalonia 12 and assert zero compile errors.
- Run the `PleasantUI.Example.Desktop` sample and verify all pages render correctly.
- Run the `PleasantUI.Designer` and verify the designer host loads without errors.
- Test the `ThemeEditorWindow` drag-and-drop flow end-to-end.
- Test the `SmoothScrollViewer` scroll gesture flow end-to-end.
