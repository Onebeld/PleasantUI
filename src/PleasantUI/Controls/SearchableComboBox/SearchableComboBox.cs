using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace PleasantUI.Controls;

[TemplatePart("PART_SearchTextBox", typeof(TextBox))]
public class SearchableComboBox : ComboBox
{
    public static readonly StyledProperty<string?> SearchTextProperty =
        AvaloniaProperty.Register<SearchableComboBox, string?>(nameof(SearchText), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsSearchEnabledProperty =
        AvaloniaProperty.Register<SearchableComboBox, bool>(nameof(IsSearchEnabled), true);

    private TextBox? _searchTextBox;
    private List<object> _allItems = new();
    private bool _isFiltering;

    static SearchableComboBox()
    {
        SearchTextProperty.Changed.AddClassHandler<SearchableComboBox>((x, e) => x.OnSearchTextChanged(e));
        IsDropDownOpenProperty.Changed.AddClassHandler<SearchableComboBox>((x, e) => x.OnIsDropDownOpenChanged(e));
    }

    public string? SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public bool IsSearchEnabled
    {
        get => GetValue(IsSearchEnabledProperty);
        set => SetValue(IsSearchEnabledProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _searchTextBox = e.NameScope.Find<TextBox>("PART_SearchTextBox");

        if (_searchTextBox != null)
        {
            _searchTextBox.TextChanged += OnSearchTextBoxTextChanged;
        }
    }

    private void OnSearchTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_searchTextBox != null && !_isFiltering)
            SetCurrentValue(SearchTextProperty, _searchTextBox.Text);
    }

    private void OnSearchTextChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (IsDropDownOpen)
            ApplyFilter();
    }

    private void OnIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool)
        {
            SetCurrentValue(SearchTextProperty, string.Empty);
            FocusSearchBox();
        }
        else
        {
            RestoreOriginalItems();
        }
    }

    private void FocusSearchBox()
    {
        if (_searchTextBox == null) return;

        Dispatcher.UIThread.Post(() =>
        {
            _searchTextBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            _searchTextBox.SelectAll();
        }, DispatcherPriority.Render);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemsSourceProperty)
        {
            CacheAllItems();
        }
    }

    private void CacheAllItems()
    {
        _allItems.Clear();
        if (ItemsSource is IEnumerable enumerable)
        {
            _allItems.AddRange(enumerable.Cast<object>());
        }
    }

    private void ApplyFilter()
    {
        if (!IsSearchEnabled || _isFiltering) return;

        _isFiltering = true;
        try
        {
            string term = (SearchText ?? "").Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(term))
            {
                RestoreOriginalItems();
                return;
            }

            var filtered = _allItems
                .Where(item => GetItemText(item).ToLowerInvariant().Contains(term))
                .ToList();

            var selected = SelectedItem;
            if (selected != null && !filtered.Contains(selected))
                filtered.Insert(0, selected);

            // Важно: меняем ItemsSource только когда это безопасно
            ItemsSource = new List<object>(filtered);
        }
        finally
        {
            _isFiltering = false;
        }
    }

    private void RestoreOriginalItems()
    {
        if (_allItems.Count == 0) return;

        var selected = SelectedItem;
        ItemsSource = new List<object>(_allItems);

        if (selected != null)
            SelectedItem = selected;
    }

    private string GetItemText(object? item)
    {
        if (item == null) return string.Empty;
        return item.ToString() ?? string.Empty;
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
    }
}