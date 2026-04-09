# PleasantFlyout

A custom flyout control that provides additional functionality and styling options over the standard Avalonia `Flyout`. Uses a custom presenter for enhanced theming and behavior.

## Basic usage

```xml
<Button Content="Show Flyout">
    <Button.Flyout>
        <PleasantFlyout>
            <StackPanel Spacing="8" Padding="16">
                <TextBlock Text="Flyout content" />
                <Button Content="Action" />
            </StackPanel>
        </PleasantFlyout>
    </Button.Flyout>
</Button>
```

## Properties

Inherits all properties from `Avalonia.Controls.Flyout`:
- `Content` - The content to display in the flyout
- `IsOpen` - Whether the flyout is currently open
- `Placement` - Where the flyout should be positioned relative to its target
- `ShowMode` - How the flyout should be shown
- And other standard flyout properties

## Placement

Control where the flyout appears:

```xml
<PleasantFlyout Placement="Bottom">
    <!-- Content -->
</PleasantFlyout>
```

Available placements: `Top`, `Bottom`, `Left`, `Right`, `TopEdgeAligned`, `BottomEdgeAligned`, etc.

## Show mode

Control how the flyout is shown:

```xml
<!-- Standard mode (default) -->
<PleasantFlyout ShowMode="Standard" />

<!-- transient mode -->
<PleasantFlyout ShowMode="Transient" />
```

## Programmatic control

```csharp
var flyout = new PleasantFlyout
{
    Content = new TextBlock { Text = "Hello" }
};

flyout.ShowAt(targetControl);
flyout.Hide();
```

## Example

```xml
<Button Content="Options">
    <Button.Flyout>
        <PleasantFlyout Placement="Bottom">
            <StackPanel Width="200" Spacing="8">
                <MenuItem Header="Copy" />
                <MenuItem Header="Paste" />
                <Separator />
                <MenuItem Header="Settings" />
            </StackPanel>
        </PleasantFlyout>
    </Button.Flyout>
</Button>
```

## Notes

- `PleasantFlyout` uses a custom presenter (`PleasantFlyoutPresenter`) for consistent theming with the PleasantUI design system
- All standard Avalonia flyout functionality is preserved
