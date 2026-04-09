# DashboardCard

A card control with a structured header (title + optional toolbar) and a scrollable content area. Designed for dashboard panels, metric cards, and data displays.

## Basic usage

```xml
<DashboardCard Header="Sales Overview">
    <StackPanel Spacing="10">
        <TextBlock Text="$12,345" FontSize="24" FontWeight="Bold" />
        <TextBlock Text="+15% from last month" Foreground="Green" />
    </StackPanel>
</DashboardCard>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Header` | `string?` | — | Title shown in the card header (inherited from `HeaderedContentControl`) |
| `Subtitle` | `string?` | — | Subtitle shown below the header |
| `HeaderContent` | `object?` | — | Arbitrary content placed in the header's right slot |
| `HeaderContentTemplate` | `IDataTemplate?` | — | Data template for `HeaderContent` |
| `FooterContent` | `object?` | — | Content shown in the card footer |
| `FooterContentTemplate` | `IDataTemplate?` | — | Data template for `FooterContent` |
| `ContentPadding` | `Thickness` | `16` | Padding applied to the content area |
| `ShowScrollViewer` | `bool` | `true` | Whether the content area is wrapped in a `SmoothScrollViewer` |
| `ToolbarItems` | `AvaloniaList<object>` | — | Collection of toolbar items shown in the header's right slot |

## Subtitle

Add a subtitle below the header:

```xml
<DashboardCard Header="Revenue"
              Subtitle="Monthly report">
    <TextBlock Text="$45,000" />
</DashboardCard>
```

## Toolbar items

Add toolbar buttons or other controls to the header:

```xml
<DashboardCard Header="Users">
    <DashboardCard.ToolbarItems>
        <Button Content="Add" />
        <Button Content="Refresh" />
    </DashboardCard.ToolbarItems>
    
    <ItemsControl ItemsSource="{Binding Users}" />
</DashboardCard>
```

## Header content

Place custom content in the header's right slot:

```xml
<DashboardCard Header="Settings">
    <DashboardCard.HeaderContent>
        <CheckBox Content="Enable" />
    </DashboardCard.HeaderContent>
    
    <!-- Card content -->
</DashboardCard>
```

## Footer content

Add a footer with action buttons or summary information:

```xml
<DashboardCard Header="Tasks">
    <ItemsControl ItemsSource="{Binding Tasks}" />
    
    <DashboardCard.FooterContent>
        <Border Background="{DynamicResource ControlFillColor1}"
                Padding="12">
            <TextBlock Text="5 pending tasks" />
        </Border>
    </DashboardCard.FooterContent>
</DashboardCard>
```

## Content padding

Adjust the padding inside the content area:

```xml
<DashboardCard ContentPadding="24" />
```

## Scroll viewer

By default, content is wrapped in a `SmoothScrollViewer`. Disable this if you want custom scrolling behavior:

```xml
<DashboardCard ShowScrollViewer="False">
    <!-- Custom scrollable content -->
</DashboardCard>
```

## Complex example

```xml
<DashboardCard Header="Project Status"
              Subtitle="Q4 2024">
    <DashboardCard.ToolbarItems>
        <Button Content="Edit" />
        <Button Content="Delete" />
    </DashboardCard.ToolbarItems>
    
    <StackPanel Spacing="12">
        <ProgressBar Value="75" Maximum="100" />
        <TextBlock Text="75% complete" />
    </StackPanel>
    
    <DashboardCard.FooterContent>
        <StackPanel Orientation="Horizontal"
                    HorizontalAlignment="Right"
                    Spacing="8">
            <Button Content="Cancel" />
            <Button Content="Save" Theme="{DynamicResource AccentButtonTheme}" />
        </StackPanel>
    </DashboardCard.FooterContent>
</DashboardCard>
```
