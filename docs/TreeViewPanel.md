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
| `FilterText` | `string?` | — | Text used to filter items across all sections |
| `FilterWatermark` | `string` | `"Filter..."` | Watermark shown in the filter text box |
| `SelectedItem` | `object?` | — | Currently selected item (across all sections) |
| `SelectedSection` | `TreeViewSection?` | — | Section that owns the currently selected item |
| `ShowExpandButton` | `bool` | `true` | Whether the expand-all button is visible |
| `ShowCollapseButton` | `bool` | `true` | Whether the collapse-all button is visible |
| `ExpandButtonColumnWidth` | `GridLength` | `32` | Width of the expand button column |
| `CollapseButtonColumnWidth` | `GridLength` | `32` | Width of the collapse button column |
| `Sections` | `AvaloniaList<TreeViewSection>` | — | Collection of sections displayed in the panel |

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

## Expand/collapse buttons

The expand-all and collapse-all buttons allow users to quickly expand or collapse all sections at once.

```csharp
treeViewPanel.ExpandAll();
treeViewPanel.CollapseAll();
```

Control button visibility:

```xml
<TreeViewPanel ShowExpandButton="True"
              ShowCollapseButton="True" />
```

```csharp
treeViewPanel.ShowExpandButton = false;
treeViewPanel.ShowCollapseButton = false;
```

## Button column widths

Customize the width of the expand and collapse button columns:

```xml
<TreeViewPanel ExpandButtonColumnWidth="40"
              CollapseButtonColumnWidth="40" />
```

```csharp
treeViewPanel.ExpandButtonColumnWidth = new GridLength(40);
treeViewPanel.CollapseButtonColumnWidth = new GridLength(40);
```

These columns animate when the search box gains/loses focus.

## Selection handling

When an item is selected in any section, the `SelectionChanged` event is raised. The `SelectedItem` and `SelectedSection` properties are updated automatically:

```csharp
treeViewPanel.SelectionChanged += (sender, e) =>
{
    var selectedItem = treeViewPanel.SelectedItem;
    var selectedSection = treeViewPanel.SelectedSection;
};
```

## Filter changed event

Handle filter text changes:

```csharp
treeViewPanel.FilterChanged += (sender, text) =>
{
    // Respond to filter text changes
};
```
