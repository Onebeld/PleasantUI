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
        if (e.AddedItems.Count == 0 || e.AddedItems[0] is not NavigationViewItem selected) return;

        // Switching to a top-level item (About, Settings, or Home directly) —
        // clear any previously selected leaf so it doesn't stay highlighted.
        if (selected.Tag is null)
        {
            vm.BackToHomePage();
            
            return;
        }

        // Leaf item — navigate to the corresponding page
        IPage? page = (selected.Tag as string) switch
        {
            // Basic controls
            "Button"        => new ButtonPage(),
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
            "BreadcrumbBar"      => new BreadcrumbBarPage(),
            "CommandBar"         => new CommandBarPage(),
            "DashboardCard"      => new DashboardCardPage(),
            "LogViewerPanel"     => new LogViewerPanelPage(),
            "TerminalPanel"      => new TerminalPanelPage(),
            "TreeViewPanel"      => new TreeViewPanelPage(),
            "PropertyGrid"       => new PropertyGridPage(),
            "DownloadPanel"      => new DownloadPanelPage(),
            "CrashReportDialog"  => new CrashReportDialogPage(),
            // ToolKit
            "MessageBox"   => new MessageBoxPage(),
            "NoticeDialog" => new NoticeDialogPage(),
            "StepDialog"   => new StepDialogPage(),
            "TableView" => new TableViewPage(),
            _            => null
        };

        if (page is null) return;

        vm.ChangePage(page);
    }
}
