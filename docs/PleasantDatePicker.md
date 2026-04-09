# PleasantDatePicker

A PleasantUI-styled DatePicker that decouples the visual container from the internal flyout button, allowing full theme control over the outer shape. The template must provide a click area and a hidden flyout button for Avalonia's internal wiring.

## Basic usage

```xml
<PleasantDatePicker Watermark="Select a date" />
```

## Properties

Inherits all properties from `DatePicker`:
- `SelectedDate` - The currently selected date
- `Watermark` - Placeholder text shown when no date is selected
- `MinYear` - Minimum year that can be selected
- `MaxYear` - Maximum year that can be selected
- `IsTodayHighlighted` - Whether today's date is highlighted in the calendar
- `DisplayDate` - The date to display in the calendar view
- And other standard DatePicker properties

## Template parts

The control template must provide:
- `PART_ClickArea` - The visible, styled container that the user clicks
- `PART_FlyoutButton` - A hidden zero-size Button kept for Avalonia's internal wiring

## Basic example

```xml
<PleasantDatePicker Watermark="Select a date"
                   SelectedDate="{Binding BirthDate}" />
```

## Date range

Restrict selectable dates:

```xml
<PleasantDatePicker MinYear="1900"
                   MaxYear="2100" />
```

## Today highlight

Control whether today's date is highlighted:

```xml
<PleasantDatePicker IsTodayHighlighted="True" />
```

## Binding

Bind to a DateTime property:

```xml
<PleasantDatePicker SelectedDate="{Binding SelectedDate, Mode=TwoWay}" />
```

```csharp
public DateTime? SelectedDate
{
    get => _selectedDate;
    set
    {
        _selectedDate = value;
        OnPropertyChanged();
    }
}
```

## Example

```xml
<StackPanel Spacing="12" Width="250">
    <TextBlock Text="Date Selection" FontWeight="Bold" />
    
    <PleasantDatePicker Watermark="Start date"
                       SelectedDate="{Binding StartDate}" />
    
    <PleasantDatePicker Watermark="End date"
                       SelectedDate="{Binding EndDate}"
                       MinYear="2020"
                       MaxYear="2030" />
</StackPanel>
```

## Notes

- The control uses a custom click area to trigger the calendar popup, allowing complete control over the visual styling
- The internal flyout button is kept in the visual tree (zero size) to maintain Avalonia's DatePicker functionality
