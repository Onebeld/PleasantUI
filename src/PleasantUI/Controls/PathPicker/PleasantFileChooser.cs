using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PleasantUI.Controls;

/// <summary>
/// A PleasantUI-styled file/folder chooser control.
/// All state is exposed as <see cref="AvaloniaProperty"/> so the AXAML template
/// uses only <c>{TemplateBinding}</c> — no DataContext or x:DataType required.
/// </summary>
[TemplatePart(PART_QuickLinks,    typeof(ListBox))]
[TemplatePart(PART_FileList,      typeof(ListBox))]
[TemplatePart(PART_LocationBox,   typeof(TextBox))]
[TemplatePart(PART_FileNameBox,   typeof(TextBox))]
[TemplatePart(PART_FilterBox,     typeof(TextBox))]
[TemplatePart(PART_FilterCombo,   typeof(ComboBox))]
[TemplatePart(PART_OkButton,      typeof(Button))]
[TemplatePart(PART_CancelButton,  typeof(Button))]
[TemplatePart(PART_BackButton,    typeof(Button))]
[TemplatePart(PART_ForwardButton, typeof(Button))]
[TemplatePart(PART_UpButton,      typeof(Button))]
public class PleasantFileChooser : TemplatedControl
{
    /// <summary>Template part: quick-access sidebar list.</summary>
    public const string PART_QuickLinks    = "PART_QuickLinks";
    /// <summary>Template part: main file/folder list.</summary>
    public const string PART_FileList      = "PART_FileList";
    /// <summary>Template part: current path text box.</summary>
    public const string PART_LocationBox   = "PART_LocationBox";
    /// <summary>Template part: file name entry box.</summary>
    public const string PART_FileNameBox   = "PART_FileNameBox";
    /// <summary>Template part: name filter text box.</summary>
    public const string PART_FilterBox     = "PART_FilterBox";
    /// <summary>Template part: file-type filter combo box.</summary>
    public const string PART_FilterCombo   = "PART_FilterCombo";
    /// <summary>Template part: OK / Open / Save button.</summary>
    public const string PART_OkButton      = "PART_OkButton";
    /// <summary>Template part: Cancel button.</summary>
    public const string PART_CancelButton  = "PART_CancelButton";
    /// <summary>Template part: navigate back button.</summary>
    public const string PART_BackButton    = "PART_BackButton";
    /// <summary>Template part: navigate forward button.</summary>
    public const string PART_ForwardButton = "PART_ForwardButton";
    /// <summary>Template part: navigate up button.</summary>
    public const string PART_UpButton      = "PART_UpButton";

    // ── Styled properties (bound via TemplateBinding in AXAML) ────────────────

    /// <summary>Defines the <see cref="QuickLinks"/> property.</summary>
    public static readonly StyledProperty<ObservableCollection<PleasantFileChooserItem>> QuickLinksProperty =
        AvaloniaProperty.Register<PleasantFileChooser, ObservableCollection<PleasantFileChooserItem>>(
            nameof(QuickLinks), defaultValue: []);

    /// <summary>Defines the <see cref="Items"/> property.</summary>
    public static readonly StyledProperty<ObservableCollection<PleasantFileChooserItem>> ItemsProperty =
        AvaloniaProperty.Register<PleasantFileChooser, ObservableCollection<PleasantFileChooserItem>>(
            nameof(Items), defaultValue: []);

    /// <summary>Defines the <see cref="SelectedItems"/> property.</summary>
    public static readonly StyledProperty<ObservableCollection<PleasantFileChooserItem>> SelectedItemsProperty =
        AvaloniaProperty.Register<PleasantFileChooser, ObservableCollection<PleasantFileChooserItem>>(
            nameof(SelectedItems), defaultValue: []);

    /// <summary>Defines the <see cref="Filters"/> property.</summary>
    public static readonly StyledProperty<AvaloniaList<PleasantFileChooserFilter>> FiltersProperty =
        AvaloniaProperty.Register<PleasantFileChooser, AvaloniaList<PleasantFileChooserFilter>>(
            nameof(Filters), defaultValue: []);

    /// <summary>Defines the <see cref="CurrentPath"/> property.</summary>
    public static readonly StyledProperty<string> CurrentPathProperty =
        AvaloniaProperty.Register<PleasantFileChooser, string>(nameof(CurrentPath), string.Empty);

    /// <summary>Defines the <see cref="FileName"/> property.</summary>
    public static readonly StyledProperty<string> FileNameProperty =
        AvaloniaProperty.Register<PleasantFileChooser, string>(nameof(FileName), string.Empty);

    /// <summary>Defines the <see cref="FilterText"/> property.</summary>
    public static readonly StyledProperty<string> FilterTextProperty =
        AvaloniaProperty.Register<PleasantFileChooser, string>(nameof(FilterText), string.Empty);

    /// <summary>Defines the <see cref="SelectedFilterIndex"/> property.</summary>
    public static readonly StyledProperty<int> SelectedFilterIndexProperty =
        AvaloniaProperty.Register<PleasantFileChooser, int>(nameof(SelectedFilterIndex));

    /// <summary>Defines the <see cref="CanGoBack"/> property.</summary>
    public static readonly StyledProperty<bool> CanGoBackProperty =
        AvaloniaProperty.Register<PleasantFileChooser, bool>(nameof(CanGoBack));

    /// <summary>Defines the <see cref="CanGoForward"/> property.</summary>
    public static readonly StyledProperty<bool> CanGoForwardProperty =
        AvaloniaProperty.Register<PleasantFileChooser, bool>(nameof(CanGoForward));

    /// <summary>Defines the <see cref="CanGoUp"/> property.</summary>
    public static readonly StyledProperty<bool> CanGoUpProperty =
        AvaloniaProperty.Register<PleasantFileChooser, bool>(nameof(CanGoUp));

    /// <summary>Defines the <see cref="IsLoading"/> property.</summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<PleasantFileChooser, bool>(nameof(IsLoading));

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Quick-access sidebar items.</summary>
    public ObservableCollection<PleasantFileChooserItem> QuickLinks
    {
        get => GetValue(QuickLinksProperty);
        set => SetValue(QuickLinksProperty, value);
    }

    /// <summary>Filtered file/folder items shown in the main list.</summary>
    public ObservableCollection<PleasantFileChooserItem> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>Currently selected items.</summary>
    public ObservableCollection<PleasantFileChooserItem> SelectedItems
    {
        get => GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>File-type filter entries shown in the filter combo box.</summary>
    public AvaloniaList<PleasantFileChooserFilter> Filters
    {
        get => GetValue(FiltersProperty);
        set => SetValue(FiltersProperty, value);
    }

    /// <summary>Current directory path shown in the location bar.</summary>
    public string CurrentPath
    {
        get => GetValue(CurrentPathProperty);
        set => SetValue(CurrentPathProperty, value);
    }

    /// <summary>File name shown in the bottom entry box.</summary>
    public string FileName
    {
        get => GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    /// <summary>Text used to filter items by name.</summary>
    public string FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>Index of the selected file-type filter.</summary>
    public int SelectedFilterIndex
    {
        get => GetValue(SelectedFilterIndexProperty);
        set => SetValue(SelectedFilterIndexProperty, value);
    }

    /// <summary>Whether the Back navigation button is enabled.</summary>
    public bool CanGoBack
    {
        get => GetValue(CanGoBackProperty);
        private set => SetValue(CanGoBackProperty, value);
    }

    /// <summary>Whether the Forward navigation button is enabled.</summary>
    public bool CanGoForward
    {
        get => GetValue(CanGoForwardProperty);
        private set => SetValue(CanGoForwardProperty, value);
    }

    /// <summary>Whether the Up navigation button is enabled.</summary>
    public bool CanGoUp
    {
        get => GetValue(CanGoUpProperty);
        private set => SetValue(CanGoUpProperty, value);
    }

    /// <summary>Whether a directory load is in progress.</summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        private set => SetValue(IsLoadingProperty, value);
    }

    // ── Private fields ────────────────────────────────────────────────────────

    private PleasantFileChooserViewModel? _vm;

    private ListBox?  _quickLinks;
    private ListBox?  _fileList;
    private TextBox?  _locationBox;
    private TextBox?  _fileNameBox;
    private TextBox?  _filterBox;
    private ComboBox? _filterCombo;
    private Button?   _okButton;
    private Button?   _cancelButton;
    private Button?   _backButton;
    private Button?   _forwardButton;
    private Button?   _upButton;

    // ── Static constructor ────────────────────────────────────────────────────

    static PleasantFileChooser()
    {
        // Sync control properties → ViewModel when changed from AXAML/code
        CurrentPathProperty.Changed.AddClassHandler<PleasantFileChooser>(
            (c, e) => { if (c._vm is not null && e.NewValue is string p) c._vm.Navigate(p); });

        FileNameProperty.Changed.AddClassHandler<PleasantFileChooser>(
            (c, e) => { if (c._vm is not null && e.NewValue is string n) c._vm.FileName = n; });

        FilterTextProperty.Changed.AddClassHandler<PleasantFileChooser>(
            (c, e) => { if (c._vm is not null && e.NewValue is string t) c._vm.FilterText = t; });

        SelectedFilterIndexProperty.Changed.AddClassHandler<PleasantFileChooser>(
            (c, e) => { if (c._vm is not null && e.NewValue is int i) c._vm.SelectedFilterIndex = i; });
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the chooser with the given ViewModel.
    /// Syncs all ViewModel state into the control's styled properties.
    /// </summary>
    public void SetViewModel(PleasantFileChooserViewModel vm)
    {
        _vm = vm;

        // Populate collections
        QuickLinks.Clear();
        foreach (var q in vm.QuickLinks) QuickLinks.Add(q);

        Filters.Clear();
        foreach (var f in vm.Filters) Filters.Add(f);

        // Sync scalar state
        CurrentPath         = vm.CurrentPath;
        FileName            = vm.FileName;
        FilterText          = vm.FilterText;
        SelectedFilterIndex = vm.SelectedFilterIndex;
        CanGoBack           = vm.CanGoBack;
        CanGoForward        = vm.CanGoForward;
        CanGoUp             = vm.CanGoUp;

        // Subscribe to ViewModel property changes → push back to control properties
        vm.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(PleasantFileChooserViewModel.CurrentPath):
                    SetCurrentValue(CurrentPathProperty, vm.CurrentPath);
                    break;
                case nameof(PleasantFileChooserViewModel.FileName):
                    SetCurrentValue(FileNameProperty, vm.FileName);
                    break;
                case nameof(PleasantFileChooserViewModel.CanGoBack):
                    CanGoBack = vm.CanGoBack;
                    break;
                case nameof(PleasantFileChooserViewModel.CanGoForward):
                    CanGoForward = vm.CanGoForward;
                    break;
                case nameof(PleasantFileChooserViewModel.CanGoUp):
                    CanGoUp = vm.CanGoUp;
                    break;
                case nameof(PleasantFileChooserViewModel.IsLoading):
                    IsLoading = vm.IsLoading;
                    break;
            }
        };

        // Subscribe to Items collection changes
        vm.Items.CollectionChanged += (_, _) => SyncItems();
        SyncItems();

        _ = vm.LoadDirectoryAsync(vm.CurrentPath);
    }

    private void SyncItems()
    {
        Items.Clear();
        if (_vm is null) return;
        foreach (var item in _vm.Items) Items.Add(item);
    }

    // ── Template ──────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Detach();

        _quickLinks    = e.NameScope.Find<ListBox>(PART_QuickLinks);
        _fileList      = e.NameScope.Find<ListBox>(PART_FileList);
        _locationBox   = e.NameScope.Find<TextBox>(PART_LocationBox);
        _fileNameBox   = e.NameScope.Find<TextBox>(PART_FileNameBox);
        _filterBox     = e.NameScope.Find<TextBox>(PART_FilterBox);
        _filterCombo   = e.NameScope.Find<ComboBox>(PART_FilterCombo);
        _okButton      = e.NameScope.Find<Button>(PART_OkButton);
        _cancelButton  = e.NameScope.Find<Button>(PART_CancelButton);
        _backButton    = e.NameScope.Find<Button>(PART_BackButton);
        _forwardButton = e.NameScope.Find<Button>(PART_ForwardButton);
        _upButton      = e.NameScope.Find<Button>(PART_UpButton);

        Attach();
    }

    private void Attach()
    {
        if (_quickLinks    is not null) _quickLinks.SelectionChanged  += OnQuickLinkSelected;
        if (_fileList      is not null)
        {
            _fileList.DoubleTapped      += OnFileListDoubleTapped;
            _fileList.SelectionChanged  += OnFileListSelectionChanged;
        }
        if (_locationBox   is not null) _locationBox.KeyDown          += OnLocationKeyDown;
        if (_filterBox     is not null) _filterBox.TextChanged        += OnFilterTextChanged;
        if (_filterCombo   is not null) _filterCombo.SelectionChanged += OnFilterComboChanged;
        if (_okButton      is not null) _okButton.Click               += OnOkClicked;
        if (_cancelButton  is not null) _cancelButton.Click           += OnCancelClicked;
        if (_backButton    is not null) _backButton.Click             += OnBackClicked;
        if (_forwardButton is not null) _forwardButton.Click          += OnForwardClicked;
        if (_upButton      is not null) _upButton.Click               += OnUpClicked;
    }

    private void Detach()
    {
        if (_quickLinks    is not null) _quickLinks.SelectionChanged  -= OnQuickLinkSelected;
        if (_fileList      is not null)
        {
            _fileList.DoubleTapped      -= OnFileListDoubleTapped;
            _fileList.SelectionChanged  -= OnFileListSelectionChanged;
        }
        if (_locationBox   is not null) _locationBox.KeyDown          -= OnLocationKeyDown;
        if (_filterBox     is not null) _filterBox.TextChanged        -= OnFilterTextChanged;
        if (_filterCombo   is not null) _filterCombo.SelectionChanged -= OnFilterComboChanged;
        if (_okButton      is not null) _okButton.Click               -= OnOkClicked;
        if (_cancelButton  is not null) _cancelButton.Click           -= OnCancelClicked;
        if (_backButton    is not null) _backButton.Click             -= OnBackClicked;
        if (_forwardButton is not null) _forwardButton.Click          -= OnForwardClicked;
        if (_upButton      is not null) _upButton.Click               -= OnUpClicked;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnQuickLinkSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_vm is null || _quickLinks?.SelectedItem is not PleasantFileChooserItem item) return;
        _vm.Navigate(item.FullPath);
        _quickLinks.SelectedItem = null;
    }

    private void OnFileListDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (_vm is null || _fileList?.SelectedItem is not PleasantFileChooserItem item) return;
        _vm.OpenItem(item);
    }

    private void OnFileListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_vm is null || _fileList is null) return;

        _vm.SelectedItems.Clear();
        foreach (var obj in _fileList.SelectedItems)
        {
            if (obj is PleasantFileChooserItem item)
                _vm.SelectedItems.Add(item);
        }
    }

    private void OnLocationKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && _vm is not null && _locationBox is not null)
        {
            _vm.Navigate(_locationBox.Text ?? string.Empty);
            e.Handled = true;
        }
    }

    private void OnFilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_vm is not null && _filterBox is not null)
            _vm.FilterText = _filterBox.Text ?? string.Empty;
    }

    private void OnFilterComboChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_vm is not null && _filterCombo is not null)
            _vm.SelectedFilterIndex = _filterCombo.SelectedIndex;
    }

    private void OnOkClicked(object? sender, RoutedEventArgs e)     => _vm?.Confirm();
    private void OnCancelClicked(object? sender, RoutedEventArgs e)  => _vm?.Cancel();
    private void OnBackClicked(object? sender, RoutedEventArgs e)    => _vm?.GoBack();
    private void OnForwardClicked(object? sender, RoutedEventArgs e) => _vm?.GoForward();
    private void OnUpClicked(object? sender, RoutedEventArgs e)      => _vm?.GoUp();

    // ── Static show API ───────────────────────────────────────────────────────

    /// <summary>
    /// Shows the file chooser as a <see cref="ContentDialog"/> and returns the selected paths,
    /// or null if the user cancelled.
    /// </summary>
    public static async Task<IReadOnlyList<string>?> ShowAsync(
        TopLevel topLevel,
        PleasantFileChooserOptions options)
    {
        var vm = new PleasantFileChooserViewModel
        {
            Title         = options.Title ?? "Open",
            AllowMultiple = options.AllowMultiple,
            FoldersOnly   = options.FoldersOnly,
            ShowHidden    = options.ShowHidden,
            Filters       = options.Filters ?? [],
            CurrentPath   = options.InitialDirectory
                            ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        var chooser = new PleasantFileChooser();
        chooser.SetViewModel(vm);

        var tcs    = new TaskCompletionSource<IReadOnlyList<string>?>();
        var dialog = new ContentDialog
        {
            Content   = chooser,
            Padding   = new Thickness(0),
            MaxWidth  = 760,
            MaxHeight = 520
        };

        bool resultSet = false;
        vm.CloseRequested += async (_, _) =>
        {
            if (resultSet) return;
            resultSet = true;
            await dialog.CloseAsync();
            tcs.TrySetResult(vm.Result);
        };

        await dialog.ShowAsync(topLevel);
        tcs.TrySetResult(null);
        return await tcs.Task;
    }
}

/// <summary>Options for <see cref="PleasantFileChooser.ShowAsync"/>.</summary>
public sealed class PleasantFileChooserOptions
{
    /// <summary>Dialog title.</summary>
    public string? Title { get; set; }

    /// <summary>Initial directory to open.</summary>
    public string? InitialDirectory { get; set; }

    /// <summary>Whether multiple items can be selected.</summary>
    public bool AllowMultiple { get; set; }

    /// <summary>Whether to show only directories.</summary>
    public bool FoldersOnly { get; set; }

    /// <summary>Whether to show hidden files.</summary>
    public bool ShowHidden { get; set; }

    /// <summary>File-type filters. An empty extensions list means "all files".</summary>
    public IReadOnlyList<PleasantFileChooserFilter>? Filters { get; set; }
}
