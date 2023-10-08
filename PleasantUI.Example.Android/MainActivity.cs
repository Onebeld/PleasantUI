using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using PleasantUI.Core;

namespace PleasantUI.Example.Android;

[Activity(
    Label = "PleasantUI.Example",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true, 
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder);
    }

    protected override void OnStop()
    {
        PleasantSettings.Instance.Save();
        
        base.OnStop();
    }
}