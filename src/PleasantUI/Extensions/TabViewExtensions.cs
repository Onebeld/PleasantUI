using System.Collections;
using Avalonia.Controls;

namespace PleasantUI.Extensions;

public static class TabViewExtensions
{
    /// <summary>
    ///     Removes the TabItem.
    /// </summary>
    /// <param name="tabControl">The TabControl Parent</param>
    /// <param name="tabItem">The TabItem to Remove</param>
    public static void CloseTab(this TabControl tabControl, object tabItem)
    {
        try
        {
            ((IList)tabControl.Items!).Remove(tabItem);
        }
        catch (Exception e)
        {
            throw new Exception("The TabItem does not exist", e);
        }
    }

    /// <summary>
    ///     Removes a TabItem with its index number.
    /// </summary>
    /// <param name="tabControl">A TabControl Parent</param>
    /// <param name="index">The TabItem Index</param>
    public static void CloseTab(this TabControl tabControl, int index)
    {
        index--;
        try
        {
            if (index < 0)
            {
            }
            else
            {
                ((IList)tabControl.Items!).RemoveAt(index);
            }
        }
        catch (Exception e)
        {
            throw new Exception("the index must be greater than 0", e);
        }
    }

    /// <summary>
    ///     Add a TabItem
    /// </summary>
    /// <param name="tabControl">The TabControl Parent</param>
    /// <param name="tabItemToAdd">The TabItem to Add</param>
    /// <param name="focus"></param>
    /// <returns>
    ///     If the method has been done correctly,returns bool if it has been done correctly or false if it has been done
    ///     incorrectly
    /// </returns>
    public static void AddTab(this TabControl tabControl, TabItem tabItemToAdd, bool focus = true)
    {
        try
        {
            ((IList)tabControl.Items!).Add(tabItemToAdd);
            switch (focus)
            {
                case true:
                    tabItemToAdd.IsSelected = true;
                    break;
            }
        }
        catch (SystemException e)
        {
            throw new SystemException("The Item to add is null", e);
        }
    }
}