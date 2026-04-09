# PleasantUI Example App

A comprehensive demonstration application showcasing all PleasantUI controls, patterns, and best practices. This example app serves as both a reference for learning PleasantUI and a solid foundation for building your own applications.

## Overview

The PleasantUI Example app demonstrates:

- **All PleasantUI Controls** - Every control from the PleasantUI library is showcased with interactive examples
- **MVVM Architecture** - Clean separation of concerns with ViewModels, Views, and Models
- **Navigation Patterns** - NavigationView with three positions (Left, Top, Bottom) and dynamic content switching
- **Localization** - Full support for multiple languages (English, Russian) with the PleasantUI localization system
- **Theme System** - Theme switching with built-in themes (Light, Dark, Mint, Strawberry, Ice, Sunny, Spruce, Cherry, Cave, Lunar)
- **Event Aggregation** - Message-based communication between components
- **Logging** - Serilog integration for application logging
- **Responsive Design** - Adaptive layouts that work across different window sizes

## Why Use PleasantUI?

### 1. **Modern Fluent Design**
PleasantUI brings Microsoft's Fluent Design language to Avalonia, providing:
- Rounded corners and smooth animations
- Layered fill colors with elevation levels
- Accent color integration
- Pointer-over and pressed transitions
- Professional, polished appearance

### 2. **Complete Control Set**
PleasantUI provides both styled standard controls and custom controls:

**Styled Standard Controls:**
- Button (with Accent, Danger, AppBar variants)
- CheckBox, RadioButton, ToggleSwitch
- TextBox, AutoCompleteBox, NumericUpDown
- ComboBox, DropDownButton
- Calendar, CalendarDatePicker, TimePicker
- DataGrid (with PleasantUI.DataGrid package)
- ListBox, TreeView, Expander
- TabControl, ScrollBar, ScrollViewer
- ProgressBar, Menu, ToolTip
- Carousel, Separator, NotificationCard

**Custom Pleasant Controls:**
- NavigationView - Collapsible side navigation
- PleasantTabView - Chromium-style tab strip
- ContentDialog - Modal overlay dialog
- ProgressRing - Circular progress indicator
- OptionsDisplayItem - Settings-style rows
- InformationBlock - Compact label with icon
- Timeline - Chronological event display
- InstallWizard - Multi-step wizard
- PleasantMenu - Customizable flyout menu
- PathPicker - File/folder path picker
- PopConfirm - Confirmation popup
- And many more...

### 3. **Built-in Theme Engine**
- 10 built-in themes (Light, Dark, Mint, Strawberry, Ice, Sunny, Spruce, Cherry, Cave, Lunar)
- Custom theme support with ThemeEditorWindow
- Accent color follows OS preference or can be overridden
- Settings persisted automatically on desktop
- Reactive theme switching

### 4. **Localization System**
- `Localizer` singleton with .NET ResourceManager
- `{Localize Key}` AXAML markup extension for reactive bindings
- Language switching updates all bound strings instantly
- Safe lookup with `Localizer.TrDefault(key, fallback)`

### 5. **Cross-Platform**
- Works on Windows, macOS, Linux, Android, and iOS
- Platform-specific optimizations (e.g., macOS caption buttons)
- Adaptive layouts for desktop and mobile

### 6. **AOT-Compatible**
- No `rd.xml` required
- Ready for Native AOT compilation
- Performance optimized

## Architecture

### Project Structure

```
PleasantUI.Example/
├── Assets/                    # Images and resources
├── DataTemplates/              # Reusable data templates
├── Factories/                 # Factory classes for creating objects
├── Interfaces/                # Shared interfaces (IPage)
├── Logging/                   # Serilog configuration
├── Messages/                  # Event messages for EventAggregator
├── Models/                    # Data models
├── Pages/                     # Page implementations
│   ├── BasicControls/         # Standard Avalonia control demos
│   ├── PleasantControls/      # PleasantUI custom control demos
│   └── Toolkit/               # PleasantUI.ToolKit demos
├── Properties/
│   └── Localizations/         # .resx resource files (en, ru)
├── Structures/                # Helper structures
├── Styling/                   # Custom styles
├── ViewModels/                # ViewModels (MVVM pattern)
├── Views/                     # Views (AXAML)
│   ├── AboutView.axaml
│   ├── HomeView.axaml
│   ├── SettingsView.axaml
│   └── Pages/                 # Page-specific views
├── MainView.axaml             # Main navigation view
├── MainView.axaml.cs
├── PleasantUI.Example.csproj
└── PleasantUiExampleApp.cs    # Application entry point
```

### Key Patterns

#### 1. MVVM Pattern
```csharp
// ViewModel (AppViewModel.cs)
public class AppViewModel : ViewModelBase
{
    private IPage _page = null!;
    
    public IPage Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }
}

// View (MainView.axaml)
<UserControl DataContext="{Binding ViewModel}">
    <ContentControl Content="{Binding Page}" />
</UserControl>
```

#### 2. Navigation with NavigationView
The app uses NavigationView with support for three positions (Left, Top, Bottom):

```csharp
public static NavigationViewPosition NavPosition { get; set; } = NavigationViewPosition.Left;

public static void SetNavPosition(NavigationViewPosition position)
{
    NavPosition = position;
    NavPositionChanged?.Invoke(position);
}
```

#### 3. Event Aggregation
Components communicate via messages:

```csharp
// Subscribe
_eventAggregator.Subscribe<ChangePageMessage>(async message =>
{
    ChangePage(message.Page);
});

// Publish
_eventAggregator.Publish(new ChangePageMessage(page));
```

#### 4. Localization
```csharp
// Initialize
Localizer.AddRes(new ResourceManager(typeof(Properties.Localizations.App)));
Localizer.ChangeLang("en");

// In AXAML
<TextBlock Text="{Localize WelcomeMessage}" />

// In code-behind
string text = Localizer.Tr("WelcomeMessage");
```

#### 5. Theme System
```csharp
// PleasantTheme is automatically loaded from App.axaml
// Theme settings are persisted to disk automatically
// Access current theme:
var theme = PleasantSettings.Current?.Theme;
```

## Converting to a Real Application - Practical Guide

This section provides a step-by-step practical example of converting the PleasantUI Example app into a real application. We'll demonstrate converting it into an **ERP System for Autopark Management** - a business application for managing vehicles, drivers, maintenance, and fuel tracking.

### Real-World Example: Autopark Management ERP

**Target Application Features:**
- Vehicle inventory and tracking
- Driver management and assignments
- Maintenance scheduling and tracking
- Fuel consumption monitoring
- Trip logging and reporting
- Dashboard with KPIs

---

### Step 1: Copy and Rename the Project

```bash
# Copy the example app to your new project location
xcopy /E /I PleasantUI.Example AutoparkERP

# Navigate to the new project
cd AutoparkERP

# Rename files
ren PleasantUI.Example.csproj AutoparkERP.csproj
ren PleasantUiExampleApp.cs AutoparkERPApp.cs
```

**Update `AutoparkERP.csproj`:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net9.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <DebugType>Full</DebugType>
        <IncludeAvaloniaGenerators>true</IncludeAvaloniaGenerators>
        <NeutralLanguage>en</NeutralLanguage>
        
        <!-- Update these -->
        <Title>AutoparkERP</Title>
        <AssemblyName>AutoparkERP</AssemblyName>
        <RootNamespace>AutoparkERP</RootNamespace>
    </PropertyGroup>
    <!-- ... rest of file -->
</Project>
```

---

### Step 2: Remove Demo-Only Files and Directories

**Delete these demo-specific directories:**

```bash
# Remove demo control showcase pages
rmdir /S /Q Pages\BasicControls
rmdir /S /Q Pages\PleasantControls
rmdir /S /Q Pages\Toolkit

# Remove demo-specific factories
rmdir /S /Q Factories

# Remove demo-specific messages
rmdir /S /Q Messages

# Remove demo-specific structures
rmdir /S /Q Structures

# Remove demo-specific data templates
rmdir /S /Q DataTemplates

# Keep IPage interface - it's useful for page abstraction
# The IPage system provides a clean abstraction for your pages
```

**What you're keeping:**
- `Assets/` - Replace with your own assets
- `Logging/` - Keep for production logging
- `Models/` - Keep structure, replace with your models
- `Properties/Localizations/` - Keep, update with your strings
- `Styling/` - Keep, customize as needed
- `ViewModels/` - Keep `AppViewModel.cs`, remove others
- `Views/` - Keep structure, replace with your views
- `Interfaces/IPage.cs` - Keep for page abstraction

---

### Step 3: Update the Application Entry Point

**Rename and update `AutoparkERPApp.cs`:**

```csharp
using System.Resources;
using Avalonia;
using Avalonia.Controls;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Localization;
using AutoparkERP.ViewModels;

namespace AutoparkERP;

public class AutoparkERPApp : Application
{
    public static PleasantTheme PleasantTheme { get; protected set; } = null!;
    public static IPleasantWindow Main { get; protected set; } = null!;
    public static MainViewModel ViewModel { get; private set; } = null!;

    public AutoparkERPApp()
    {
        Localizer.Reset();
        Localizer.AddRes(new ResourceManager(typeof(Properties.Localizations.App)));
        Localizer.ChangeLang(LanguageKey);
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Load persisted language from settings
            if (PleasantSettings.Current is not null && 
                !string.IsNullOrEmpty(PleasantSettings.Current.Language))
            {
                LanguageKey = PleasantSettings.Current.Language;
                Localizer.ChangeLang(LanguageKey);
            }

            // Initialize your main ViewModel with real services
            var vehicleService = new VehicleService();
            var driverService = new DriverService();
            var maintenanceService = new MaintenanceService();
            
            ViewModel = new MainViewModel(vehicleService, driverService, maintenanceService);
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = ViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    public static string LanguageKey { get; set; } = "en";
}
```

---

### Step 4: Create Your Main Window

**Replace `MainView.axaml` with `MainWindow.axaml`:**

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:views="clr-namespace:AutoparkERP.Views"
        x:Class="AutoparkERP.MainWindow"
        Title="Autopark ERP"
        Width="1400" Height="900"
        WindowStartupLocation="CenterScreen"
        MinWidth="1200" MinHeight="700">
    
    <DockPanel>
        <!-- Title Bar -->
        <StackPanel DockPanel.Dock="Top" 
                    Orientation="Horizontal" 
                    Spacing="10"
                    Margin="20,10">
            <PathIcon Data="{x:Static MaterialIcons.Car}" 
                      Width="32" Height="32" 
                      Foreground="{DynamicResource AccentColor}" />
            <StackPanel VerticalAlignment="Center">
                <TextBlock Text="Autopark ERP" 
                           FontSize="18" 
                           FontWeight="SemiBold" />
                <TextBlock Text="Vehicle Management System" 
                           FontSize="12" 
                           Opacity="0.7" />
            </StackPanel>
        </StackPanel>

        <!-- Main Content -->
        <views:MainView />
    </DockPanel>
</Window>
```

**Update `MainWindow.axaml.cs`:**

```csharp
using Avalonia.Controls;
using AutoparkERP.ViewModels;

namespace AutoparkERP;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

---

### Step 5: Adapt IPage for Business Pages

The `IPage` interface provides a useful abstraction for your pages. Keep it and adapt it for your business needs.

**Keep `Interfaces/IPage.cs` as-is or extend it:**

```csharp
namespace AutoparkERP.Interfaces;

public interface IPage
{
    // Base interface for all pages - provides type safety and abstraction
    // You can extend this with additional properties if needed:
    // string PageTitle { get; }
    // bool RequiresAuthentication { get; }
}
```

**Implement IPage for your business pages:**

```csharp
using Avalonia.Controls;
using AutoparkERP.Interfaces;

namespace AutoparkERP.Views;

public class DashboardView : UserControl, IPage
{
    public DashboardView()
    {
        InitializeComponent();
    }
}

public class VehiclesView : UserControl, IPage
{
    public VehiclesView(VehicleService vehicleService)
    {
        InitializeComponent();
        _vehicleService = vehicleService;
        LoadVehiclesAsync();
    }
    
    private readonly VehicleService _vehicleService;
}
```

**Update MainViewModel to work with IPage:**

```csharp
private UserControl _currentPage = null!;

public UserControl CurrentPage
{
    get => _currentPage;
    set => SetProperty(ref _currentPage, value);
}

public void NavigateToDashboard()
{
    CurrentPage = new DashboardView(); // DashboardView implements IPage
}

public void NavigateToVehicles()
{
    CurrentPage = new VehiclesView(_vehicleService); // VehiclesView implements IPage
}
```

The IPage interface gives you:
- **Type safety** - Ensure all pages follow a common contract
- **Abstraction** - Work with pages through the interface
- **Extensibility** - Add common properties/methods to all pages (e.g., PageTitle, OnNavigatedTo, OnNavigatedFrom)
- **IntelliSense** - Better IDE support when working with pages

---

### Step 6: Create Real Business Models

**Create `Models/Vehicle.cs`:**

```csharp
using System;

namespace AutoparkERP.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Vin { get; set; } = string.Empty;
    public VehicleStatus Status { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public decimal FuelLevel { get; set; }
    public int Odometer { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public enum VehicleStatus
{
    Available,
    InUse,
    Maintenance,
    OutOfService
}
```

**Create `Models/Driver.cs`:**

```csharp
namespace AutoparkERP.Models;

public class Driver
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTime LicenseExpiry { get; set; }
    public bool IsActive { get; set; }
}
```

**Create `Models/MaintenanceRecord.cs`:**

```csharp
using System;

namespace AutoparkERP.Models;

public class MaintenanceRecord
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty; // Oil change, Tire rotation, etc.
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public int Odometer { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
}
```

---

### Step 7: Create Business Services

**Create `Services/VehicleService.cs`:**

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoparkERP.Models;

namespace AutoparkERP.Services;

public class VehicleService
{
    private readonly List<Vehicle> _vehicles = new();
    
    public VehicleService()
    {
        // Initialize with sample data
        _vehicles.AddRange(new[]
        {
            new Vehicle 
            { 
                Id = 1, 
                LicensePlate = "ABC-123", 
                Make = "Toyota", 
                Model = "Camry", 
                Year = 2022,
                Status = VehicleStatus.Available,
                Odometer = 45000
            },
            new Vehicle 
            { 
                Id = 2, 
                LicensePlate = "DEF-456", 
                Make = "Honda", 
                Model = "Civic", 
                Year = 2023,
                Status = VehicleStatus.InUse,
                Odometer = 12000
            }
        });
    }
    
    public Task<List<Vehicle>> GetAllVehiclesAsync()
    {
        return Task.FromResult(_vehicles.ToList());
    }
    
    public Task<Vehicle?> GetVehicleByIdAsync(int id)
    {
        return Task.FromResult(_vehicles.FirstOrDefault(v => v.Id == id));
    }
    
    public Task AddVehicleAsync(Vehicle vehicle)
    {
        vehicle.Id = _vehicles.Count > 0 ? _vehicles.Max(v => v.Id) + 1 : 1;
        _vehicles.Add(vehicle);
        return Task.CompletedTask;
    }
    
    public Task UpdateVehicleAsync(Vehicle vehicle)
    {
        var index = _vehicles.FindIndex(v => v.Id == vehicle.Id);
        if (index >= 0)
        {
            _vehicles[index] = vehicle;
        }
        return Task.CompletedTask;
    }
    
    public Task DeleteVehicleAsync(int id)
    {
        _vehicles.RemoveAll(v => v.Id == id);
        return Task.CompletedTask;
    }
}
```

**Create similar services for `DriverService.cs` and `MaintenanceService.cs`** following the same pattern.

---

### Step 7: Update MainViewModel for Real Navigation

**Replace `ViewModels/AppViewModel.cs` with `ViewModels/MainViewModel.cs`:**

```csharp
using Avalonia.Collections;
using Avalonia.Threading;
using PleasantUI.Core;
using PleasantUI.Core.Localization;
using AutoparkERP.Services;
using AutoparkERP.Views;

namespace AutoparkERP.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly VehicleService _vehicleService;
    private readonly DriverService _driverService;
    private readonly MaintenanceService _maintenanceService;
    
    private UserControl _currentPage = null!;
    private NavigationViewItem? _selectedNavItem;

    public AvaloniaList<DashboardKpi> DashboardKpis { get; } = new();
    public UserControl CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public MainViewModel(VehicleService vehicleService, 
                        DriverService driverService, 
                        MaintenanceService maintenanceService)
    {
        _vehicleService = vehicleService;
        _driverService = driverService;
        _maintenanceService = maintenanceService;
        
        // Load dashboard data
        LoadDashboardData();
        
        // Set initial page
        CurrentPage = new DashboardView();
        
        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
    }

    private void LoadDashboardData()
    {
        var vehicles = _vehicleService.GetAllVehiclesAsync().Result;
        var drivers = _driverService.GetAllDriversAsync().Result;
        
        DashboardKpis.Clear();
        DashboardKpis.Add(new DashboardKpi 
        { 
            Title = Localizer.Tr("TotalVehicles"), 
            Value = vehicles.Count.ToString(),
            Icon = MaterialIcons.Car
        });
        DashboardKpis.Add(new DashboardKpi 
        { 
            Title = Localizer.Tr("ActiveDrivers"), 
            Value = drivers.Count(d => d.IsActive).ToString(),
            Icon = MaterialIcons.Account
        });
        DashboardKpis.Add(new DashboardKpi 
        { 
            Title = Localizer.Tr("InMaintenance"), 
            Value = vehicles.Count(v => v.Status == VehicleStatus.Maintenance).ToString(),
            Icon = MaterialIcons.Wrench
        });
    }

    public void NavigateToDashboard()
    {
        CurrentPage = new DashboardView();
    }

    public void NavigateToVehicles()
    {
        CurrentPage = new VehiclesView(_vehicleService);
    }

    public void NavigateToDrivers()
    {
        CurrentPage = new DriversView(_driverService);
    }

    public void NavigateToMaintenance()
    {
        CurrentPage = new MaintenanceView(_maintenanceService, _vehicleService);
    }

    public void NavigateToReports()
    {
        CurrentPage = new ReportsView();
    }

    public void NavigateToSettings()
    {
        CurrentPage = new SettingsView();
    }

    private void OnLanguageChanged(string _)
    {
        Dispatcher.UIThread.Post(LoadDashboardData);
    }
}

public class DashboardKpi
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
```

---

### Step 9: Create Real Application Views

**Create `Views/MainView.axaml`:**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:views="clr-namespace:AutoparkERP.Views"
            x:Class="AutoparkERP.Views.MainView">
    
    <Grid RowDefinitions="Auto,*">
        <!-- Navigation Bar -->
        <NavigationView x:Name="MainNavigationView"
                      Grid.Row="1"
                      IsPaneOpen="True"
                      OpenPaneLength="250">
            
            <NavigationView.MenuItems>
                <NavigationViewItem Header="{Localize Dashboard}" 
                                  Icon="{x:Static MaterialIcons.Dashboard}"
                                  Tag="Dashboard"
                                  IsSelected="True" />
                
                <NavigationViewItem Header="{Localize Vehicles}" 
                                  Icon="{x:Static MaterialIcons.Car}"
                                  Tag="Vehicles" />
                
                <NavigationViewItem Header="{Localize Drivers}" 
                                  Icon="{x:Static MaterialIcons.Account}"
                                  Tag="Drivers" />
                
                <NavigationViewItem Header="{Localize Maintenance}" 
                                  Icon="{x:Static MaterialIcons.Wrench}"
                                  Tag="Maintenance" />
                
                <NavigationViewItem Header="{Localize Reports}" 
                                  Icon="{x:Static MaterialIcons.ChartBar}"
                                  Tag="Reports" />
            </NavigationView.MenuItems>

            <NavigationView.FooterMenuItems>
                <NavigationViewItem Header="{Localize Settings}" 
                                  Icon="{x:Static MaterialIcons.Settings}"
                                  Tag="Settings" />
            </NavigationView.FooterMenuItems>

            <NavigationView.Content>
                <ContentControl Content="{Binding CurrentPage}" />
            </NavigationView.Content>
        </NavigationView>
    </Grid>
</UserControl>
```

**Update `Views/MainView.axaml.cs`:**

```csharp
using Avalonia.Controls;
using Avalonia.Interactivity;
using AutoparkERP.ViewModels;

namespace AutoparkERP.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        MainNavigationView.SelectionChanged += OnNavigationSelectionChanged;
    }

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (e.AddedItems.Count == 0) return;

        var selected = e.AddedItems[0] as NavigationViewItem;
        if (selected is null) return;

        switch (selected.Tag as string)
        {
            case "Dashboard":
                vm.NavigateToDashboard();
                break;
            case "Vehicles":
                vm.NavigateToVehicles();
                break;
            case "Drivers":
                vm.NavigateToDrivers();
                break;
            case "Maintenance":
                vm.NavigateToMaintenance();
                break;
            case "Reports":
                vm.NavigateToReports();
                break;
            case "Settings":
                vm.NavigateToSettings();
                break;
        }
    }
}
```

---

### Step 10: Create the Dashboard View

**Create `Views/DashboardView.axaml`:**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            x:Class="AutoparkERP.Views.DashboardView">
    
    <ScrollViewer>
        <StackPanel Margin="20" Spacing="20">
            
            <!-- Header -->
            <TextBlock Text="{Localize Dashboard}" 
                       FontSize="28" 
                       FontWeight="SemiBold" />
            
            <!-- KPI Cards -->
            <ItemsControl ItemsSource="{Binding DashboardKpis}">
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <WrapPanel Orientation="Horizontal" />
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Border Width="300" Height="120" 
                                Margin="0,0,10,0"
                                CornerRadius="8"
                                Background="{DynamicResource BackgroundColor2}">
                            <DockPanel Margin="20">
                                <PathIcon Data="{Binding Icon}" 
                                          Width="40" Height="40"
                                          DockPanel.Dock="Left"
                                          Foreground="{DynamicResource AccentColor}" />
                                <StackPanel DockPanel.Dock="Right" VerticalAlignment="Center">
                                    <TextBlock Text="{Binding Title}" 
                                               FontSize="14" 
                                               Opacity="0.7" />
                                    <TextBlock Text="{Binding Value}" 
                                               FontSize="32" 
                                               FontWeight="SemiBold" />
                                </StackPanel>
                            </DockPanel>
                        </Border>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>

            <!-- Recent Activity Section -->
            <TextBlock Text="{Localize RecentActivity}" 
                       FontSize="20" 
                       FontWeight="SemiBold" 
                       Margin="0,20,0,10" />
            
            <Border CornerRadius="8" 
                    Background="{DynamicResource BackgroundColor2}"
                    Padding="20">
                <StackPanel Spacing="10">
                    <TextBlock Text="Vehicle ABC-123 assigned to John Doe" 
                               Opacity="0.8" />
                    <TextBlock Text="Maintenance scheduled for DEF-456" 
                               Opacity="0.8" />
                    <TextBlock Text="Fuel level updated for GHI-789" 
                               Opacity="0.8" />
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

**Create `Views/DashboardView.axaml.cs`:**

```csharp
using Avalonia.Controls;

namespace AutoparkERP.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }
}
```

---

### Step 11: Create the Vehicles View

**Create `Views/VehiclesView.axaml`:**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:models="clr-namespace:AutoparkERP.Models"
            x:Class="AutoparkERP.Views.VehiclesView">
    
    <Grid RowDefinitions="Auto,*">
        <!-- Toolbar -->
        <StackPanel Grid.Row="0" 
                   Orientation="Horizontal" 
                   Spacing="10"
                   Margin="20,20,20,10">
            <Button Content="{Localize AddVehicle}" 
                    Theme="{DynamicResource AccentButtonTheme}" />
            <Button Content="{Localize Refresh}" />
        </StackPanel>

        <!-- Data Grid -->
        <DataGrid Grid.Row="1"
                  Margin="20,0,20,20"
                  AutoGenerateColumns="False"
                  IsReadOnly="True"
                  ItemsSource="{Binding Vehicles}">
            <DataGrid.Columns>
                <DataGridTextColumn Header="{Localize LicensePlate}" 
                                   Binding="{Binding LicensePlate}" 
                                   Width="*" />
                <DataGridTextColumn Header="{Localize Make}" 
                                   Binding="{Binding Make}" 
                                   Width="*" />
                <DataGridTextColumn Header="{Localize Model}" 
                                   Binding="{Binding Model}" 
                                   Width="*" />
                <DataGridTextColumn Header="{Localize Year}" 
                                   Binding="{Binding Year}" 
                                   Width="100" />
                <DataGridTextColumn Header="{Localize Status}" 
                                   Binding="{Binding Status}" 
                                   Width="*" />
                <DataGridTextColumn Header="{Localize Odometer}" 
                                   Binding="{Binding Odometer}" 
                                   Width="120" />
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</UserControl>
```

**Create `Views/VehiclesView.axaml.cs`:**

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using AutoparkERP.Models;
using AutoparkERP.Services;

namespace AutoparkERP.Views;

public partial class VehiclesView : UserControl
{
    private readonly VehicleService _vehicleService;
    
    public VehiclesView(VehicleService vehicleService)
    {
        InitializeComponent();
        _vehicleService = vehicleService;
        LoadVehiclesAsync();
    }

    private async Task LoadVehiclesAsync()
    {
        var vehicles = await _vehicleService.GetAllVehiclesAsync();
        DataContext = new { Vehicles = vehicles };
    }
}
```

---

### Step 11: Update Localization Strings

**Update `Properties/Localizations/App.resx` with your app's strings:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Dashboard" xml:space="preserve">
    <value>Dashboard</value>
  </data>
  <data name="Vehicles" xml:space="preserve">
    <value>Vehicles</value>
  </data>
  <data name="Drivers" xml:space="preserve">
    <value>Drivers</value>
  </data>
  <data name="Maintenance" xml:space="preserve">
    <value>Maintenance</value>
  </data>
  <data name="Reports" xml:space="preserve">
    <value>Reports</value>
  </data>
  <data name="Settings" xml:space="preserve">
    <value>Settings</value>
  </data>
  <data name="AddVehicle" xml:space="preserve">
    <value>Add Vehicle</value>
  </data>
  <data name="Refresh" xml:space="preserve">
    <value>Refresh</value>
  </data>
  <data name="LicensePlate" xml:space="preserve">
    <value>License Plate</value>
  </data>
  <data name="Make" xml:space="preserve">
    <value>Make</value>
  </data>
  <data name="Model" xml:space="preserve">
    <value>Model</value>
  </data>
  <data name="Year" xml:space="preserve">
    <value>Year</value>
  </data>
  <data name="Status" xml:space="preserve">
    <value>Status</value>
  </data>
  <data name="Odometer" xml:space="preserve">
    <value>Odometer</value>
  </data>
  <data name="TotalVehicles" xml:space="preserve">
    <value>Total Vehicles</value>
  </data>
  <data name="ActiveDrivers" xml:space="preserve">
    <value>Active Drivers</value>
  </data>
  <data name="InMaintenance" xml:space="preserve">
    <value>In Maintenance</value>
  </data>
  <data name="RecentActivity" xml:space="preserve">
    <value>Recent Activity</value>
  </data>
</root>
```

---

### Step 13: Update Project References

**Keep only the PleasantUI packages you need in `AutoparkERP.csproj`:**

```xml
<ItemGroup>
    <ProjectReference Include="..\..\src\PleasantUI\PleasantUI.csproj"/>
    <ProjectReference Include="..\..\src\PleasantUI.DataGrid\PleasantUI.DataGrid.csproj"/>
    <ProjectReference Include="..\..\src\PleasantUI.MaterialIcons\PleasantUI.MaterialIcons.csproj"/>
    <!-- Remove ToolKit if you don't need MessageBox or ThemeEditorWindow -->
    <ProjectReference Include="..\..\src\PleasantUI.ToolKit\PleasantUI.ToolKit.csproj"/>
</ItemGroup>

<ItemGroup>
    <!-- Keep Serilog for production logging -->
    <PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
    <PackageReference Include="Serilog" Version="4.3.0" />
</ItemGroup>
```

---

### Step 14: What You Removed vs What You Kept

**Removed (Demo Fluff):**
- ❌ `Pages/BasicControls/` - Demo pages for standard Avalonia controls
- ❌ `Pages/PleasantControls/` - Demo pages showcasing PleasantUI controls
- ❌ `Pages/Toolkit/` - Demo pages for ToolKit features
- ❌ `Factories/` - Factory for creating demo control cards
- ❌ `Messages/` - Demo event messages
- ❌ `Structures/` - Demo helper structures
- ❌ `DataTemplates/` - Demo-specific templates
- ❌ `ViewModels/Pages/` - Demo page ViewModels

**Kept (Core Infrastructure):**
- ✅ `Assets/` - Replace with your own images/icons
- ✅ `Logging/` - Production-ready logging configuration
- ✅ `Models/` - Structure kept, replaced with business models
- ✅ `Properties/Localizations/` - Localization infrastructure
- ✅ `Styling/` - Custom styles (customize as needed)
- ✅ `ViewModels/MainViewModel.cs` - Core navigation ViewModel
- ✅ `Views/` - Structure kept, replaced with real views
- ✅ `Interfaces/IPage.cs` - Page abstraction interface (keep and adapt)
- ✅ PleasantUI package references - All UI functionality
- ✅ Localization system - Full i18n support
- ✅ Theme system - Theme switching capability

---

### Summary of Changes

| Component | Example App | Autopark ERP | Change |
|-----------|-------------|--------------|--------|
| **Entry Point** | `PleasantUiExampleApp.cs` | `AutoparkERPApp.cs` | Renamed, removed demo logic |
| **Main View** | `MainView.axaml` (demo showcase) | `MainWindow.axaml` + `MainView.axaml` | Split into window and navigation |
| **ViewModels** | `AppViewModel.cs` (demo cards) | `MainViewModel.cs` (real navigation) | Replaced demo logic with business navigation |
| **Pages** | 38 demo pages | 5 real pages (Dashboard, Vehicles, Drivers, Maintenance, Reports) | Removed demo, added business pages |
| **Models** | Demo models | `Vehicle`, `Driver`, `MaintenanceRecord` | Replaced with business models |
| **Services** | None | `VehicleService`, `DriverService`, `MaintenanceService` | Added business logic layer |
| **Localization** | Demo strings | ERP-specific strings | Updated for business context |
| **Navigation** | Demo control showcase | Business module navigation | Replaced with real navigation |

---

### Key Takeaways

1. **Keep the Infrastructure** - The PleasantUI setup, localization, theme system, and logging are production-ready. Don't reinvent the wheel.

2. **Remove the Demo Content** - Delete all control showcase pages, factories, and demo-specific code. They exist only to demonstrate PleasantUI features.

3. **Add Your Business Layer** - Create services that encapsulate your business logic. Keep ViewModels focused on UI state and navigation.

4. **Replace Models with Domain Objects** - Use models that represent your actual business entities, not demo examples.

5. **Customize Navigation** - The NavigationView pattern from the example is excellent for desktop apps. Just change the menu items and destinations.

6. **Leverage PleasantUI Controls** - Use PleasantUI controls like DataGrid, NavigationView, and OptionsDisplayItem in your real app - they're production-ready.

7. **Keep Localization** - The PleasantUI localization system is powerful and easy to use. Keep it for your multilingual needs.

8. **Theme System** - The theme system adds value to any application. Keep it for a polished user experience.

The example app provides an excellent foundation - you get a complete PleasantUI setup with all the plumbing already in place. You just need to remove the demo content and add your business logic.

## Building the Example App

### Prerequisites

- .NET 9.0 SDK
- Avalonia 12.0

### Build from Source

```bash
# Navigate to the PleasantUI repository root
cd PleasantUI

# Build the entire solution
dotnet build

# Run the example app
dotnet run --project samples/PleasantUI.Example/PleasantUI.Example.csproj
```

### Using NuGet Packages

If you want to use the published packages instead of building from source:

1. Remove project references from `PleasantUI.Example.csproj`
2. Add package references:

```xml
<ItemGroup>
    <PackageReference Include="PleasantUI" Version="5.2.0" />
    <PackageReference Include="PleasantUI.DataGrid" Version="5.2.0" />
    <PackageReference Include="PleasantUI.MaterialIcons" Version="5.2.0" />
    <PackageReference Include="PleasantUI.ToolKit" Version="5.2.0" />
</ItemGroup>
```

## Controls Demonstrated

### Basic Controls (Standard Avalonia)
- Button, CheckBox, RadioButton
- TextBox, NumericUpDown
- ComboBox, Calendar
- DataGrid, ListBox, TreeView
- ProgressBar, Slider
- Expander, TabControl
- Carousel, Separator

### Pleasant Controls
- NavigationView
- PleasantTabView
- ContentDialog
- ProgressRing
- OptionsDisplayItem
- InformationBlock
- Timeline
- InstallWizard
- PleasantMenu
- PathPicker
- PopConfirm
- BreadcrumbBar
- CommandBar
- DashboardCard
- LogViewerPanel
- TerminalPanel
- TreeViewPanel
- ItemListPanel
- StepDialog
- PropertyGrid
- DownloadPanel
- CrashReportDialog
- PleasantDrawer
- PleasantMiniWindow
- PleasantSnackbar

### ToolKit
- MessageBox
- Docking System

## Best Practices Demonstrated

### 1. **Separation of Concerns**
- Views only handle UI logic
- ViewModels handle business logic and state
- Models represent data

### 2. **Reactive Bindings**
- Use `{CompiledBinding}` for better performance
- Use `{Localize}` for reactive localization
- Use `ObservableCollection` for dynamic lists

### 3. **Resource Management**
- Use data templates for reusable UI
- Use styles for consistent appearance
- Use converters for data transformation

### 4. **Error Handling**
- Try-catch blocks around critical operations
- Logging for debugging and monitoring
- Graceful degradation when features fail

### 5. **Performance**
- Virtualization for large lists
- Lazy loading of resources
- Efficient event subscription/unsubscription

## Customization Guide

### Adding New Pages

1. Create a page class implementing `IPage`:
```csharp
public class MyPage : UserControl, IPage
{
    public MyPage()
    {
        InitializeComponent();
    }
}
```

2. Create the AXAML view:
```xml
<UserControl x:Class="YourApp.Pages.MyPage">
    <DockPanel>
        <!-- Your content -->
    </DockPanel>
</UserControl>
```

3. Add to navigation:
```csharp
vm.ChangePage(new MyPage());
```

### Adding New Languages

1. Create a new `.resx` file (e.g., `App.fr.resx`)
2. Translate all strings
3. Add to language list:
```csharp
public static readonly Language[] Languages =
[
    new("English", "en"),
    new("Русский", "ru"),
    new("Français", "fr")
];
```

### Customizing Themes

1. Create custom theme colors in ThemeEditorWindow
2. Export and save the theme
3. Apply in your app:
```csharp
PleasantSettings.Current.Theme = "MyCustomTheme";
```

## Additional Resources

- [PleasantUI Documentation](../docs/llms.txt) - Complete documentation for all controls
- [PleasantUI README](../README.md) - Main project README
- [Avalonia Documentation](https://docs.avaloniaui.net/) - Avalonia framework documentation
- [Fluent Design Guidelines](https://learn.microsoft.com/en-us/windows/apps/design/) - Microsoft's design guidelines

## License

This example app is part of the PleasantUI project and is licensed under the MIT License. See the main project LICENSE file for details.
