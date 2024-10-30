using System.Collections;
using Avalonia.Controls;

namespace PleasantUI.Core.Extensions;

/// <summary>
/// Contains extension methods for <see cref="TabControl"/>.
/// </summary>
public static class TabViewExtensions
{
    /// <summary>
    /// Removes the TabItem.
    /// </summary>
    /// <param name="tabControl">The TabControl Parent</param>
    /// <param name="tabItem">The TabItem to Remove</param>
    public static void CloseTab(this TabControl tabControl, object tabItem)
    {
        try
        {
            ((IList)tabControl.Items).Remove(tabItem);
        }
        catch (Exception e)
        {
            throw new Exception("The TabItem does not exist", e);
        }
    }
}