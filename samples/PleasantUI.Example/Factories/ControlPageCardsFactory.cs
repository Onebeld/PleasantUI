using Avalonia.Collections;
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
            new("CardTitle/Button",       MaterialIcons.ButtonCursor,        "Card/Button", () =>  new ButtonPage(),   _eventAggregator),
            new("CardTitle/Checkbox",     MaterialIcons.CheckboxMarkedOutline,"Card/Checkbox", () =>  new CheckBoxPage(), _eventAggregator),
            new("CardTitle/Progress",     MaterialIcons.ProgressHelper,       "Card/Progress", () =>  new ProgressPage(), _eventAggregator),
            new("CardTitle/Calendar",     MaterialIcons.CalendarOutline,      "Card/Calendar", () =>  new CalendarPage(), _eventAggregator),
            new("CardTitle/Carousel",     MaterialIcons.ViewCarouselOutline,  "Card/Carousel", () =>  new CarouselPage(), _eventAggregator),
            new("CardTitle/ComboBox",     MaterialIcons.ExpandAllOutline,     "Card/ComboBox", () =>  new ComboBoxPage(), _eventAggregator),
            new("CardTitle/TextBox",      MaterialIcons.FormTextbox,          "Card/TextBox", () =>   new TextBoxPage(),  _eventAggregator),
            new("CardTitle/DataGrid",     MaterialIcons.Grid,                 "Card/DataGrid", () =>  new DataGridPage(), _eventAggregator),
            new("CardTitle/PinCode",      MaterialIcons.KeyboardOutline,       "Card/PinCode", () =>   new PinCodePage(),  _eventAggregator),
            new("CardTitle/SelectionList", MaterialIcons.ViewListOutline, "Card/SelectionList", () => new SelectionListPage(), _eventAggregator),
        ];
    }

    public AvaloniaList<ControlPageCard> CreatePleasantControlPageCards()
    {
        return
        [
            new("CardTitle/PleasantSnackbar",    MaterialIcons.InformationOutline,    "Card/PleasantSnackbar", () =>    new PleasantSnackbarPage(),    _eventAggregator),
            new("CardTitle/InformationBlock",    MaterialIcons.InformationBoxOutline, "Card/InformationBlock", () =>    new InformationBlockPage(),    _eventAggregator),
            new("CardTitle/OptionsDisplayItem",  MaterialIcons.ViewListOutline,       "Card/OptionsDisplayItem", () =>  new OptionsDisplayItemPage(),  _eventAggregator),
            new("CardTitle/PleasantTabView",     MaterialIcons.Tab,                   "Card/PleasantTabView", () =>     new PleasantTabViewPage(),     _eventAggregator),
            new("CardTitle/PleasantMenu",        MaterialIcons.MenuOpen,              "Card/PleasantMenu", () =>        new PleasantMenuPage(),        _eventAggregator),
            new("CardTitle/Timeline",            MaterialIcons.TimelineOutline,       "Card/Timeline", () =>            new TimelinePage(),            _eventAggregator),
            new("CardTitle/InstallWizard",       MaterialIcons.WizardHat,             "Card/InstallWizard", () =>       new InstallWizardPage(),       _eventAggregator),
            new("CardTitle/PleasantDrawer",      MaterialIcons.DrawingBox,    "Card/PleasantDrawer", () =>      new PleasantDrawerPage(),      _eventAggregator),
            new("CardTitle/PopConfirm",          MaterialIcons.CheckboxMarkedCircle,  "Card/PopConfirm", () =>          new PopConfirmPage(),          _eventAggregator),
            new("CardTitle/PathPicker",          MaterialIcons.FolderOpenOutline,     "Card/PathPicker", () =>          new PathPickerPage(),          _eventAggregator),
            new("CardTitle/PleasantMiniWindow",  MaterialIcons.WindowMinimize,        "Card/PleasantMiniWindow", () =>  new PleasantMiniWindowPage(),  _eventAggregator),
            new("CardTitle/BreadcrumbBar",       MaterialIcons.PageNextOutline,        "Card/BreadcrumbBar", () =>       new BreadcrumbBarPage(),       _eventAggregator),
            new("CardTitle/CommandBar",          MaterialIcons.ViewGridOutline,        "Card/CommandBar", () =>          new CommandBarPage(),          _eventAggregator),
            new("CardTitle/DashboardCard",       MaterialIcons.ViewDashboardOutline,   "Card/DashboardCard", () =>       new DashboardCardPage(),       _eventAggregator),
            new("CardTitle/LogViewerPanel",      MaterialIcons.TextBoxOutline,         "Card/LogViewerPanel", () =>      new LogViewerPanelPage(),      _eventAggregator),
            new("CardTitle/TerminalPanel",       MaterialIcons.ConsoleLine,            "Card/TerminalPanel", () =>       new TerminalPanelPage(),       _eventAggregator),
            new("CardTitle/TreeViewPanel",       MaterialIcons.FileTreeOutline,        "Card/TreeViewPanel", () =>       new TreeViewPanelPage(),       _eventAggregator),
            new("CardTitle/ItemListPanel",       MaterialIcons.FormatListBulletedType, "Card/ItemListPanel", () =>       new ItemListPanelPage(),       _eventAggregator),
            new("CardTitle/PropertyGrid",        MaterialIcons.TableColumnPlusAfter,   "Card/PropertyGrid", () =>        new PropertyGridPage(),        _eventAggregator),
            new("CardTitle/DownloadPanel",       MaterialIcons.DownloadOutline,        "Card/DownloadPanel", () =>       new DownloadPanelPage(),       _eventAggregator),
            new("CardTitle/CrashReportDialog",   MaterialIcons.BugOutline,             "Card/CrashReportDialog", () =>   new CrashReportDialogPage(),   _eventAggregator),
        ];
    }

    public AvaloniaList<ControlPageCard> CreateToolkitControlPageCards()
    {
        return
        [
            new("CardTitle/MessageBox",    MaterialIcons.MessageOutline,       "Card/MessageBox", () =>    new MessageBoxPage(),    _eventAggregator),
            new("CardTitle/NoticeDialog",  MaterialIcons.InformationOutline,   "Card/NoticeDialog", () =>  new NoticeDialogPage(),  _eventAggregator),
            new("CardTitle/StepDialog",    MaterialIcons.OrderNumericAscending, "Card/StepDialog", () =>   new StepDialogPage(),    _eventAggregator),
            new("CardTitle/Docking",       MaterialIcons.ViewDashboardOutline,  "Card/Docking", () =>      new DockingPage(),       _eventAggregator),
        ];
    }
}