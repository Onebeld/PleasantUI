using Android.Runtime;
using Avalonia.Android;
using PleasantUI.Core;

namespace PleasantUI.Example.Android;

[Activity(
    Label = "PleasantUI.Example",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true, 
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnStop()
    {
        PleasantSettings.Save();
        
        base.OnStop();
    }
}

[Application]
public class AndroidApp : AvaloniaAndroidApplication<App>
{
    protected AndroidApp(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer) { }
}