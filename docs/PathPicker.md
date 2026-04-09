# PathPicker

Combines a read-only `TextBox` with a browse button. Supports `OpenFile`, `SaveFile`, and `OpenFolder` modes, optional multi-select, file-type filters, and two-way binding on the selected path text.

## Basic usage

```xml
<!-- Open a single file -->
<PathPicker Mode="OpenFile"
            SelectedPathsText="{Binding FilePath}" />

<!-- Open a folder -->
<PathPicker Mode="OpenFolder"
            SelectedPathsText="{Binding FolderPath}" />

<!-- Save a file -->
<PathPicker Mode="SaveFile"
            SuggestedFileName="output.csv"
            DefaultFileExtension=".csv"
            FileFilter="[CSV,*.csv][All]" />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Mode` | `PathPickerMode` | `OpenFile` | `OpenFile`, `SaveFile`, or `OpenFolder` |
| `Title` | `string?` | — | Dialog title shown in the platform picker |
| `AllowMultiple` | `bool` | `false` | Allow selecting multiple paths (OpenFile / OpenFolder only) |
| `SuggestedStartPath` | `string?` | — | Folder the picker opens in by default |
| `SuggestedFileName` | `string?` | — | Pre-filled file name for SaveFile mode |
| `FileFilter` | `string?` | — | File-type filter string (see format below) |
| `DefaultFileExtension` | `string?` | — | Default extension for SaveFile mode (e.g. `.txt`) |
| `SelectedPathsText` | `string?` | — | Selected path(s) as a newline-separated string — two-way bindable |
| `SelectedPaths` | `IReadOnlyList<string>` | — | Selected paths as a list (read-only) |
| `Command` | `ICommand?` | — | Command executed after the picker closes — receives `SelectedPaths` |
| `IsOmitCommandOnCancel` | `bool` | `false` | When true, `Command` is not executed if the user cancels |
| `IsClearSelectionOnCancel` | `bool` | `false` | When true, the selection is cleared if the user cancels |
| `UseCustomPicker` | `bool` | `false` | Use the built-in `PleasantFileChooser` UI instead of the platform-native dialog |
| `ButtonContent` | `object?` | — | Custom content for the browse button (defaults to a folder icon) |

## PathPickerMode values

| Value | Description |
|---|---|
| `OpenFile` | Open one or more existing files |
| `SaveFile` | Choose a path to save a file |
| `OpenFolder` | Open one or more folders |

## FileFilter format

Filters are specified as `[Name,*.ext,*.ext2]` groups, or using built-in names:

```xml
<!-- Custom filters -->
<PathPicker FileFilter="[Images,*.png,*.jpg,*.webp][All]" />

<!-- Built-in names -->
<PathPicker FileFilter="[ImageAll][Pdf][TextPlain][All]" />
```

Built-in names: `All`, `Pdf`, `ImageAll`, `ImageJpg`, `ImagePng`, `ImageWebp`, `TextPlain`

## Multi-select

```xml
<PathPicker Mode="OpenFile"
            AllowMultiple="True"
            SelectedPathsText="{Binding Paths}" />
```

When multiple paths are selected, `SelectedPathsText` contains them newline-separated. `SelectedPaths` gives you the list directly.

## With command

```xml
<PathPicker Mode="OpenFile"
            Command="{Binding LoadFileCommand}"
            IsOmitCommandOnCancel="True" />
```

## Custom picker UI

Set `UseCustomPicker="True"` to use the built-in `PleasantFileChooser` dialog instead of the OS-native picker:

```xml
<PathPicker Mode="OpenFile" UseCustomPicker="True" />
```
