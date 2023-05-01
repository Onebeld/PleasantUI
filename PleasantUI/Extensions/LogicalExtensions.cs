using Avalonia.LogicalTree;

namespace PleasantUI.Extensions;

public static class LogicalExtensions
{
    /// <summary>
    /// Return a parent of the ILogical indicated
    /// </summary>
    /// <param name="logical">The control to get its parent</param>
    /// <returns>the parent of the control</returns>
    public static T? GetParentTOfLogical<T>(this ILogical logical) where T : class
    {
        return logical.GetSelfAndLogicalAncestors().OfType<T>().FirstOrDefault();
    }
    
    internal static int CalculateDistanceFromLogicalParent<T>(ILogical logical, int @default = -1) where T : class, ILogical
    {
        int result = 0;

        while (logical != null && logical is not T)
        {
            ++result;
            logical = logical.LogicalParent;
        }

        return logical != null ? result : @default;
    }
}