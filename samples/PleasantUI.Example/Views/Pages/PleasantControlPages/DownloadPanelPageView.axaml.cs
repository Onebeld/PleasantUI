using Avalonia.Interactivity;
using PleasantUI.ToolKit.Controls;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class DownloadPanelPageView : LocalizedUserControl
{
    private bool _paused;
    private bool _merging;

    public DownloadPanelPageView()
    {
        InitializeComponent();
        WireHandlers();
        DownloadPanelControl.Tabs.AddRange(["Status", "Speed Limiter", "Options"]);
        DownloadPanelControl.SelectedTab = "Status";
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        TickBtn.Click  -= OnTick;
        PauseBtn.Click -= OnPause;
        MergeBtn.Click -= OnMerge;
        ResetBtn.Click -= OnReset;

        TickBtn.Click  += OnTick;
        PauseBtn.Click += OnPause;
        MergeBtn.Click += OnMerge;
        ResetBtn.Click += OnReset;
    }

    private void OnTick(object? s, RoutedEventArgs e)
    {
        if (_paused) return;

        foreach (var chunk in DownloadPanelControl.Chunks)
        {
            chunk.Progress = Math.Min(1.0, chunk.Progress + 0.08);
            double mb = chunk.Progress * 32;
            chunk.DownloadedSize = $"{mb:F1} MB";
            chunk.Status = chunk.Progress >= 1.0 ? ChunkStatus.Completed : ChunkStatus.Downloading;
        }

        double overall = DownloadPanelControl.Chunks.Average(c => c.Progress) * 100;
        DownloadPanelControl.Progress = overall;
        DownloadPanelControl.RedrawChunkCanvas();
    }

    private void OnPause(object? s, RoutedEventArgs e)
    {
        _paused = !_paused;
        DownloadPanelControl.IsPaused = _paused;
    }

    private void OnMerge(object? s, RoutedEventArgs e)
    {
        _merging = !_merging;
        DownloadPanelControl.IsMerging = _merging;
        if (_merging) DownloadPanelControl.MergeProgress = 55;
    }

    private void OnReset(object? s, RoutedEventArgs e)
    {
        _paused  = false;
        _merging = false;
        DownloadPanelControl.IsPaused      = false;
        DownloadPanelControl.IsMerging     = false;
        DownloadPanelControl.Progress      = 0;
        DownloadPanelControl.MergeProgress = 0;

        foreach (var chunk in DownloadPanelControl.Chunks)
        {
            chunk.Progress       = 0;
            chunk.DownloadedSize = "0 MB";
            chunk.Status        = ChunkStatus.Waiting;
        }
        DownloadPanelControl.RedrawChunkCanvas();
    }
}
