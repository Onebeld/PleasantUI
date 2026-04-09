# ModalWindowHost

Hosts a modal window as an overlay within the visual tree. Used by PleasantUI controls like `ContentDialog`, `CrashReportDialog`, and `StepDialog` to display modal overlays.

## Basic usage

The `ModalWindowHost` is typically created and managed by PleasantUI modal controls automatically:

```csharp
// ContentDialog creates its own host internally
await dialog.ShowAsync(window);
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `HorizontalContentAlignment` | `HorizontalAlignment` | `Center` | Horizontal alignment of the content within the host |
| `VerticalContentAlignment` | `VerticalAlignment` | `Center` | Vertical alignment of the content within the host |

## Content alignment

Control how the modal content is positioned:

```xml
<ModalWindowHost HorizontalContentAlignment="Center"
                VerticalContentAlignment="Center">
    <!-- Modal content -->
</ModalWindowHost>
```

Available alignments:
- `Center` (default)
- `Stretch`
- `Left` / `Right`
- `Top` / `Bottom`

## Pointer handling

The host automatically handles all pointer events and marks them as handled to prevent interaction with the underlying content while the modal is visible. This includes:
- `PointerEntered`
- `PointerExited`
- `PointerPressed`
- `PointerReleased`
- `PointerCaptureLost`
- `PointerMoved`
- `PointerWheelChanged`

## Root bounds tracking

The host automatically tracks changes in the root control's bounds and invalidates its measure when the root size changes. This ensures the modal overlay always fills the available space.

## Notes

- This control is typically created and managed by PleasantUI modal controls, not instantiated directly
- The control has no background by default
- All pointer events are automatically handled to prevent interaction with underlying content
- The host measures to the size of the top-level client size or the content size
