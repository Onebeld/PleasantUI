using Avalonia.Controls;
using PleasantUI.Example.Views.Pages.ControlPages;

namespace PleasantUI.Example.Pages.BasicControls;

public class ComboBoxPage : LocalizedPage
{
    public override string TitleKey { get; } = "CardTitle/ComboBox";
    public override bool ShowTitle { get; } = true;
    public override Control Content { get; } = new ComboBoxPageView();
}
