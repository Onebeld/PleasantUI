using Avalonia.Controls;
using PleasantUI.Controls;
using PleasantUI.Example.Interfaces;
using PleasantUI.Example.Pages.BasicControls;
using PleasantUI.Example.Pages.PleasantControls;
using PleasantUI.Example.Pages.Toolkit;
using PleasantUI.Example.ViewModels;

namespace PleasantUI.Example;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        MainNavigationView.SelectionChanged += OnNavigationSelectionChanged;
    }

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not AppViewModel vm) return;
        if (e.AddedItems.Count == 0) return;

        var selected = e.AddedItems[0] as NavigationViewItem;
        if (selected is null) return;

        // Home item itself — go back to home card grid
        if (ReferenceEquals(selected, HomeNavItem))
        {
            vm.BackToHomePage();
            return;
        }

        // Leaf items use Tag to identify the target page
        var page = selected.Tag as string switch
        {
            // Basic controls
            "Button"        => (IPage)new ButtonPage(),
            "Checkbox"      => new CheckBoxPage(),
            "Progress"      => new ProgressPage(),
            "Calendar"      => new CalendarPage(),
            "Carousel"      => new Pages.BasicControls.CarouselPage(),            "ComboBox"      => new ComboBoxPage(),
            "TextBox"       => new TextBoxPage(),
            "DataGrid"      => new DataGridPage(),
            "PinCode"       => new PinCodePage(),
            "SelectionList" => new SelectionListPage(),
            // Pleasant controls
            "PleasantSnackbar"   => new PleasantSnackbarPage(),
            "InformationBlock"   => new InformationBlockPage(),
            "OptionsDisplayItem" => new OptionsDisplayItemPage(),
            "PleasantTabView"    => new PleasantTabViewPage(),
            "PleasantMenu"       => new PleasantMenuPage(),
            "Timeline"           => new TimelinePage(),
            "InstallWizard"      => new InstallWizardPage(),
            "PleasantDrawer"     => new PleasantDrawerPage(),
            "PopConfirm"         => new PopConfirmPage(),
            "PathPicker"         => new PathPickerPage(),
            // ToolKit
            "MessageBox" => new MessageBoxPage(),
            "Docking"    => new DockingPage(),
            _            => null
        };

        if (page is null) return;

        vm.ChangePage(page);

        // Keep HomeNavItem selected so its Content (HomeView) stays visible
        MainNavigationView.SelectionChanged -= OnNavigationSelectionChanged;
        MainNavigationView.SelectedItem = HomeNavItem;
        MainNavigationView.SelectionChanged += OnNavigationSelectionChanged;
    }
}
