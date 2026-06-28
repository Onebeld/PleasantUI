using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;

namespace PleasantUI.Core.Common;

public class FilteredCollectionView : IList, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly IEnumerable _source;
    private readonly List<object> _filteredItems = new();
    private string _filterText = string.Empty;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public FilteredCollectionView(IEnumerable source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        
        if (source is INotifyCollectionChanged observable)
            observable.CollectionChanged += OnSourceCollectionChanged;
            
        RefreshFilter();
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (_filterText == value) return;
            _filterText = value;
            RefreshFilter();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterText)));
        }
    }

    private void RefreshFilter()
    {
        _filteredItems.Clear();
        
        foreach (object? item in _source)
        {
            if (string.IsNullOrEmpty(_filterText) || (item?.ToString() ?? string.Empty).Contains(_filterText, StringComparison.OrdinalIgnoreCase))
                _filteredItems.Add(item!);
        }

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshFilter();
    }

    public int Count => _filteredItems.Count;
    public bool IsSynchronized { get; }
    public object SyncRoot { get; }
    public bool IsReadOnly => true;
    public object? this[int index] { get => _filteredItems[index]; set => throw new NotSupportedException(); }
    
    public IEnumerator GetEnumerator() => _filteredItems.GetEnumerator();
    public int Add(object? value) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public bool Contains(object? value) => _filteredItems.Contains(value);
    public int IndexOf(object? value) => _filteredItems.IndexOf(value);
    public void Insert(int index, object? value) => throw new NotSupportedException();
    public void Remove(object? value) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();
    public bool IsFixedSize { get; }
    public void CopyTo(Array array, int index) => _filteredItems.CopyTo((object[])array, index);
}