using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using PleasantUI.Core.Extensions;

namespace PleasantUI.Controls;

/// <summary>
/// A control that hosts submenu items for NavigationViewItem's popup.
/// This is a separate control instance that creates its own NavigationViewItem containers,
/// avoiding visual parent conflicts with the inline ItemsPresenter.
/// </summary>
public class NavigationViewSubMenuControl : ItemsControl
{
    private ItemsPresenter? _itemsPresenter;

    /// <summary>
    /// Defines the <see cref="NavigationViewItem" /> property.
    /// </summary>
    public static readonly StyledProperty<NavigationViewItem?> NavigationViewItemProperty =
        AvaloniaProperty.Register<NavigationViewSubMenuControl, NavigationViewItem?>(nameof(NavigationViewItem));

    /// <summary>
    /// Gets or sets the parent NavigationViewItem that owns this submenu.
    /// </summary>
    public NavigationViewItem? NavigationViewItem
    {
        get => GetValue(NavigationViewItemProperty);
        set => SetValue(NavigationViewItemProperty, value);
    }

    static NavigationViewSubMenuControl()
    {
        // No ItemTemplate - we create NavigationViewItem containers directly
        ItemTemplateProperty.OverrideDefaultValue<NavigationViewSubMenuControl>(null);
    }

    public NavigationViewSubMenuControl()
    {
        Classes.Add("navigationViewSubMenu");
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsPresenter = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
    }

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        // Always create a fresh NavigationViewItem container
        var container = new NavigationViewItem();
        return container;
    }

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        // Always create a new container, never reuse the item directly
        recycleKey = null;
        return true;
    }

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control element, object? item, int index)
    {
        base.PrepareContainerForItemOverride(element, item, index);

        if (element is NavigationViewItem navItem && item is not null)
        {
            // The item is either:
            // 1. A NavigationViewItem (from LogicalChildren) - we need to copy its properties
            // 2. A data object - we need to set it as the container's data context
            
            if (item is NavigationViewItem sourceItem)
            {
                // Copy all relevant properties from the source NavigationViewItem
                navItem.Header = sourceItem.Header;
                navItem.HeaderTemplate = sourceItem.HeaderTemplate;
                navItem.Icon = sourceItem.Icon;
                navItem.Content = sourceItem.Content;
                navItem.Title = sourceItem.Title;
                navItem.Tag = sourceItem.Tag;
                navItem.IsSelected = sourceItem.IsSelected;
                navItem.SelectOnClose = sourceItem.SelectOnClose;
                navItem.ClickMode = sourceItem.ClickMode;
                
                // Clone the children recursively - create NEW NavigationViewItem instances
                // Do NOT add the original children as they already have visual parents
                foreach (var child in sourceItem.Items.OfType<NavigationViewItem>())
                {
                    var clonedChild = CloneNavigationViewItem(child);
                    navItem.Items.Add(clonedChild);
                }
            }
            else
            {
                // It's a data object, set it as the DataContext
                navItem.DataContext = item;
                navItem.Header = item;
            }

            // Apply styling from parent NavigationViewItem and propagate NavigationView reference
            var parent = NavigationViewItem;
            if (parent is not null)
            {
                navItem.CompactPaneLength = parent.CompactPaneLength;
                navItem.OpenPaneLength = parent.OpenPaneLength;
                navItem.IsOpen = true; // Submenu items always show as open in popup
                // Propagate the NavigationView reference so popup items can trigger navigation
                navItem.NavigationView = parent.NavigationView ?? parent.GetParentTOfLogical<NavigationView>();
            }
        }
    }

    /// <inheritdoc />
    protected override void ClearContainerForItemOverride(Control element)
    {
        base.ClearContainerForItemOverride(element);

        if (element is NavigationViewItem navItem)
        {
            // Clear properties to prepare for recycling
            navItem.Header = null;
            navItem.Icon = null;
            navItem.Content = null;
            navItem.DataContext = null;
            
            // Reset distance
            navItem.SetValue(NavigationViewItem.NavigationViewDistanceProperty, 0);
        }
    }

    /// <summary>
    /// Recursively clones a NavigationViewItem and all its children.
    /// Creates completely new instances to avoid visual parent conflicts.
    /// </summary>
    private static NavigationViewItem CloneNavigationViewItem(NavigationViewItem source)
    {
        var clone = new NavigationViewItem
        {
            Header = source.Header,
            HeaderTemplate = source.HeaderTemplate,
            Icon = source.Icon,
            Content = source.Content,
            Title = source.Title,
            Tag = source.Tag,
            IsSelected = source.IsSelected,
            SelectOnClose = source.SelectOnClose,
            ClickMode = source.ClickMode,
            IsOpen = true // Submenu items always show as open in popup
        };

        // Recursively clone all children using public Items property
        foreach (var child in source.Items.OfType<NavigationViewItem>())
        {
            var clonedChild = CloneNavigationViewItem(child);
            clone.Items.Add(clonedChild);
        }

        return clone;
    }
}
