# SelectionList / SelectionListItem

A multi-select list control with image, title, subtitle, and timestamp support per item. Shows a selection-count header when items are selected and an empty-state message when the list is empty.

## Basic usage

```xml
<SelectionList>
    <SelectionListItem Title="Alice Johnson"
                       Subtitle="alice@example.com"
                       Timestamp="Today, 9:41 AM" />
    <SelectionListItem Title="Bob Smith"
                       Subtitle="bob@example.com"
                       Timestamp="Yesterday" />
</SelectionList>
```

## SelectionList properties

| Property | Type | Default | Description |
|---|---|---|---|
| `EmptyMessage` | `string` | `"No items found"` | Text shown when the list has no items |
| `Orientation` | `Orientation` | `Vertical` | Layout direction of the items panel |
| `ImageMemberBinding` | `BindingBase?` | — | Binding path for the image when using `ItemsSource` |
| `TitleMemberBinding` | `BindingBase?` | — | Binding path for the title |
| `SubtitleMemberBinding` | `BindingBase?` | — | Binding path for the subtitle |
| `TimestampMemberBinding` | `BindingBase?` | — | Binding path for the timestamp |
| `ImageTemplate` | `IDataTemplate?` | — | Data template for the image area |

Inherits all `ListBox` properties (`ItemsSource`, `SelectedItems`, `SelectionMode`, etc.). Default selection mode is `Multiple | Toggle`.

## SelectionListItem properties

| Property | Type | Description |
|---|---|---|
| `Title` | `string?` | Primary label |
| `Subtitle` | `string?` | Secondary text below the title |
| `Timestamp` | `string?` | Timestamp text shown on the right |
| `Image` | `IImage?` | Image shown on the left |
| `ImageTemplate` | `IDataTemplate?` | Data template for the image area |

## Data-bound list

```xml
<SelectionList ItemsSource="{Binding Contacts}"
               TitleMemberBinding="{Binding Name}"
               SubtitleMemberBinding="{Binding Email}"
               TimestampMemberBinding="{Binding LastSeen}"
               EmptyMessage="No contacts found">
    <SelectionList.ImageTemplate>
        <DataTemplate DataType="vm:ContactViewModel">
            <Ellipse Width="36" Height="36"
                     Fill="{DynamicResource AccentColor}" />
        </DataTemplate>
    </SelectionList.ImageTemplate>
</SelectionList>
```

## Reading selected items

```csharp
var selected = selectionList.SelectedItems?.Cast<MyModel>().ToList();
```

## Horizontal layout

```xml
<SelectionList Orientation="Horizontal"
               ItemsSource="{Binding Tags}"
               TitleMemberBinding="{Binding Name}" />
```

## Custom empty state

```xml
<SelectionList EmptyMessage="No results match your search." />
```
