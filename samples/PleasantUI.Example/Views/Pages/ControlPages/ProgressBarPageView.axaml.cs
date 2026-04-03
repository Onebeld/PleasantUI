using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ProgressBarPageView : LocalizedUserControl
{
    private DispatcherTimer? _timer;
    private double _value;
    private StartButtonState _startState = StartButtonState.Ready;

    private ProgressBar _liveBar = null!;
    private RangeBase _liveRing = null!;
    private TextBlock _liveLabel = null!;
    private Button _startButton = null!;

    public ProgressBarPageView()
    {
        InitializeComponent();
        WireHandlers();
    }

    private enum StartButtonState
    {
        Ready,
        Running,
        Done
    }

    private void RefreshLocalizedRuntimeText()
    {
        // Only touches runtime-set strings (code-behind overwrites the XAML {Localize} binding).
        _startButton.Content = _startState switch
        {
            StartButtonState.Ready   => Localizer.Tr("Start"),
            StartButtonState.Running => Localizer.Tr("Running"),
            StartButtonState.Done    => Localizer.Tr("Done"),
            _ => _startButton.Content
        };
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
                _startState = StartButtonState.Done;
                RefreshLocalizedRuntimeText();
            }
        };
        _timer.Start();
        _startState = StartButtonState.Running;
        RefreshLocalizedRuntimeText();
        _startButton.IsEnabled = false;
    }

    private void Reset()
    {
        _timer?.Stop();
        _value = 0;
        _liveBar.Value = 0;
        _liveRing.Value = 0;
        _liveLabel.Text = "0%";
        _startState = StartButtonState.Ready;
        RefreshLocalizedRuntimeText();
        _startButton.IsEnabled = true;
    }
    // Complex constructor — don't re-run InitializeComponent

    protected override void ReinitializeComponent()
    {
        // Stop timer so it doesn't try to update the old visual tree.
        _timer?.Stop();
        _timer = null;

        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        _liveBar     = this.FindControl<ProgressBar>("LiveBar")!;
        _liveRing    = this.FindControl<RangeBase>("LiveRing")!;
        _liveLabel   = this.FindControl<TextBlock>("LiveLabel")!;
        _startButton = this.FindControl<Button>("StartButton")!;

        _startButton.Click += (_, _) => StartAnimation();
        this.FindControl<Button>("ResetButton")!.Click += (_, _) => Reset();

        Localizer.Instance.LocalizationChanged += _ => RefreshLocalizedRuntimeText();
        RefreshLocalizedRuntimeText();
    }
}
