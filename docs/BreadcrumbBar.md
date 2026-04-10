# BreadcrumbBar

Displays a breadcrumb trail that shows the path to the current location. Items that overflow the available width are collapsed into an ellipsis flyout.

## Basic usage

```xml
<BreadcrumbBar>
    <BreadcrumbBarItem Content="Home" />
    <BreadcrumbBarItem Content="Documents" />
    <BreadcrumbBarItem Content="Projects" />
    <BreadcrumbBarItem Content="Current" />
</BreadcrumbBar>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsLastItemClickEnabled` | `bool` | `false` | Whether the last (current) item can be clicked |

## Events

| Event | Description |
|---|---|
| `ItemClicked` | Raised when the user clicks a breadcrumb item |

## Data binding

Bind to a collection of path items:

```xml
<BreadcrumbBar ItemsSource="{Binding NavigationPath}">
    <BreadcrumbBar.ItemTemplate>
        <DataTemplate>
            <BreadcrumbBarItem Content="{Binding}" />
        </DataTemplate>
    </BreadcrumbBar.ItemTemplate>
</BreadcrumbBar>
```

## Handling clicks

Respond to breadcrumb item clicks:

```csharp
breadcrumbBar.ItemClicked += (sender, e) =>
{
    var clickedIndex = e.Index;
    var clickedContent = e.Content;
    // Navigate to the selected location
};
```

## Last item behavior

By default, the last item (current location) is rendered as plain text and cannot be clicked. Enable clicking if needed:

```xml
<BreadcrumbBar IsLastItemClickEnabled="True" />
```

## Overflow behavior

When items don't fit in the available width, they are automatically collapsed into an ellipsis (...) menu. Clicking the ellipsis shows a flyout with the hidden items.

## Keyboard navigation

Use arrow keys to navigate between breadcrumb items:
- Left/Right arrows move focus between items
- Enter activates the focused item
