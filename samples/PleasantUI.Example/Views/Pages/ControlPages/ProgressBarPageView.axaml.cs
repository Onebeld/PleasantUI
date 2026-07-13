using Avalonia.Controls;
using Avalonia.Threading;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class ProgressBarPageView : LocalizedUserControl
{
    private DispatcherTimer? _timer;
    private double _value;
    private StartButtonState _startState = StartButtonState.Ready;

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
        StartButton.Content = _startState switch
        {
            StartButtonState.Ready   => Localizer.Tr("Start"),
            StartButtonState.Running => Localizer.Tr("Running"),
            StartButtonState.Done    => Localizer.Tr("Done"),
            _ => StartButton.Content
        };
    }

    private void StartAnimation()
    {
        if (_timer?.IsEnabled == true) return;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(40) };
        _timer.Tick += (_, _) =>
        {
            _value += 1;
            LiveBar.Value = _value;
            LiveRing.Value = _value;
            LiveLabel.Text = $"{(int)_value}%";

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
        StartButton.IsEnabled = false;
    }

    private void Reset()
    {
        _timer?.Stop();
        _value = 0;
        LiveBar.Value = 0;
        LiveRing.Value = 0;
        LiveLabel.Text = "0%";
        _startState = StartButtonState.Ready;
        RefreshLocalizedRuntimeText();
        StartButton.IsEnabled = true;
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
        StartButton.Click += (_, _) => StartAnimation();
        this.FindControl<Button>("ResetButton")!.Click += (_, _) => Reset();

        Localizer.Instance.LocalizationChanged += _ => RefreshLocalizedRuntimeText();
        RefreshLocalizedRuntimeText();
    }
}
