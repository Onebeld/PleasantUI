# ItemListPanel

A panel that displays a searchable, filterable, multi-selectable list of items with an optional loading overlay, bulk-action toolbar, and pagination footer slot.

## Basic usage

```xml
<ItemListPanel ItemsSource="{Binding Items}"
              ItemTemplate="{StaticResource ItemTemplate}"
              SearchText="{Binding SearchText, Mode=TwoWay}" />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `ItemsSource` | `IEnumerable?` | — | Items source for the list |
| `ItemTemplate` | `IDataTemplate?` | — | Data template for list items |
| `SelectedItem` | `object?` | — | Currently selected item |
| `SelectedCount` | `int` | `0` | Number of selected items in multi-select mode (read-only) |
| `IsLoading` | `bool` | `false` | Whether the loading overlay is shown |
| `LoadingTitle` | `string` | `"Loading..."` | Title shown in the loading overlay |
| `LoadingSubtitle` | `string?` | — | Subtitle shown in the loading overlay |
| `IsMultiSelectMode` | `bool` | `false` | Whether multi-select mode is active |
| `SearchText` | `string?` | — | Search/filter text |
| `SearchWatermark` | `string` | `"Search..."` | Watermark shown in the search box |
| `BulkActionsContent` | `object?` | — | Content shown in the bulk-actions bar (visible in multi-select mode) |
| `FooterContent` | `object?` | — | Content shown in the footer slot (e.g. pagination) |
| `EmptyStateTitle` | `string` | `"No items"` | Title shown in the empty state |
| `EmptyStateSubtitle` | `string?` | — | Subtitle shown in the empty state |

## Events

| Event | Description |
|---|---|
| `SelectionChanged` | Raised when the selected item changes |
| `SearchChanged` | Raised when the search text changes |

## Methods

| Method | Description |
|---|---|
| `GetSelectedItems()` | Gets all currently selected items (multi-select mode) |
| `SelectAll()` | Selects all items |
| `UnselectAll()` | Clears all selections |
| `ClearSearch()` | Clears the search text |

## Loading state

Show a loading overlay while data is being fetched:

```xml
<ItemListPanel IsLoading="True"
              LoadingTitle="Loading items..."
              LoadingSubtitle="Please wait" />
```

```csharp
itemListPanel.IsLoading = true;
// ... load data ...
itemListPanel.IsLoading = false;
```

## Multi-select mode

Enable multi-select mode to allow selecting multiple items:

```xml
<ItemListPanel IsMultiSelectMode="True">
    <ItemListPanel.BulkActionsContent>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <Button Content="Delete" />
            <Button Content="Archive" />
        </StackPanel>
    </ItemListPanel.BulkActionsContent>
</ItemListPanel>
```

```csharp
itemListPanel.IsMultiSelectMode = true;
var selectedItems = itemListPanel.GetSelectedItems();
itemListPanel.SelectAll();
itemListPanel.UnselectAll();
```

## Searching and filtering

The panel includes a search box that filters items in real-time:

```xml
<ItemListPanel SearchWatermark="Filter items..." />
```

```csharp
// Set search text programmatically
itemListPanel.SearchText = "search term";

// Clear search
itemListPanel.ClearSearch();
```

## Empty state

Customize the empty state when no items are available:

```xml
<ItemListPanel EmptyStateTitle="No results found"
              EmptyStateSubtitle="Try adjusting your search criteria" />
```

## Footer content

Add a footer for pagination or other controls:

```xml
<ItemListPanel>
    <ItemListPanel.FooterContent>
        <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Spacing="8">
            <Button Content="Previous" />
            <TextBlock Text="Page 1 of 10" />
            <Button Content="Next" />
        </StackPanel>
    </ItemListPanel.FooterContent>
</ItemListPanel>
```

## Selection handling

Handle selection changes:

```csharp
itemListPanel.SelectionChanged += (sender, e) =>
{
    var selectedItem = itemListPanel.SelectedItem;
    var selectedCount = itemListPanel.SelectedCount;
};
```
