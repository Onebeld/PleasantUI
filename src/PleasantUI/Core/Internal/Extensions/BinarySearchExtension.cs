/*
 * SPDX-FileCopyrightText: 2026 Dmitry Zhutkov (Onebeld) <onebeld@gmail.com>
 * SPDX-FileCopyrightText: 2020 Brad Robinson (toptensoftware) <github@toptensoftware.com>
 * SPDX-License-Identifier: Apache-2.0
 *
 * Derived from:
 * https://github.com/toptensoftware/RichTextKit/blob/8038a9b252c03fc9ee4807e5db3817a3628626e0/Topten.RichTextKit/Utils/BinarySearchExtension.cs
 */

namespace PleasantUI.Core.Internal.Extensions;

internal static class BinarySearchExtension
{
    private static int GetMedian(int low, int hi)
    {
        System.Diagnostics.Debug.Assert(low <= hi);
        System.Diagnostics.Debug.Assert(hi - low >= 0, "Length overflow!");
        return low + (hi - low >> 1);
    }

    /// <summary>
    /// Performs a binary search on the entire contents of an IReadOnlyList
    /// </summary>
    /// <typeparam name="T">The list element type</typeparam>
    /// <param name="list">The list to be searched</param>
    /// <param name="value">The value to search for</param>
    /// <param name="comparer">The comparer</param>
    /// <returns>The index of the found item; otherwise the bitwise complement of the index of the next larger item</returns>
    public static int BinarySearch<T>(this IReadOnlyList<T> list, T value, IComparer<T> comparer)
    {
        return list.BinarySearch(0, list.Count, value, comparer);
    }

    /// <summary>
    /// Performs a binary search on a subset of an IReadOnlyList
    /// </summary>
    /// <typeparam name="T">The list element type</typeparam>
    /// <param name="list">The list to be searched</param>
    /// <param name="index">The start of the range to be searched</param>
    /// <param name="length">The length of the range to be searched</param>
    /// <param name="value">The value to search for</param>
    /// <param name="comparer">A comparer</param>
    /// <returns>The index of the found item; otherwise the bitwise complement of the index of the next larger item</returns>
    public static int BinarySearch<T>(this IReadOnlyList<T> list, int index, int length, T value, IComparer<T> comparer)
    {
        // Based on this: https://referencesource.microsoft.com/#mscorlib/system/array.cs,957
        int lo = index;
        int hi = index + length - 1;
        while (lo <= hi)
        {
            int i = GetMedian(lo, hi);
            int c = comparer.Compare(list[i], value);
            if (c == 0) return i;
            if (c < 0)
            {
                lo = i + 1;
            }
            else
            {
                hi = i - 1;
            }
        }

        return ~lo;
    }
}