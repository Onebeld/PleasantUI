using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ProgressBarPageView : UserControl
{
    private DispatcherTimer? _timer;
    private double _value;

    // stored after InitializeComponent so we don't rely on source-gen fields
    private ProgressBar _liveBar = null!;
    private RangeBase _liveRing = null!;
    private TextBlock _liveLabel = null!;
    private Button _startButton = null!;

    public ProgressBarPageView()
    {
        InitializeComponent();

        _liveBar     = this.FindControl<ProgressBar>("LiveBar")!;
        _liveRing    = this.FindControl<RangeBase>("LiveRing")!;
        _liveLabel   = this.FindControl<TextBlock>("LiveLabel")!;
        _startButton = this.FindControl<Button>("StartButton")!;

        _startButton.Click += (_, _) => StartAnimation();
        this.FindControl<Button>("ResetButton")!.Click += (_, _) => Reset();
    }

    private void StartAnimation()
    {
        if (_timer?.IsEnabled == true) return;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
        _timer.Tick += (_, _) =>
        {
            _value += 1;
            _liveBar.Value = _value;
            _liveRing.Value = _value;
            _liveLabel.Text = $"{(int)_value}%";

            if (_value >= 100)
            {
                _timer.Stop();
                _startButton.Content = "Done";
            }
        };
        _timer.Start();
        _startButton.Content = "Running…";
        _startButton.IsEnabled = false;
    }

    private void Reset()
    {
        _timer?.Stop();
        _value = 0;
        _liveBar.Value = 0;
        _liveRing.Value = 0;
        _liveLabel.Text = "0%";
        _startButton.Content = "Start";
        _startButton.IsEnabled = true;
    }
}
