using Avalonia.Controls;
using PleasantUI.Controls;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class PinCodePageView : LocalizedUserControl
{
    public PinCodePageView()
    {
        InitializeComponent();
        WireHandlers();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        // Unwire old handlers first to prevent memory leaks and stale references
        DefaultPin.Complete  -= OnPinComplete;
        DigitPin.Complete    -= OnPinComplete;
        LetterPin.Complete   -= OnPinComplete;
        PasswordPin.Complete -= OnPinComplete;
        SixPin.Complete      -= OnPinComplete;
        ClearButton.Click    -= OnClearClick;

        // Wire new handlers
        DefaultPin.Complete  += OnPinComplete;
        DigitPin.Complete    += OnPinComplete;
        LetterPin.Complete   += OnPinComplete;
        PasswordPin.Complete += OnPinComplete;
        SixPin.Complete      += OnPinComplete;
        ClearButton.Click    += OnClearClick;
    }

    private void OnPinComplete(object? sender, PinCodeCompleteEventArgs e)
    {
        ResultText.Text = e.Value;
    }

    private void OnClearClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
    }
}
