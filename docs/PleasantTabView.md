# PleasantTabView / PleasantTabItem

A Chromium-style tab strip with scrollable tabs, an add button, and closable/editable tab items.

## Basic usage

```xml
<PleasantTabView>
    <PleasantTabItem Header="Tab 1">
        <TextBlock Text="Content of tab 1" Margin="20" />
    </PleasantTabItem>
    <PleasantTabItem Header="Tab 2">
        <TextBlock Text="Content of tab 2" Margin="20" />
    </PleasantTabItem>
</PleasantTabView>
```

## PleasantTabView properties

| Property | Type | Default | Description |
|---|---|---|---|
| `AdderButtonIsVisible` | `bool` | `true` | Shows the + button to add new tabs |
| `MaxWidthOfItemsPresenter` | `double` | `∞` | Caps the tab strip width |
| `MarginType` | `None` / `Little` / `Extended` | `None` | Margin applied around the content area |

## PleasantTabView events

| Event | Description |
|---|---|
| `ClickOnAddingButton` | Fired when the + button is clicked — add a new tab here |

## PleasantTabItem properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsClosable` | `bool` | `true` | Shows the × close button on the tab |
| `IsEditedIndicator` | `bool` | `false` | Shows a dot indicator (e.g. unsaved changes) |

## PleasantTabItem events

| Event | Description |
|---|---|
| `Closing` | Fired when the tab is about to close |
| `CloseButtonClick` | Fired when the × button is clicked |

## Dynamic tab management

```csharp
// Add a tab when + is clicked
tabView.ClickOnAddingButton += (_, _) =>
{
    tabView.Items.Add(new PleasantTabItem
    {
        Header  = $"Tab {tabView.Items.Count + 1}",
        Content = new MyTabContent()
    });
};

// Prevent closing a specific tab
tab.Closing += (_, e) =>
{
    if (HasUnsavedChanges())
        e.Handled = true; // cancel close
};
```

## Programmatic close

```csharp
tab.CloseCore(); // removes the tab from its parent PleasantTabView
```

## Data-bound tabs

```xml
<PleasantTabView ItemsSource="{Binding Tabs}">
    <PleasantTabView.ItemTemplate>
        <DataTemplate DataType="vm:TabViewModel">
            <PleasantTabItem Header="{Binding Title}"
                             IsEditedIndicator="{Binding IsModified}">
                <views:TabContentView DataContext="{Binding}" />
            </PleasantTabItem>
        </DataTemplate>
    </PleasantTabView.ItemTemplate>
</PleasantTabView>
```
