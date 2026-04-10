# MarkedTextBox / MarkedNumericUpDown

Input controls that display an inline label or unit marker alongside the input field. Useful for forms where the field type or unit needs to be visible at all times (e.g. "kg", "$", "@").

## MarkedTextBox

Extends `TextBox` with a `Mark` property rendered inside the control.

```xml
<MarkedTextBox Mark="@" PlaceholderText="username" />

<MarkedTextBox Mark="https://" PlaceholderText="example.com" />
```

### Properties

| Property | Type | Description |
|---|---|---|
| `Mark` | `object?` | The label shown inside the text box (string or any control) |

All standard `TextBox` properties (`Text`, `PlaceholderText`, `MaxLength`, etc.) are inherited.

## MarkedNumericUpDown

Extends `NumericUpDown` with a `Mark` property rendered inside the control.

```xml
<MarkedNumericUpDown Mark="kg" Value="{Binding Weight}" Minimum="0" Maximum="500" />

<MarkedNumericUpDown Mark="$" Value="{Binding Price}" FormatString="F2" />
```

### Properties

| Property | Type | Description |
|---|---|---|
| `Mark` | `object?` | The label shown inside the numeric control (string or any control) |

All standard `NumericUpDown` properties (`Value`, `Minimum`, `Maximum`, `Increment`, `FormatString`, etc.) are inherited.

## Custom mark content

The `Mark` property accepts any object, so you can use a `PathIcon` or other control:

```xml
<MarkedTextBox PlaceholderText="Search">
    <MarkedTextBox.Mark>
        <PathIcon Data="{DynamicResource SearchRegular}" Width="14" Height="14" />
    </MarkedTextBox.Mark>
</MarkedTextBox>
```
