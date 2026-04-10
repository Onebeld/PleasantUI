# LogViewerPanel

A slide-in panel that displays a filterable, searchable log of `LogEntry` items. Supports auto-scroll, level filtering, source filtering, copy, and clear operations.

## Basic usage

```xml
<LogViewerPanel IsOpen="True"
               Title="Activity Log"
               PanelWidth="420" />
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `IsOpen` | `bool` | — | Whether the panel is visible |
| `Title` | `string` | `"Activity Log"` | Panel title |
| `AutoScroll` | `bool` | `true` | Whether the list auto-scrolls to new entries |
| `ShowDebugEntries` | `bool` | `false` | Whether Debug-level entries are shown |
| `SearchText` | `string?` | — | Text used to filter entries by message content |
| `SelectedLevelFilter` | `LogLevel?` | — | Level filter (null = show all levels) |
| `SelectedSourceFilter` | `string?` | — | Source filter (null = show all sources) |
| `MaxEntries` | `int` | `5000` | Maximum number of entries to retain |
| `PanelWidth` | `double` | `420` | Width of the slide-in panel |
| `Entries` | `ObservableCollection<LogEntry>` | — | Full collection of log entries |
| `FilteredEntries` | `AvaloniaList<LogEntry>` | — | Filtered view of entries shown in the list |
| `LevelFilterOptions` | `LogLevel?[]` | — | Available log level filter options |
| `SourceFilterOptions` | `AvaloniaList<string?>` | — | Available source filter options |

## Events

| Event | Description |
|---|---|
| `Closed` | Raised when the panel is closed |
| `CopyAllRequested` | Raised when the user requests all entries to be copied |
| `CopyEntryRequested` | Raised when the user requests a single entry to be copied |

## Opening/closing

Control panel visibility:

```xml
<LogViewerPanel IsOpen="{Binding IsLogViewerOpen}" />
```

```csharp
logViewerPanel.Open();
logViewerPanel.Close();
```

## Adding entries

Append log entries to the panel:

```csharp
// Simple message
logViewerPanel.Append(LogLevel.Information, "Operation completed");

// Full entry
logViewerPanel.Append(new LogEntry
{
    Level = LogLevel.Error,
    Message = "Failed to connect",
    Source = "NetworkService",
    Details = "Timeout after 30 seconds"
});
```

## Clearing entries

Remove all entries:

```csharp
logViewerPanel.Clear();
```

## Filtering

Filter by log level:

```csharp
logViewerPanel.SelectedLevelFilter = LogLevel.Error;
```

Filter by source:

```csharp
logViewerPanel.SelectedSourceFilter = "NetworkService";
```

Filter by text search:

```xml
<LogViewerPanel SearchText="{Binding SearchText}" />
```

## Debug entries

Control whether debug entries are shown:

```xml
<LogViewerPanel ShowDebugEntries="True" />
```

## Auto-scroll

Control automatic scrolling to new entries:

```xml
<LogViewerPanel AutoScroll="True" />
```

## Max entries

Set the maximum number of entries to retain (older entries are removed):

```xml
<LogViewerPanel MaxEntries="10000" />
```

## Panel width

Adjust the width of the slide-in panel:

```xml
<LogViewerPanel PanelWidth="600" />
```

## Copy handling

Handle copy requests:

```csharp
logViewerPanel.CopyAllRequested += (sender, e) =>
{
    var text = string.Join("\n", logViewerPanel.FilteredEntries.Select(x => x.ToString()));
    Clipboard.SetTextAsync(text);
};

logViewerPanel.CopyEntryRequested += (sender, entry) =>
{
    Clipboard.SetTextAsync(entry.ToString());
};
```

## Example

```csharp
// In your ViewModel
public class MainViewModel
{
    public LogViewerPanel LogViewer { get; } = new LogViewerPanel
    {
        Title = "Application Logs",
        MaxEntries = 10000
    };
    
    private void LogOperation(string message, LogLevel level = LogLevel.Information)
    {
        LogViewer.Append(level, message, Source: "MainViewModel");
    }
}
```

## Pseudo-classes

The control applies pseudo-classes based on state:
- `:open` - Applied when the panel is open
- `:hasEntries` - Applied when there are filtered entries
- `:hasFilter` - Applied when a filter is active

Use these for styling:

```xml
<Style Selector="LogViewerPanel:hasEntries">
    <Setter Property="Background" Value="White" />
</Style>
```
