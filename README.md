![GitHub License](https://img.shields.io/github/license/onebeld/PleasantUI?style=flat-square)
![GitHub repo size](https://img.shields.io/github/repo-size/onebeld/PleasantUI?style=flat-square)
![Nuget](https://img.shields.io/nuget/dt/PleasantUI?style=flat-square&logo=nuget)
![GitHub release (with filter)](https://img.shields.io/github/v/release/onebeld/PleasantUI?style=flat-square)

<img align="center" src="https://github.com/Onebeld/PleasantUI/assets/44552715/c8354beb-5b4b-4ce6-acbb-eb2b5e6a23e1">

# PleasantUI

Pleasant UI is a graphical user interface library for Avalonia with its own controls. Previously, it was only available as part of the Regul and Regul Save Cleaner projects. This project has existed since at least 2021.

This library continues the OlibUI tradition of releasing only later versions, not the very first.

This library is mostly focused on performance, lightness, and beauty, compared to many others.

This library is fully compatible with AOT compilation, and does not need to be added to rd.xml

## Getting Started

Install this library using NuGet, or copy the code to paste into your project file:

```xml
<PackageReference Include="PleasantUI" Version="4.0.0" />
```

### Setup

For your application, add PleasantTheme to your styles:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="YourApplication.App">
    <Application.Styles>
        <PleasantTheme />
    </Application.Styles>
</Application>
```
This library automatically loads settings and saves them when the program is finished _(note, for mobile projects you need to save settings manually)_

Make sure that in the application class file, the XAML loader is in the overridden initialization method. Otherwise, you will get an error that the program is not initialized when you run the program.

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using YourApplication.ViewModels;
using YourApplication.Views;

namespace YourApplication;

public partial class App : Application
{
    // That's exactly what you need to do, as shown below
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

Next, we need to modify the main window so that it inherits from PleasantWindow:

```csharp
using PleasantUI.Controls;

namespace YourApplication.Views;

public partial class MainWindow : PleasantWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```
Make sure that the (A)XAML file of the main window has a PleasantWindow object instead of Window:

```xml
<PleasantWindow xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
                mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
                x:Class="YourApplication.Views.MainWindow"
                Title="Avalonia Application">
</PleasantWindow>
```

Done! Now you can build your applications with this library.

## Screenshots

*Coming soon*

## Credits

* [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
* Some controls from PieroCastillo's [Aura.UI](https://github.com/PieroCastillo/Aura.UI) library
* [ProgressRing](https://github.com/ymg2006/FluentAvalonia.ProgressRing) by ymg2006

The editors I used to create this project:
* [JetBrains Rider](https://www.jetbrains.com/rider/)

<img src="https://github.com/Onebeld/PleasantUI/assets/44552715/c6bcf430-4153-4f72-bcca-e97e5cdce491" width="360" align="right"/>
