using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using PleasantUI.Example;
using PleasantUI.Example.Browser;

internal sealed partial class Program
{
    private static Task Main(string[] args)
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        
        return BuildAvaloniaApp()
            .LogToTrace()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}