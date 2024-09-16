using System.Collections;

namespace PleasantUI.Extensions;

/// <summary>
/// Provides miscellaneous extension methods.
/// </summary>
public static class MiscExtensions
{
    /// <summary>
    /// Inserts multiple copies of the same item into a list at the specified index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to insert into.</param>
    /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
    /// <param name="item">The item to insert multiple times.</param>
    /// <param name="count">The number of copies of the item to insert.</param>
    public static void InsertMany<T>(this List<T> list, int index, T item, int count)
    {
        FastRepeat<T> repeat = FastRepeat<T>.Instance;
        repeat.Count = count;
        repeat.Item = item;
        list.InsertRange(index, FastRepeat<T>.Instance);
        repeat.Item = default;
    }
}

file class FastRepeat<T> : ICollection<T>
{
    public static readonly FastRepeat<T> Instance = [];
    public T? Item { get; set; }
    public int Count { get; set; }
    public bool IsReadOnly => true;

    public void Add(T item)
    {
        throw new NotSupportedException();
    }

    public void Clear()
    {
        throw new NotSupportedException();
    }

    public bool Contains(T item)
    {
        throw new NotSupportedException();
    }

    public bool Remove(T item)
    {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotSupportedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotSupportedException();
    }

    public void CopyTo(T?[] array, int arrayIndex)
    {
        int end = arrayIndex + Count;

        for (int i = arrayIndex; i < end; ++i) array[i] = Item;
    }
}