using Avalonia.Controls;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.ControlPages;

public partial class PinCodePageView : UserControl
{
    public PinCodePageView()
    {
        InitializeComponent();

        void OnComplete(object? sender, PinCodeCompleteEventArgs e)
        {
            ResultText.Text = e.Value;
        }

        DefaultPin.Complete  += OnComplete;
        DigitPin.Complete    += OnComplete;
        LetterPin.Complete   += OnComplete;
        PasswordPin.Complete += OnComplete;
        SixPin.Complete      += OnComplete;

        ClearButton.Click += (_, _) =>
        {
            foreach (var pin in new[] { DefaultPin, DigitPin, LetterPin, PasswordPin, SixPin })
            {
                for (int i = 0; i < pin.Count; i++)
                    pin.Digits[i] = string.Empty;

                // Reset visual cells
                if (pin.FindControl<ItemsControl>("PART_ItemsControl") is { } ic)
                    for (int i = 0; i < pin.Count; i++)
                        if (ic.ContainerFromIndex(i) is PinCodeItem cell)
                            cell.Text = string.Empty;
            }
            ResultText.Text = "—";
        };
    }
}
