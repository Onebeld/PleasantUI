# PleasantMenu

A flyout-style application menu with a title, optional info badges, a grid of large icon buttons (`PleasantMenuItem`), and a footer bar with small utility buttons (`PleasantMenuFooterItem`).

## Basic usage

```xml
<Button Content="Menu">
    <Button.Flyout>
        <Flyout>
            <PleasantMenu Title="My App" Columns="3">
                <PleasantMenu.Items>
                    <PleasantMenuItem Label="Open"
                                      Icon="{DynamicResource FolderOpenRegular}"
                                      Command="{Binding OpenCommand}" />
                    <PleasantMenuItem Label="Save"
                                      Icon="{DynamicResource SaveRegular}"
                                      Command="{Binding SaveCommand}" />
                    <PleasantMenuItem Label="Export"
                                      Icon="{DynamicResource ArrowExportRegular}"
                                      Command="{Binding ExportCommand}" />
                </PleasantMenu.Items>
                <PleasantMenu.FooterItems>
                    <PleasantMenuFooterItem Icon="{DynamicResource TuneRegular}"
                                           Command="{Binding SettingsCommand}"
                                           ToolTip="Settings" />
                    <PleasantMenuFooterItem Icon="{DynamicResource QuestionCircleRegular}"
                                           Command="{Binding HelpCommand}"
                                           ToolTip="Help"
                                           AlignRight="True" />
                </PleasantMenu.FooterItems>
            </PleasantMenu>
        </Flyout>
    </Button.Flyout>
</Button>
```

## PleasantMenu properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `""` | Text shown in the top-left header |
| `Columns` | `int` | `3` | Number of columns in the items grid |
| `Items` | `AvaloniaList<PleasantMenuItem>` | — | Large icon button items in the grid |
| `FooterItems` | `AvaloniaList<PleasantMenuFooterItem>` | — | Small icon buttons in the footer bar |
| `Badges` | `object?` | `null` | Arbitrary content (e.g. notification badges) shown in the header top-right |
| `ShowFooter` | `bool` | `true` | Whether the footer bar is visible |
| `ItemMinWidth` | `double` | `100` | Minimum width of each grid item button |
| `ItemMinHeight` | `double` | `70` | Minimum height of each grid item button |

## PleasantMenuItem properties

| Property | Type | Description |
|---|---|---|
| `Icon` | `Geometry?` | Icon geometry displayed in the button |
| `Label` | `string` | Text shown below the icon |
| `Command` | `ICommand?` | Command invoked on click |
| `CommandParameter` | `object?` | Parameter passed to `Command` |
| `SecondaryCommand` | `ICommand?` | When set, adds a chevron dropdown button next to the main button |
| `ToolTip` | `string?` | Tooltip text |
| `IsEnabled` | `bool` | Whether the button is enabled |

## PleasantMenuFooterItem properties

| Property | Type | Description |
|---|---|---|
| `Icon` | `Geometry?` | Icon geometry |
| `Command` | `ICommand?` | Command invoked on click |
| `CommandParameter` | `object?` | Parameter passed to `Command` |
| `ToolTip` | `string?` | Tooltip text |
| `IsEnabled` | `bool` | Whether the button is enabled |
| `AlignRight` | `bool` | When true, the button is right-aligned in the footer |

## Split button item

Set `SecondaryCommand` on a `PleasantMenuItem` to split it into a main action and a dropdown chevron:

```xml
<PleasantMenuItem Label="New"
                  Icon="{DynamicResource AddRegular}"
                  Command="{Binding NewFileCommand}"
                  SecondaryCommand="{Binding NewFromTemplateCommand}" />
```

## Badges in header

```xml
<PleasantMenu Title="Notifications">
    <PleasantMenu.Badges>
        <Border Background="{DynamicResource SystemFillColorCritical}"
                CornerRadius="999" Padding="4 2">
            <TextBlock Text="3" Foreground="White" FontSize="10" />
        </Border>
    </PleasantMenu.Badges>
</PleasantMenu>
```
