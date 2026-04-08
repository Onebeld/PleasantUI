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
    // Tracks the last leaf NavigationViewItem that was selected so we can
    // explicitly deselect it when the user switches to About/Settings.
    private NavigationViewItem? _lastLeafItem;

    public MainView()
    {
        InitializeComponent();
        MainNavigationView.SelectionChanged += OnNavigationSelectionChanged;
        PleasantUiExampleApp.NavPositionChanged += OnNavPositionChanged;
    }

    private void OnNavPositionChanged(NavigationViewPosition position)
    {
        // NavigationView.OnPositionChanged handles proxy creation internally.
        // We must never move AXAML-declared items — they already have a visual parent.
        MainNavigationView.Position = position;
    }

    private void OnNavigationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not AppViewModel vm) return;
        if (e.AddedItems.Count == 0) return;

        var selected = e.AddedItems[0] as NavigationViewItem;
        if (selected is null) return;

        // Switching to a top-level item (About, Settings, or Home directly) —
        // clear any previously selected leaf so it doesn't stay highlighted.
        if (selected.Tag is null)
        {
            ClearLastLeaf();

            if (ReferenceEquals(selected, HomeNavItem))
                vm.BackToHomePage();

            return;
        }

        // Leaf item — navigate to the corresponding page
        var page = selected.Tag as string switch
        {
            // Basic controls
            "Button"        => (IPage)new ButtonPage(),
            "Checkbox"      => new CheckBoxPage(),
            "Progress"      => new ProgressPage(),
            "Calendar"      => new CalendarPage(),
            "Carousel"      => new Pages.BasicControls.CarouselPage(),
            "ComboBox"      => new ComboBoxPage(),
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
            "PleasantMiniWindow" => new PleasantMiniWindowPage(),
            // ToolKit
            "MessageBox" => new MessageBoxPage(),
            "Docking"    => new DockingPage(),
            _            => null
        };

        if (page is null) return;

        // Deselect the previous leaf before tracking the new one
        ClearLastLeaf();
        _lastLeafItem = selected;

        vm.ChangePage(page);

        // Redirect SelectedItem back to HomeNavItem so its Content (HomeView)
        // stays visible, without going through SelectionChanged again.
        MainNavigationView.SelectionChanged -= OnNavigationSelectionChanged;
        MainNavigationView.SelectedItem = HomeNavItem;
        // HomeNavItem must not appear highlighted while a leaf page is active.
        HomeNavItem.IsSelected = false;
        // Re-highlight the leaf item — SelectSingleItemCore cleared it when we redirected to HomeNavItem.
        selected.IsSelected = true;
        MainNavigationView.SelectionChanged += OnNavigationSelectionChanged;
    }

    private void ClearLastLeaf()
    {
        if (_lastLeafItem is not null)
        {
            _lastLeafItem.IsSelected = false;
            _lastLeafItem = null;
        }
    }
}
