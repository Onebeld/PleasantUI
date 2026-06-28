using Avalonia.Controls;
using Material.Icons;
using PleasantUI.Controls;
using PleasantUI.Core.Localization;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class PleasantMenuPageView : LocalizedUserControl
{
    public PleasantMenuPageView()
    {
        InitializeComponent();
        BuildMenus();
    }

    private void BuildMenus()
    {
        // If we're rebuilding after a language switch, clear any existing items.
        DemoMenu.Items.Clear();
        DemoMenu.FooterItems.Clear();
        NoFooterMenu.Items.Clear();
        TwoColMenu.Items.Clear();
        TwoColMenu.FooterItems.Clear();

        PleasantMenuItem[] items =
        [
            MakeItem(MaterialIconKind.FolderOpenOutline,  labelKey: "Open",   labelDefault: "Open",   tipKey: "OpenTip",   tipDefault: "Open a file"),
            MakeItem(MaterialIconKind.ContentSaveOutline, labelKey: "Save",   labelDefault: "Save",   tipKey: "SaveTip",   tipDefault: "Save current file"),
            MakeItem(MaterialIconKind.PlusCircleOutline,  labelKey: "New",    labelDefault: "New",    tipKey: "NewTip",    tipDefault: "Create new file"),
            MakeItem(MaterialIconKind.WrenchOutline,      labelKey: "Tools",  labelDefault: "Tools",  tipKey: "ToolsTip",  tipDefault: "Open tools"),
            MakeItem(MaterialIconKind.HomeOutline,        labelKey: "Home",   labelDefault: "Home",   tipKey: "HomeTip",   tipDefault: "Go to home screen"),
            MakeItem(MaterialIconKind.DeleteOutline,      labelKey: "Delete", labelDefault: "Delete", tipKey: "DeleteTip", tipDefault: "Delete selected", isEnabled: false)
        ];

        PleasantMenuFooterItem[] footerItems =
        [
            MakeFooterItem(MaterialIconKind.CogOutline,         tipKey: "Settings", tipDefault: "Settings", alignRight: false),
            MakeFooterItem(MaterialIconKind.InformationOutline, tipKey: "About",    tipDefault: "About",    alignRight: true),
            MakeFooterItem(MaterialIconKind.ExitToApp,          tipKey: "Exit",     tipDefault: "Exit",     alignRight: true)
        ];

        StackPanel badges = new()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                MakeBadge(MaterialIconKind.FolderOutline,        "3", tipKey: "OpenFiles", tipDefault: "Open files"),
                MakeBadge(MaterialIconKind.PackageVariantClosed, "5", tipKey: "Modules",   tipDefault: "Loaded modules")
            }
        };

        foreach (PleasantMenuItem item in items)     DemoMenu.Items.Add(item);
        foreach (PleasantMenuFooterItem fi in footerItems) DemoMenu.FooterItems.Add(fi);
        DemoMenu.Badges = badges;

        foreach (PleasantMenuItem item in items) NoFooterMenu.Items.Add(item);

        foreach (PleasantMenuItem item in items)     TwoColMenu.Items.Add(item);
        foreach (PleasantMenuFooterItem fi in footerItems) TwoColMenu.FooterItems.Add(fi);
    }
    // Complex constructor — don't re-run InitializeComponent

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        BuildMenus();
    }

    private static PleasantMenuItem MakeItem(
        object? icon,
        string labelKey,
        string labelDefault,
        string tipKey,
        string tipDefault,
        bool isEnabled = true)
    {
        PleasantMenuItem item = new()
        {
            Icon = icon,
            IsEnabled = isEnabled,
            SecondaryCommand = null
        };

        item.Bind(PleasantMenuItem.LabelProperty,   LocalizeBinding.Create(labelKey, context: "PleasantMenu", @default: labelDefault));
        item.Bind(PleasantMenuItem.ToolTipProperty, LocalizeBinding.Create(tipKey,   context: "PleasantMenu", @default: tipDefault));
        return item;
    }

    private static PleasantMenuFooterItem MakeFooterItem(
        object? icon,
        string tipKey,
        string tipDefault,
        bool alignRight)
    {
        PleasantMenuFooterItem item = new()
        {
            Icon = icon,
            AlignRight = alignRight
        };

        item.Bind(PleasantMenuFooterItem.ToolTipProperty, LocalizeBinding.Create(tipKey, context: "PleasantMenu", @default: tipDefault));
        return item;
    }

    private static InformationBlock MakeBadge(object icon, string content, string tipKey, string tipDefault)
    {
        InformationBlock badge = new()
        {
            Icon = icon,
            Content = content
        };

        badge.Bind(ToolTip.TipProperty, LocalizeBinding.Create(tipKey, context: "PleasantMenu", @default: tipDefault));
        return badge;
    }
}
