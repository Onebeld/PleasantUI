# PleasantMiniWindow

A lightweight floating window with a minimal title bar, pin-to-top button, and optional hide button. Suitable for tool palettes, floating panels, and utility windows.

## Basic usage

```xml
<PleasantMiniWindow xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    x:Class="YourApp.MyToolWindow"
                    Title="Tools"
                    Width="300" Height="400"
                    SizeToContent="Height">
    <!-- content -->
</PleasantMiniWindow>
```

```csharp
public partial class MyToolWindow : PleasantMiniWindow
{
    public MyToolWindow() => InitializeComponent();
}
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `EnableCustomTitleBar` | `bool` | from settings | Replaces the OS title bar with the minimal Fluent one |
| `EnableBlur` | `bool` | from settings | Acrylic/blur background |
| `ShowPinButton` | `bool` | `true` | Shows the pin (always-on-top) toggle button |
| `ShowHiddenButton` | `bool` | `false` | Shows a minimize button |

## Showing as a dialog

```csharp
var toolWindow = new MyToolWindow();
await toolWindow.ShowDialog(parentWindow);
```

## Showing non-modally

```csharp
var toolWindow = new MyToolWindow();
toolWindow.Show(parentWindow);
```

## Differences from PleasantWindow

| Feature | PleasantWindow | PleasantMiniWindow |
|---|---|---|
| Full title bar with subtitle, icon, custom content | ✓ | — |
| Minimal drag handle + pin button | — | ✓ |
| Splash screen support | ✓ | — |
| Blur | ✓ | ✓ |
| Modal dialogs / snackbars | ✓ | ✓ |
