using Avalonia.Controls;
using PleasantUI.Controls;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class PinCodePageView : LocalizedUserControl
{
    private EventHandler<PinCodeCompleteEventArgs>? _onComplete;

    public PinCodePageView()
    {
        InitializeComponent();
        WireHandlers();
    }
    // Complex constructor — don't re-run InitializeComponent

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        _onComplete = (_, e) => { ResultText.Text = e.Value; };

        DefaultPin.Complete  += _onComplete;
        DigitPin.Complete    += _onComplete;
        LetterPin.Complete   += _onComplete;
        PasswordPin.Complete += _onComplete;
        SixPin.Complete      += _onComplete;

        ClearButton.Click += (_, _) =>
        {
            foreach (var pin in new[] { DefaultPin, DigitPin, LetterPin, PasswordPin, SixPin })
            {
                for (int i = 0; i < pin.Count; i++)
                    pin.Digits[i] = string.Empty;

                if (pin.FindControl<ItemsControl>("PART_ItemsControl") is { } ic)
                    for (int i = 0; i < pin.Count; i++)
                        if (ic.ContainerFromIndex(i) is PinCodeItem cell)
                            cell.Text = string.Empty;
            }
            ResultText.Text = "—";
        };
    }
}
