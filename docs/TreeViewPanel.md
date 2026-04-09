# TreeViewPanel

A panel that hosts a searchable, collapsible tree of `TreeViewSection` items. Provides a filter text box, collapse-all/expand-all buttons, and raises a unified `SelectionChanged` event when any section's selection changes.

## Basic usage

```xml
<TreeViewPanel>
    <TreeViewSection Header="Section 1">
        <TreeViewItem Content="Item 1.1" />
        <TreeViewItem Content="Item 1.2" />
    </TreeViewSection>
    <TreeViewSection Header="Section 2">
        <TreeViewItem Content="Item 2.1" />
        <TreeViewItem Content="Item 2.2" />
    </TreeViewSection>
</TreeViewPanel>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Sections` | `AvaloniaList<TreeViewSection>` | — | Collection of sections displayed in the panel |
| `FilterText` | `string?` | — | Text used to filter items across all sections |
| `FilterWatermark` | `string` | `"Filter..."` | Watermark shown in the filter text box |
| `SelectedItem` | `object?` | — | Currently selected item (across all sections) |
| `SelectedSection` | `TreeViewSection?` | — | Section that owns the currently selected item |
| `ShowExpandButton` | `bool` | `true` | Whether the expand-all button is visible |
| `ShowCollapseButton` | `bool` | `true` | Whether the collapse-all button is visible |
| `ExpandButtonColumnWidth` | `GridLength` | `32` | Width of the expand button column |
| `CollapseButtonColumnWidth` | `GridLength` | `32` | Width of the collapse button column |

## Events

| Event | Description |
|---|---|
| `SelectionChanged` | Raised when the selected item changes in any section |
| `FilterChanged` | Raised when the filter text changes |

## Methods

| Method | Description |
|---|---|
| `ExpandAll()` | Expands all sections |
| `CollapseAll()` | Collapses all sections |
| `ClearFilter()` | Clears the filter text |

## Filtering

The filter text box automatically filters items across all sections. When text is entered, only items matching the filter (case-insensitive) are displayed:

```xml
<TreeViewPanel FilterWatermark="Search items..." />
```

```csharp
// Set filter programmatically
treeViewPanel.FilterText = "search term";
```

## Expand/Collapse buttons

The panel includes expand-all and collapse-all buttons that can be shown or hidden:

```xml
<TreeViewPanel ShowExpandButton="False"
                ShowCollapseButton="False" />
```

## Selection handling

When an item is selected in any section, the `SelectionChanged` event is raised. The `SelectedItem` and `SelectedSection` properties are updated automatically:

```csharp
treeViewPanel.SelectionChanged += (sender, e) =>
{
    var selectedItem = treeViewPanel.SelectedItem;
    var selectedSection = treeViewPanel.SelectedSection;
};
```

## Custom column widths

Adjust the width of the expand/collapse button columns:

```xml
<TreeViewPanel ExpandButtonColumnWidth="40"
                CollapseButtonColumnWidth="40" />
```
