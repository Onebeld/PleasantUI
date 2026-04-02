using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class PleasantMenuPageView : UserControl
{
    private static string T(string key, string fallback) =>
        Localizer.TrDefault(key, fallback, "PleasantMenu");

    public PleasantMenuPageView()
    {
        InitializeComponent();
        BuildMenus();
    }

    private void BuildMenus()
    {
        // ── Shared items ──────────────────────────────────────────────────────
        PleasantMenuItem[] items =
        [
            new()
            {
                Icon    = MaterialIcons.FolderOpenOutline,
                Label   = T("Open",    "Open"),
                ToolTip = T("OpenTip", "Open a file"),
                Command = null   // wire to a real command in your app
            },
            new()
            {
                Icon    = MaterialIcons.ContentSaveOutline,
                Label   = T("Save",    "Save"),
                ToolTip = T("SaveTip", "Save current file"),
                SecondaryCommand = null  // shows split chevron when set
            },
            new()
            {
                Icon    = MaterialIcons.PlusCircleOutline,
                Label   = T("New",     "New"),
                ToolTip = T("NewTip",  "Create new file")
            },
            new()
            {
                Icon    = MaterialIcons.WrenchOutline,
                Label   = T("Tools",   "Tools"),
                ToolTip = T("ToolsTip","Open tools")
            },
            new()
            {
                Icon    = MaterialIcons.HomeOutline,
                Label   = T("Home",    "Home"),
                ToolTip = T("HomeTip", "Go to home screen")
            },
            new()
            {
                Icon      = MaterialIcons.DeleteOutline,
                Label     = T("Delete",    "Delete"),
                ToolTip   = T("DeleteTip", "Delete selected"),
                IsEnabled = false
            }
        ];

        PleasantMenuFooterItem[] footerItems =
        [
            new()
            {
                Icon       = MaterialIcons.CogOutline,
                ToolTip    = T("Settings",    "Settings"),
                AlignRight = false,
                Command    = null
            },
            new()
            {
                Icon       = MaterialIcons.InformationOutline,
                ToolTip    = T("About",       "About"),
                AlignRight = true,
                Command    = null
            },
            new()
            {
                Icon       = MaterialIcons.ExitToApp,
                ToolTip    = T("Exit",        "Exit"),
                AlignRight = true,
                Command    = null
            }
        ];

        // Badges: info blocks showing counts
        var badges = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing     = 6,
            Children    =
            {
                new InformationBlock
                {
                    Icon    = MaterialIcons.FolderOutline,
                    Content = "3",
                    [ToolTip.TipProperty] = T("OpenFiles", "Open files")
                },
                new InformationBlock
                {
                    Icon    = MaterialIcons.PackageVariantClosed,
                    Content = "5",
                    [ToolTip.TipProperty] = T("Modules", "Loaded modules")
                }
            }
        };

        // ── Full demo menu ────────────────────────────────────────────────────
        foreach (var item in items)   DemoMenu.Items.Add(item);
        foreach (var fi in footerItems) DemoMenu.FooterItems.Add(fi);
        DemoMenu.Badges = badges;

        // ── No-footer menu ────────────────────────────────────────────────────
        foreach (var item in items)   NoFooterMenu.Items.Add(item);

        // ── 2-column menu ─────────────────────────────────────────────────────
        foreach (var item in items)   TwoColMenu.Items.Add(item);
        foreach (var fi in footerItems) TwoColMenu.FooterItems.Add(fi);
    }
}
