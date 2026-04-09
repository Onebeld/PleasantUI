# PleasantView

A content control that serves as the main view container for PleasantUI applications. Supports modal windows and a snackbar queue manager for displaying notifications.

## Basic usage

```xml
<controls:PleasantView>
    <StackPanel Spacing="8">
        <TextBlock Text="Main content" />
        <Button Content="Show Snackbar" />
    </StackPanel>
</controls:PleasantView>
```

## Properties

| Property | Type | Description |
|---|---|---|
| `SnackbarQueueManager` | `SnackbarQueueManager<PleasantSnackbar>` | Manager for displaying snackbar notifications |
| `ModalWindows` | `AvaloniaList<PleasantPopupElement>` | Collection of modal windows |

## Snackbar notifications

Display snackbar notifications using the `SnackbarQueueManager`:

```csharp
var snackbar = new PleasantSnackbar
{
    Title = "Success",
    Content = "Operation completed successfully"
};

pleasantView.SnackbarQueueManager.Enqueue(snackbar);
```

## Modal windows

Show modal windows by adding them to the `ModalWindows` collection:

```csharp
var dialog = new ContentDialog
{
    Title = "Confirm",
    Content = "Are you sure?"
};

dialog.ShowAsync(window);
```

The modal window automatically registers itself with the view.

## Content

The control inherits from `ContentControl`, so you can set any content:

```xml
<controls:PleasantView Content="{Binding MainContent}" />
```

Or use content templates:

```xml
<controls:PleasantView>
    <controls:PleasantView.ContentTemplate>
        <DataTemplate>
            <Border Background="White" Padding="16">
                <ContentPresenter Content="{Binding}" />
            </Border>
        </DataTemplate>
    </controls:PleasantView.ContentTemplate>
</controls:PleasantView>
```

## Styling

Style the view like any other `ContentControl`:

```xml
<Style Selector="controls:PleasantView">
    <Setter Property="Background" Value="#F5F5F5" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="16" />
</Style>
```

## Template parts

The control template must provide:
- `PART_VisualLayerManager` - Visual layer manager for modal windows and snackbars
- `PART_ContentPresenter` - Content presenter for the main content

## Notes

- `PleasantView` implements `IPleasantWindow` for integration with the PleasantUI overlay system
- The default template includes a visual layer manager for handling modal windows and snackbars
- Use this control as the root content container in PleasantUI applications
