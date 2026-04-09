# TerminalPanel

A terminal-style panel with a scrollable output area and a command input line. Maintains a configurable output buffer and raises events for command submission.

## Basic usage

```xml
<TerminalPanel Title="Terminal"
              Prompt="$"
              IsRunning="True" />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Title` | `string` | `"Terminal"` | Panel title shown in the header |
| `IsRunning` | `bool` | — | Whether the terminal session is active |
| `StatusText` | `string?` | — | Status text shown in the header |
| `StatusColor` | `IBrush?` | — | Brush used for the status indicator dot |
| `InputText` | `string?` | — | Current input line text |
| `OutputText` | `string?` | — | Full output text displayed in the terminal |
| `Prompt` | `string` | `"$"` | Prompt string shown before the input box |
| `MaxOutputLines` | `int` | `5000` | Maximum number of output lines to retain |
| `ShowCloseButton` | `bool` | `true` | Whether the close button is visible |

## Events

| Event | Description |
|---|---|
| `CommandSubmitted` | Raised when the user submits a command (presses Enter) |
| `CloseRequested` | Raised when the user clicks the close button |

## Appending output

Add text to the terminal output:

```csharp
terminalPanel.AppendOutput("Hello, World!\n");
terminalPanel.AppendOutput("Command completed successfully.\n");
```

## Clearing output

Clear all output:

```csharp
terminalPanel.ClearOutput();
```

## Command handling

Handle command submissions:

```csharp
terminalPanel.CommandSubmitted += (sender, command) =>
{
    // Process the command
    terminalPanel.AppendOutput($"Executing: {command}\n");
    
    // Simulate command execution
    var result = ProcessCommand(command);
    terminalPanel.AppendOutput($"{result}\n");
};
```

## Prompt

Customize the prompt string:

```xml
<TerminalPanel Prompt=">" />
```

```csharp
terminalPanel.Prompt = "#";
```

## Status

Set status text and color:

```xml
<TerminalPanel StatusText="Connected"
              StatusColor="Green" />
```

```csharp
terminalPanel.StatusText = "Disconnected";
terminalPanel.StatusColor = Brushes.Red;
```

## Running state

Control whether the terminal is active:

```xml
<TerminalPanel IsRunning="True" />
```

```csharp
terminalPanel.IsRunning = false;
```

## Max output lines

Set the maximum number of lines to retain (older lines are removed):

```xml
<TerminalPanel MaxOutputLines="10000" />
```

## Close button

Control the visibility of the close button:

```xml
<TerminalPanel ShowCloseButton="False" />
```

## Focus input

Focus the input box programmatically:

```csharp
terminalPanel.FocusInput();
```

## Example

```csharp
var terminal = new TerminalPanel
{
    Title = "Command Line",
    Prompt = ">",
    IsRunning = true
};

terminal.CommandSubmitted += (sender, cmd) =>
{
    terminal.AppendOutput($"{terminal.Prompt} {cmd}\n");
    
    switch (cmd.ToLower())
    {
        case "help":
            terminal.AppendOutput("Available commands: help, clear, echo [text]\n");
            break;
        case "clear":
            terminal.ClearOutput();
            break;
        default:
            terminal.AppendOutput($"Unknown command: {cmd}\n");
            break;
    }
};
```

## Pseudo-classes

The control applies pseudo-classes based on state:
- `:running` - Applied when the terminal is running
- `:hasOutput` - Applied when there is output text

## Template parts

The control template must provide:
- `PART_OutputBox` - TextBox that displays output
- `PART_InputBox` - TextBox for command input
- `PART_ClearButton` - Button to clear output
- `PART_CloseButton` - Button to close the panel
- `PART_ScrollViewer` - SmoothScrollViewer for output scrolling
