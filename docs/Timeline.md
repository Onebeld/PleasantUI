# Timeline / TimelineItem

Displays a vertical sequence of events along a connecting axis line. Supports four layout modes, custom icons, and five severity types.

## Basic usage

```xml
<Timeline>
    <TimelineItem Header="Project started"
                  Content="Initial commit pushed to repository."
                  Time="{x:Static sys:DateTime.Now}" />

    <TimelineItem Header="Beta release"
                  Content="First public beta available for testing."
                  Type="Success" />

    <TimelineItem Header="Critical bug found"
                  Content="Memory leak in the rendering pipeline."
                  Type="Error" />
</Timeline>
```

## Timeline properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Mode` | `TimelineDisplayMode` | `Left` | How items are positioned relative to the axis |
| `TimeFormat` | `string?` | `"yyyy-MM-dd HH:mm:ss"` | Format string applied to each item's `Time` |
| `IconMemberBinding` | `BindingBase?` | — | Binding path for the icon when using `ItemsSource` |
| `HeaderMemberBinding` | `BindingBase?` | — | Binding path for the header |
| `ContentMemberBinding` | `BindingBase?` | — | Binding path for the content |
| `TimeMemberBinding` | `BindingBase?` | — | Binding path for the timestamp |
| `IconTemplate` | `IDataTemplate?` | — | Data template for the icon node |
| `DescriptionTemplate` | `IDataTemplate?` | — | Data template for the content area |

## TimelineDisplayMode values

| Value | Description |
|---|---|
| `Left` | Content on the right, axis on the left |
| `Right` | Content on the left, axis on the right |
| `Center` | Axis in the center, time on the left, content on the right |
| `Alternate` | Items alternate left/right of the axis |

## TimelineItem properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Header` | `object?` | — | Title shown above the content |
| `Content` | `object?` | — | Body content of the event |
| `Time` | `DateTime` | — | Timestamp displayed next to the item |
| `TimeFormat` | `string?` | inherited | Override the format string for this item |
| `Type` | `TimelineItemType` | `Default` | Visual severity / style of the icon node |
| `Position` | `TimelineItemPosition` | `Left` | Overrides the item's position (set automatically by `Timeline.Mode`) |
| `Icon` | `object?` | — | Custom icon content for the axis node |
| `IconTemplate` | `IDataTemplate?` | — | Data template for the icon |

## TimelineItemType values

| Value | Description |
|---|---|
| `Default` | Neutral dot |
| `Success` | Green checkmark |
| `Warning` | Yellow warning |
| `Error` | Red error |
| `Info` | Blue info |

## Data-bound timeline

```xml
<Timeline ItemsSource="{Binding Events}"
          Mode="Alternate"
          HeaderMemberBinding="{Binding Title}"
          ContentMemberBinding="{Binding Description}"
          TimeMemberBinding="{Binding OccurredAt}"
          TimeFormat="MMM d, yyyy" />
```

## Custom icon

```xml
<TimelineItem Header="Deployed">
    <TimelineItem.Icon>
        <PathIcon Data="{DynamicResource RocketRegular}"
                  Width="14" Height="14" />
    </TimelineItem.Icon>
</TimelineItem>
```

## Center mode with timestamps

```xml
<Timeline Mode="Center" TimeFormat="HH:mm">
    <TimelineItem Header="Login"    Time="2024-01-15T09:00:00" />
    <TimelineItem Header="Purchase" Time="2024-01-15T09:14:32" Type="Success" />
    <TimelineItem Header="Logout"   Time="2024-01-15T09:45:00" />
</Timeline>
```
