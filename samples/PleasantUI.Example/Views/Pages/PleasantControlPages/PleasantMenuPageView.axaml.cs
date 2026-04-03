using Avalonia.Controls;
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
            MakeItem(MaterialIcons.FolderOpenOutline,  labelKey: "Open",   labelDefault: "Open",   tipKey: "OpenTip",   tipDefault: "Open a file"),
            MakeItem(MaterialIcons.ContentSaveOutline, labelKey: "Save",   labelDefault: "Save",   tipKey: "SaveTip",   tipDefault: "Save current file"),
            MakeItem(MaterialIcons.PlusCircleOutline,  labelKey: "New",    labelDefault: "New",    tipKey: "NewTip",    tipDefault: "Create new file"),
            MakeItem(MaterialIcons.WrenchOutline,      labelKey: "Tools",  labelDefault: "Tools",  tipKey: "ToolsTip",  tipDefault: "Open tools"),
            MakeItem(MaterialIcons.HomeOutline,        labelKey: "Home",   labelDefault: "Home",   tipKey: "HomeTip",   tipDefault: "Go to home screen"),
            MakeItem(MaterialIcons.DeleteOutline,      labelKey: "Delete", labelDefault: "Delete", tipKey: "DeleteTip", tipDefault: "Delete selected", isEnabled: false)
        ];

        PleasantMenuFooterItem[] footerItems =
        [
            MakeFooterItem(MaterialIcons.CogOutline,         tipKey: "Settings", tipDefault: "Settings", alignRight: false),
            MakeFooterItem(MaterialIcons.InformationOutline, tipKey: "About",    tipDefault: "About",    alignRight: true),
            MakeFooterItem(MaterialIcons.ExitToApp,          tipKey: "Exit",     tipDefault: "Exit",     alignRight: true)
        ];

        var badges = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                MakeBadge(MaterialIcons.FolderOutline,        "3", tipKey: "OpenFiles", tipDefault: "Open files"),
                MakeBadge(MaterialIcons.PackageVariantClosed, "5", tipKey: "Modules",   tipDefault: "Loaded modules")
            }
        };

        foreach (var item in items)     DemoMenu.Items.Add(item);
        foreach (var fi in footerItems) DemoMenu.FooterItems.Add(fi);
        DemoMenu.Badges = badges;

        foreach (var item in items) NoFooterMenu.Items.Add(item);

        foreach (var item in items)     TwoColMenu.Items.Add(item);
        foreach (var fi in footerItems) TwoColMenu.FooterItems.Add(fi);
    }
    // Complex constructor — don't re-run InitializeComponent

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        BuildMenus();
    }

    private static PleasantMenuItem MakeItem(
        Avalonia.Media.Geometry? icon,
        string labelKey,
        string labelDefault,
        string tipKey,
        string tipDefault,
        bool isEnabled = true)
    {
        var item = new PleasantMenuItem
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
        Avalonia.Media.Geometry? icon,
        string tipKey,
        string tipDefault,
        bool alignRight)
    {
        var item = new PleasantMenuFooterItem
        {
            Icon = icon,
            AlignRight = alignRight
        };

        item.Bind(PleasantMenuFooterItem.ToolTipProperty, LocalizeBinding.Create(tipKey, context: "PleasantMenu", @default: tipDefault));
        return item;
    }

    private static InformationBlock MakeBadge(Avalonia.Media.Geometry icon, string content, string tipKey, string tipDefault)
    {
        var badge = new InformationBlock
        {
            Icon = icon,
            Content = content
        };

        badge.Bind(ToolTip.TipProperty, LocalizeBinding.Create(tipKey, context: "PleasantMenu", @default: tipDefault));
        return badge;
    }
}
