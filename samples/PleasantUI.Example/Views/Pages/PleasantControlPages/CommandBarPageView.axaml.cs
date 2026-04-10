using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Example.Views.Pages;
using PleasantCB  = PleasantUI.Controls.CommandBarButton;
using PleasantCTB = PleasantUI.Controls.CommandBarToggleButton;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class CommandBarPageView : LocalizedUserControl
{
    public CommandBarPageView()
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
        var saveBtn   = this.FindControl<PleasantCB>("SaveBtn");
        var copyBtn   = this.FindControl<PleasantCB>("CopyBtn");
        var editBtn   = this.FindControl<PleasantCB>("EditBtn");
        var pinBtn    = this.FindControl<PleasantCTB>("PinBtn");
        var boldBtn   = this.FindControl<PleasantCTB>("BoldBtn");
        var italicBtn = this.FindControl<PleasantCTB>("ItalicBtn");
        var pinBtn2   = this.FindControl<PleasantCTB>("PinBtn2");
        var basicResult  = this.FindControl<TextBlock>("BasicActionResult");
        var toggleResult = this.FindControl<TextBlock>("ToggleStateResult");

        // Detach stale handlers
        if (saveBtn   is not null) saveBtn.Click   -= OnBasicAction;
        if (copyBtn   is not null) copyBtn.Click   -= OnBasicAction;
        if (editBtn   is not null) editBtn.Click   -= OnBasicAction;
        if (pinBtn    is not null) pinBtn.IsCheckedChanged -= OnPinChanged;
        if (boldBtn   is not null) boldBtn.IsCheckedChanged   -= OnToggleChanged;
        if (italicBtn is not null) italicBtn.IsCheckedChanged -= OnToggleChanged;
        if (pinBtn2   is not null) pinBtn2.IsCheckedChanged   -= OnToggleChanged;

        // Attach fresh handlers
        if (saveBtn   is not null) saveBtn.Click   += OnBasicAction;
        if (copyBtn   is not null) copyBtn.Click   += OnBasicAction;
        if (editBtn   is not null) editBtn.Click   += OnBasicAction;
        if (pinBtn    is not null) pinBtn.IsCheckedChanged += OnPinChanged;
        if (boldBtn   is not null) boldBtn.IsCheckedChanged   += OnToggleChanged;
        if (italicBtn is not null) italicBtn.IsCheckedChanged += OnToggleChanged;
        if (pinBtn2   is not null) pinBtn2.IsCheckedChanged   += OnToggleChanged;
    }

    private void OnBasicAction(object? sender, RoutedEventArgs e)
    {
        var result = this.FindControl<TextBlock>("BasicActionResult");
        if (result is not null && sender is PleasantCB btn)
            result.Text = btn.Label ?? "—";
    }

    private void OnPinChanged(object? sender, RoutedEventArgs e)
    {
        var result = this.FindControl<TextBlock>("BasicActionResult");
        if (result is not null && sender is PleasantCTB tb)
            result.Text = $"Pin: {(tb.IsChecked == true ? "on" : "off")}";
    }

    private void OnToggleChanged(object? sender, RoutedEventArgs e)
    {
        var boldBtn   = this.FindControl<PleasantCTB>("BoldBtn");
        var italicBtn = this.FindControl<PleasantCTB>("ItalicBtn");
        var pinBtn2   = this.FindControl<PleasantCTB>("PinBtn2");
        var result    = this.FindControl<TextBlock>("ToggleStateResult");

        if (result is null) return;

        var parts = new List<string>();
        if (boldBtn?.IsChecked   == true) parts.Add("Bold");
        if (italicBtn?.IsChecked == true) parts.Add("Italic");
        if (pinBtn2?.IsChecked   == true) parts.Add("Pinned");

        result.Text = parts.Count > 0 ? string.Join(", ", parts) : "none";
    }
}
