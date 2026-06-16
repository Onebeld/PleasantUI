using Avalonia.Collections;
using Material.Icons;
using Material.Icons.Avalonia;
using PleasantUI.Example.Models;
using PleasantUI.Example.Pages.BasicControls;
using PleasantUI.Example.Pages.PleasantControls;
using PleasantUI.Example.Pages.Toolkit;
using PleasantUI.ToolKit.Services.Interfaces;namespace PleasantUI.Example.Factories;

public class ControlPageCardsFactory
{
    private readonly IEventAggregator _eventAggregator;
    
    public ControlPageCardsFactory(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }
    
    public AvaloniaList<ControlPageCard> CreateBasicControlPageCards()
    {
        return
        [
            new("CardTitle/Button",       MaterialIconKind.ButtonCursor,        "Card/Button", () =>  new ButtonPage(),   _eventAggregator),
            new("CardTitle/Checkbox",     MaterialIconKind.CheckboxMarkedOutline,"Card/Checkbox", () =>  new CheckBoxPage(), _eventAggregator),
            new("CardTitle/Progress",     MaterialIconKind.ProgressHelper,       "Card/Progress", () =>  new ProgressPage(), _eventAggregator),
            new("CardTitle/Calendar",     MaterialIconKind.CalendarOutline,      "Card/Calendar", () =>  new CalendarPage(), _eventAggregator),
            new("CardTitle/Carousel",     MaterialIconKind.ViewCarouselOutline,  "Card/Carousel", () =>  new CarouselPage(), _eventAggregator),
            new("CardTitle/ComboBox",     MaterialIconKind.ExpandAllOutline,     "Card/ComboBox", () =>  new ComboBoxPage(), _eventAggregator),
            new("CardTitle/TextBox",      MaterialIconKind.FormTextbox,          "Card/TextBox", () =>   new TextBoxPage(),  _eventAggregator),
            new("CardTitle/DataGrid",     MaterialIconKind.Grid,                 "Card/DataGrid", () =>  new DataGridPage(), _eventAggregator),
            new("CardTitle/PinCode",      MaterialIconKind.KeyboardOutline,       "Card/PinCode", () =>   new PinCodePage(),  _eventAggregator),
            new("CardTitle/SelectionList", MaterialIconKind.ViewListOutline, "Card/SelectionList", () => new SelectionListPage(), _eventAggregator),
        ];
    }

    public AvaloniaList<ControlPageCard> CreatePleasantControlPageCards()
    {
        return
        [
            new("CardTitle/PleasantSnackbar",    MaterialIconKind.InformationOutline,    "Card/PleasantSnackbar", () =>    new PleasantSnackbarPage(),    _eventAggregator),
            new("CardTitle/InformationBlock",    MaterialIconKind.InformationBoxOutline, "Card/InformationBlock", () =>    new InformationBlockPage(),    _eventAggregator),
            new("CardTitle/OptionsDisplayItem",  MaterialIconKind.ViewListOutline,       "Card/OptionsDisplayItem", () =>  new OptionsDisplayItemPage(),  _eventAggregator),
            new("CardTitle/PleasantTabView",     MaterialIconKind.Tab,                   "Card/PleasantTabView", () =>     new PleasantTabViewPage(),     _eventAggregator),
            new("CardTitle/PleasantMenu",        MaterialIconKind.MenuOpen,              "Card/PleasantMenu", () =>        new PleasantMenuPage(),        _eventAggregator),
            new("CardTitle/Timeline",            MaterialIconKind.TimelineOutline,       "Card/Timeline", () =>            new TimelinePage(),            _eventAggregator),
            new("CardTitle/InstallWizard",       MaterialIconKind.WizardHat,             "Card/InstallWizard", () =>       new InstallWizardPage(),       _eventAggregator),
            new("CardTitle/PleasantDrawer",      MaterialIconKind.DrawingBox,    "Card/PleasantDrawer", () =>      new PleasantDrawerPage(),      _eventAggregator),
            new("CardTitle/PopConfirm",          MaterialIconKind.CheckboxMarkedCircle,  "Card/PopConfirm", () =>          new PopConfirmPage(),          _eventAggregator),
            new("CardTitle/PathPicker",          MaterialIconKind.FolderOpenOutline,     "Card/PathPicker", () =>          new PathPickerPage(),          _eventAggregator),
            new("CardTitle/PleasantMiniWindow",  MaterialIconKind.WindowMinimize,        "Card/PleasantMiniWindow", () =>  new PleasantMiniWindowPage(),  _eventAggregator),
            new("CardTitle/BreadcrumbBar",       MaterialIconKind.PageNextOutline,        "Card/BreadcrumbBar", () =>       new BreadcrumbBarPage(),       _eventAggregator),
            new("CardTitle/CommandBar",          MaterialIconKind.ViewGridOutline,        "Card/CommandBar", () =>          new CommandBarPage(),          _eventAggregator),
            new("CardTitle/DashboardCard",       MaterialIconKind.ViewDashboardOutline,   "Card/DashboardCard", () =>       new DashboardCardPage(),       _eventAggregator),
            new("CardTitle/LogViewerPanel",      MaterialIconKind.TextBoxOutline,         "Card/LogViewerPanel", () =>      new LogViewerPanelPage(),      _eventAggregator),
            new("CardTitle/TerminalPanel",       MaterialIconKind.ConsoleLine,            "Card/TerminalPanel", () =>       new TerminalPanelPage(),       _eventAggregator),
            new("CardTitle/TreeViewPanel",       MaterialIconKind.FileTreeOutline,        "Card/TreeViewPanel", () =>       new TreeViewPanelPage(),       _eventAggregator),
            new("CardTitle/ItemListPanel",       MaterialIconKind.FormatListBulletedType, "Card/ItemListPanel", () =>       new ItemListPanelPage(),       _eventAggregator),
            new("CardTitle/PropertyGrid",        MaterialIconKind.TableColumnPlusAfter,   "Card/PropertyGrid", () =>        new PropertyGridPage(),        _eventAggregator),
            new("CardTitle/DownloadPanel",       MaterialIconKind.DownloadOutline,        "Card/DownloadPanel", () =>       new DownloadPanelPage(),       _eventAggregator),
            new("CardTitle/CrashReportDialog",   MaterialIconKind.BugOutline,             "Card/CrashReportDialog", () =>   new CrashReportDialogPage(),   _eventAggregator),
        ];
    }

    public AvaloniaList<ControlPageCard> CreateToolkitControlPageCards()
    {
        return
        [
            new("CardTitle/MessageBox",    MaterialIconKind.MessageOutline,       "Card/MessageBox", () =>    new MessageBoxPage(),    _eventAggregator),
            new("CardTitle/NoticeDialog",  MaterialIconKind.InformationOutline,   "Card/NoticeDialog", () =>  new NoticeDialogPage(),  _eventAggregator),
            new("CardTitle/StepDialog",    MaterialIconKind.OrderNumericAscending, "Card/StepDialog", () =>   new StepDialogPage(),    _eventAggregator),
            new("CardTitle/Docking",       MaterialIconKind.ViewDashboardOutline,  "Card/Docking", () =>      new DockingPage(),       _eventAggregator),
        ];
    }
}