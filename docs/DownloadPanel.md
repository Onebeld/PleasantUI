# DownloadPanel

A panel that visualizes a multi-chunk download operation. Provides a tab strip, a progress bar, a chunk-progress canvas, a chunk details list, and action buttons (pause/resume, cancel).

## Basic usage

```xml
<DownloadPanel Progress="45"
              IsPaused="False"
              PauseResumeCommand="{Binding PauseResumeCommand}"
              CancelCommand="{Binding CancelCommand}">
    <DownloadPanel.Tabs>
        <x:String>Overview</x:String>
        <x:String>Details</x:String>
    </DownloadPanel.Tabs>
    
    <DownloadPanel.Chunks>
        <controls:ChunkInfo Start="0.0" End="0.5" Progress="1.0" />
        <controls:ChunkInfo Start="0.5" End="1.0" Progress="0.3" />
    </DownloadPanel.Chunks>
</DownloadPanel>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Progress` | `double` | — | Overall download progress [0..100] |
| `MergeProgress` | `double` | — | Merge/post-processing progress [0..100] |
| `IsPaused` | `bool` | — | Whether the download is paused |
| `IsMerging` | `bool` | — | Whether the download is in the merge/finalize phase |
| `ShowDetails` | `bool` | — | Whether the chunk details section is expanded |
| `IsPauseResumeEnabled` | `true` | `bool` | Whether the pause/resume button is enabled |
| `PauseResumeCommand` | `ICommand?` | — | Command invoked when pause/resume button is clicked |
| `CancelCommand` | `ICommand?` | — | Command invoked when cancel button is clicked |
| `ChunkProgressBrush` | `IBrush?` | — | Brush used to fill chunk progress rectangles |
| `ChunkBackgroundBrush` | `IBrush?` | — | Background brush of the chunk canvas |
| `ChunkItemTemplate` | `IDataTemplate?` | — | Data template for chunk list items |
| `TabItemTemplate` | `IDataTemplate?` | — | Data template for tab strip items |
| `SelectedTab` | `object?` | — | Currently selected tab item |
| `TabContent` | `object?` | — | Content shown in the tab content area |
| `Tabs` | `AvaloniaList<object>` | — | Collection of tab items shown in the tab strip |
| `Chunks` | `AvaloniaList<ChunkInfo>` | — | Collection of chunk data for canvas visualization and details list |

## Events

| Event | Description |
|---|---|
| `TabSelectionChanged` | Raised when the selected tab changes |

## Progress

Update the overall download progress:

```csharp
downloadPanel.Progress = 45.5; // 45.5%
```

When in merge phase, use `MergeProgress`:

```csharp
downloadPanel.IsMerging = true;
downloadPanel.MergeProgress = 75.0;
```

## Pause/Resume

Control pause state and wire up commands:

```csharp
downloadPanel.IsPaused = false;
downloadPanel.PauseResumeCommand = new RelayCommand(() =>
{
    downloadPanel.IsPaused = !downloadPanel.IsPaused;
    // Toggle download pause/resume
});
```

## Chunks

Populate the chunks collection to visualize download segments:

```csharp
downloadPanel.Chunks.Add(new ChunkInfo
{
    Start = 0.0,
    End = 0.25,
    Progress = 1.0,
    ProgressBrush = Brushes.Green
});

downloadPanel.Chunks.Add(new ChunkInfo
{
    Start = 0.25,
    End = 0.5,
    Progress = 0.5,
    ProgressBrush = Brushes.Orange
});
```

After updating chunk progress, call `RedrawChunkCanvas()`:

```csharp
chunk.Progress = 0.75;
downloadPanel.RedrawChunkCanvas();
```

## Custom chunk brushes

Customize the appearance of chunk progress:

```xml
<DownloadPanel ChunkProgressBrush="#4CAF50"
              ChunkBackgroundBrush="#E0E0E0" />
```

Or set per-chunk colors:

```csharp
chunk.ProgressBrush = new SolidColorBrush(Color.Parse("#4CAF50"));
```

## Tabs

Add tabs to the tab strip:

```xml
<DownloadPanel>
    <DownloadPanel.Tabs>
        <x:String>Overview</x:String>
        <x:String>Files</x:String>
        <x:String>Settings</x:String>
    </DownloadPanel.Tabs>
</DownloadPanel>
```

Handle tab selection changes:

```csharp
downloadPanel.TabSelectionChanged += (sender, e) =>
{
    var selectedTab = downloadPanel.SelectedTab;
    // Update tab content based on selection
};
```

## Details section

Toggle the chunk details section:

```xml
<DownloadPanel ShowDetails="True" />
```

```csharp
downloadPanel.ShowDetails = !downloadPanel.ShowDetails;
```

## Example

```csharp
// ViewModel
public class DownloadViewModel
{
    public DownloadPanel DownloadPanel { get; } = new();
    
    public ICommand PauseResumeCommand => new RelayCommand(() =>
    {
        DownloadPanel.IsPaused = !DownloadPanel.IsPaused;
        if (!DownloadPanel.IsPaused)
            ResumeDownload();
        else
            PauseDownload();
    });
    
    public ICommand CancelCommand => new RelayCommand(CancelDownload);
    
    private void UpdateProgress(double progress)
    {
        DownloadPanel.Progress = progress;
    }
    
    private void AddChunk(double start, double end)
    {
        DownloadPanel.Chunks.Add(new ChunkInfo
        {
            Start = start,
            End = end,
            Progress = 0.0
        });
    }
}
```
