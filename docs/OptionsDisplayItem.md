# OptionsDisplayItem

A settings-style row control with a header, optional description, icon, action button slot, navigation chevron, and expandable content area. Used throughout the example app for settings pages.

## Properties

| Property | Type | Description |
|---|---|---|
| `Header` | `string` | Primary label |
| `Description` | `string` | Secondary text below the header |
| `Icon` | `Geometry` | Icon shown on the left |
| `Navigates` | `bool` | Shows a right chevron — makes the row look tappable |
| `NavigationCommand` | `ICommand?` | Command invoked when the row is clicked (when `Navigates=True`) |
| `ActionButton` | `Control` | Control placed on the right (toggle, combobox, button, etc.) |
| `Expands` | `bool` | Reveals hidden `Content` when clicked |
| `IsExpanded` | `bool` | Current expanded state |
| `Content` | `object?` | Content shown when expanded |

## Simple row

```xml
<OptionsDisplayItem Header="Notifications"
                    Description="Manage notification preferences"
                    Icon="{DynamicResource BellOutline}" />
```

## With action control

```xml
<OptionsDisplayItem Header="Dark mode"
                    Icon="{DynamicResource WeatherNight}">
    <OptionsDisplayItem.ActionButton>
        <ToggleSwitch IsChecked="{Binding IsDarkMode}" />
    </OptionsDisplayItem.ActionButton>
</OptionsDisplayItem>

<OptionsDisplayItem Header="Language">
    <OptionsDisplayItem.ActionButton>
        <ComboBox ItemsSource="{Binding Languages}"
                  SelectedItem="{Binding SelectedLanguage}"
                  MinWidth="150" />
    </OptionsDisplayItem.ActionButton>
</OptionsDisplayItem>
```

## Navigation row

```xml
<OptionsDisplayItem Header="Account"
                    Description="Manage your account settings"
                    Icon="{DynamicResource AccountOutline}"
                    Navigates="True"
                    NavigationCommand="{Binding GoToAccountCommand}" />
```

## Expandable row

```xml
<OptionsDisplayItem Header="Advanced options"
                    Description="Click to expand"
                    Expands="True">
    <OptionsDisplayItem.Content>
        <StackPanel Spacing="8" Margin="0 8 0 0">
            <CheckBox Content="Enable feature A" IsChecked="{Binding FeatureA}" />
            <CheckBox Content="Enable feature B" IsChecked="{Binding FeatureB}" />
        </StackPanel>
    </OptionsDisplayItem.Content>
</OptionsDisplayItem>
```

## Events

```csharp
item.AddHandler(OptionsDisplayItem.NavigationRequestedEvent, (_, _) =>
{
    // navigate somewhere
});
```
