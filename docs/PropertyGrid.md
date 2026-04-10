# PropertyGrid

A scrollable two-column grid that displays a list of `PropertyRow` items. The left column shows labels and the right column shows values — which can be plain text, clickable links, colored status text, or arbitrary templated content.

## Basic usage

```xml
<PropertyGrid LabelColumnWidth="150"
             RowSpacing="10">
    <PropertyRow Label="Name" Value="John Doe" />
    <PropertyRow Label="Email" Value="john@example.com" />
    <PropertyRow Label="Status" Value="Active" />
</PropertyGrid>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `LabelColumnWidth` | `GridLength` | `Auto` | Width of the label column |
| `RowSpacing` | `double` | `10` | Vertical spacing between rows |
| `Rows` | `AvaloniaList<PropertyRow>` | — | Collection of property rows |

## Label column width

Control the width of the label column:

```xml
<!-- Auto width (default) -->
<PropertyGrid LabelColumnWidth="Auto" />

<!-- Fixed width -->
<PropertyGrid LabelColumnWidth="150" />

<!-- Star width (proportional) -->
<PropertyGrid LabelColumnWidth="*" />
```

## Row spacing

Adjust the vertical spacing between rows:

```xml
<PropertyGrid RowSpacing="8" />
```

## Adding rows

Add rows in XAML:

```xml
<PropertyGrid>
    <PropertyRow Label="Name" Value="John Doe" />
    <PropertyRow Label="Email" Value="john@example.com" />
</PropertyGrid>
```

Or programmatically:

```csharp
propertyGrid.Rows.Add(new PropertyRow
{
    Label = "Name",
    Value = "John Doe"
});
```

## Value types

The value column can display different content types:

```xml
<!-- Plain text -->
<PropertyRow Label="Name" Value="John Doe" />

<!-- Clickable link -->
<PropertyRow Label="Website" Value="https://example.com" />

<!-- Colored status -->
<PropertyRow Label="Status" Value="Active" />

<!-- Custom content -->
<PropertyRow Label="Icon">
    <PropertyRow.Value>
        <PathIcon Data="{DynamicResource CheckIcon}" />
    </PropertyRow.Value>
</PropertyRow>
```

## Data binding

Bind to a data source:

```xml
<PropertyGrid>
    <PropertyRow Label="Name" Value="{Binding UserName}" />
    <PropertyRow Label="Email" Value="{Binding UserEmail}" />
    <PropertyRow Label="Status" Value="{Binding UserStatus}" />
</PropertyGrid>
```

## Example

```xml
<PropertyGrid LabelColumnWidth="120"
             RowSpacing="12">
    <PropertyRow Label="Version" Value="1.0.0" />
    <PropertyRow Label="Build Date" Value="2024-01-15" />
    <PropertyRow Label="License" Value="MIT" />
    <PropertyRow Label="Author" Value="John Doe" />
    <PropertyRow Label="Repository" Value="https://github.com/example/repo" />
</PropertyGrid>
```

## Template parts

The control template must provide:
- `PART_RowsHost` - ItemsControl that hosts the property rows
