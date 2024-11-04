using Avalonia.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        
        Version pleasantUiVersion = typeof(PleasantTheme).Assembly.GetName().Version!;
        VersionTextBlock.Text = $"Version: {pleasantUiVersion.Major}.{pleasantUiVersion.Minor}.{pleasantUiVersion.Build}";
    }
}