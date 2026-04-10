# PleasantMenuFlyout

A custom menu flyout control that provides additional functionality and styling options over the standard Avalonia `MenuFlyout`. Uses a custom presenter for enhanced theming and behavior with the PleasantUI design system.

## Basic usage

```xml
<Button Content="Show Menu">
    <Button.Flyout>
        <flyout:PleasantMenuFlyout>
            <MenuItem Header="Copy" />
            <MenuItem Header="Paste" />
            <Separator />
            <MenuItem Header="Settings" />
        </flyout:PleasantMenuFlyout>
    </Button.Flyout>
</Button>
```

## Properties

Inherits all properties from `Avalonia.Controls.MenuFlyout`:
- `Items` - The menu items collection
- `IsOpen` - Whether the flyout is currently open
- `Placement` - Where the flyout should be positioned relative to its target
- `ShowMode` - How the flyout should be shown
- `ItemTemplate` - Data template for menu items
- `ItemContainerTheme` - Theme for item containers
- And other standard MenuFlyout properties

## Placement

Control where the menu appears:

```xml
<PleasantMenuFlyout Placement="Bottom">
    <!-- Menu items -->
</PleasantMenuFlyout>
```

## Show mode

Control how the flyout is shown:

```xml
<PleasantMenuFlyout ShowMode="Standard" />
```

## Programmatic control

```csharp
var flyout = new PleasantMenuFlyout
{
    Items = new AvaloniaList<object>
    {
        new MenuItem { Header = "Copy" },
        new MenuItem { Header = "Paste" }
    }
};

flyout.ShowAt(targetControl);
flyout.Hide();
```

## Example

```xml
<Button Content="File">
    <Button.Flyout>
        <flyout:PleasantMenuFlyout>
            <MenuItem Header="New" InputGesture="Ctrl+N" />
            <MenuItem Header="Open" InputGesture="Ctrl+O" />
            <Separator />
            <MenuItem Header="Save" InputGesture="Ctrl+S" />
            <MenuItem Header="Save As" InputGesture="Ctrl+Shift+S" />
            <Separator />
            <MenuItem Header="Exit" />
        </flyout:PleasantMenuFlyout>
    </Button.Flyout>
</Button>
```

## Notes

- `PleasantMenuFlyout` uses a custom presenter (`PleasantMenuFlyoutPresenter`) for consistent theming with the PleasantUI design system
- All standard Avalonia MenuFlyout functionality is preserved
