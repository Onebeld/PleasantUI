using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace PleasantUI;

/// <summary>
/// Includes the PleasantUI DataGrid theme in an application.
/// Add this to your App.axaml styles alongside <see cref="PleasantTheme"/>:
/// <code>
/// &lt;StyleInclude Source="avares://PleasantUI.DataGrid/PleasantDataGridTheme.axaml" /&gt;
/// </code>
/// or in code:
/// <code>
/// Application.Current.Styles.Add(new PleasantDataGridTheme());
/// </code>
/// </summary>
public class PleasantDataGridTheme : Styles
{
    public PleasantDataGridTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
    }
}
