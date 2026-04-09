# VirtualizingWrapPanel

A virtualizing panel that positions child elements in a wrap layout, breaking content to the next line at the edge of the containing box. Supports both horizontal and vertical orientation with efficient UI virtualization for large item collections.

## Basic usage

```xml
<ItemsControl ItemsSource="{Binding LargeCollection}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingWrapPanel Orientation="Horizontal" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Orientation` | `Orientation` | `Horizontal` | Axis along which items are laid out (`Horizontal` or `Vertical`) |
| `ItemWidth` | `double` | `NaN` | Width of all items in the panel |
| `ItemHeight` | `double` | `NaN` | Height of all items in the panel |
| `FirstRealizedIndex` | `int` | `-1` | Index of the first realized element (read-only) |
| `LastRealizedIndex` | `int` | `-1` | Index of the last realized element (read-only) |

## Orientation

Control the layout direction:

```xml
<!-- Horizontal layout (items flow left to right, wrap to next line) -->
<VirtualizingWrapPanel Orientation="Horizontal" />

<!-- Vertical layout (items flow top to bottom, wrap to next column) -->
<VirtualizingWrapPanel Orientation="Vertical" />
```

## Fixed item sizes

Set uniform item sizes for consistent layout:

```xml
<VirtualizingWrapPanel ItemWidth="120"
                      ItemHeight="80" />
```

When `ItemWidth` or `ItemHeight` are set, all items use those dimensions. If not set (`NaN`), items use their natural size.

## Scroll to item

Scroll to a specific item programmatically:

```csharp
var itemsControl = this.Find<ItemsControl>("itemsControl");
var panel = itemsControl.ItemsPanelRoot as VirtualizingWrapPanel;
itemsControl.ScrollIntoView(items[50]);
```

## Performance

The panel uses UI virtualization to only create and render items that are currently visible in the viewport. This makes it suitable for large collections with thousands of items.

## Example

```xml
<ItemsControl ItemsSource="{Binding Images}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingWrapPanel Orientation="Horizontal"
                                    ItemWidth="150"
                                    ItemHeight="150" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Background="LightGray" CornerRadius="8" Margin="4">
                <Image Source="{Binding}" Stretch="UniformToFill" />
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

## Notes

- The panel automatically handles viewport changes and realizes/recycles elements as needed
- Keyboard focus is preserved when elements are recycled
- The panel works with `ScrollViewer` for scrolling functionality
