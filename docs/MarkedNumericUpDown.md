# MarkedNumericUpDown

A numeric up-down control with an associated symbol (mark) that indicates the type of input expected from the user. Inherits from `NumericUpDown` and adds a `Mark` property for displaying an icon or indicator.

## Basic usage

```xml
<MarkedNumericUpDown Watermark="Enter amount"
                    Mark="{DynamicResource CurrencyIcon}" />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Mark` | `object?` | — | Symbol or indicator showing the expected input type (e.g., icon, image) |
| `Value` | `decimal?` | — | Current numeric value (inherited from `NumericUpDown`) |
| `Minimum` | `decimal` | — | Minimum value |
| `Maximum` | `decimal` | — | Maximum value |
| `Increment` | `decimal` | `1` | Amount to increment/decrement |
| `FormatString` | `string?` | — | Format string for displaying the value |
| `Watermark` | `string?` | — | Placeholder text shown when no value is entered |

## Common marks

Use icons to indicate input types:

```xml
<!-- Currency input -->
<MarkedNumericUpDown Mark="{DynamicResource DollarIcon}"
                    FormatString="C2"
                    Increment="0.01" />

<!-- Percentage input -->
<MarkedNumericUpDown Mark="{DynamicResource PercentIcon}"
                    FormatString="P2"
                    Increment="0.01" />

<!-- Quantity input -->
<MarkedNumericUpDown Mark="{DynamicResource HashIcon}"
                    Minimum="1"
                    Maximum="100" />
```

## Custom marks

The `Mark` property accepts any object:

```xml
<!-- Text mark -->
<MarkedNumericUpDown Mark="#" />

<!-- PathIcon mark -->
<MarkedNumericUpDown Mark="{DynamicResource CalculatorIcon}" />

<!-- Image mark -->
<MarkedNumericUpDown Mark="{Binding Icon}" />
```

## Value formatting

Format the displayed value:

```xml
<!-- Currency -->
<MarkedNumericUpDown FormatString="C2" />

<!-- Percentage -->
<MarkedNumericUpDown FormatString="P2" />

<!-- Fixed decimal places -->
<MarkedNumericUpDown FormatString="F2" />

<!-- Custom format -->
<MarkedNumericUpDown FormatString="0.00 units" />
```

## Range and increment

Set the allowed range and increment:

```xml
<MarkedNumericUpDown Minimum="0"
                    Maximum="100"
                    Increment="5" />
```

## Example

```xml
<StackPanel Spacing="12" Width="200">
    <TextBlock Text="Order Details" FontWeight="Bold" />
    
    <MarkedNumericUpDown Watermark="Quantity"
                        Mark="{DynamicResource BoxIcon}"
                        Minimum="1"
                        Maximum="100"
                        Increment="1" />
    
    <MarkedNumericUpDown Watermark="Price"
                        Mark="{DynamicResource DollarIcon}"
                        FormatString="C2"
                        Minimum="0"
                        Increment="0.01" />
    
    <MarkedNumericUpDown Watermark="Discount"
                        Mark="{DynamicResource PercentIcon}"
                        FormatString="P2"
                        Minimum="0"
                        Maximum="1"
                        Increment="0.01" />
</StackPanel>
```
