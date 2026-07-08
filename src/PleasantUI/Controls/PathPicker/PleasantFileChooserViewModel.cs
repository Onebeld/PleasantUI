using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace PleasantUI.Controls;

/// <summary>
/// ViewModel for <see cref="PleasantFileChooser"/>.
/// Drives navigation, filtering, selection, and result production.
/// </summary>
public sealed class PleasantFileChooserViewModel : INotifyPropertyChanged
{
    private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private string _filterText = string.Empty;
    private int _selectedFilterIndex;

    /// <summary>Dialog title.</summary>
    public string Title { get; set; } = "Open";

    /// <summary>Whether multiple items can be selected.</summary>
    public bool AllowMultiple { get; set; }

    /// <summary>Whether to show only directories (folder picker mode).</summary>
    public bool FoldersOnly { get; set; }

    /// <summary>Whether to show hidden files/folders.</summary>
    public bool ShowHidden { get; set; }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Optional file-type filters.
    /// </summary>
    public IReadOnlyList<PleasantFileChooserFilter> Filters { get; set; } = [];

    /// <summary>
    /// Gets or sets the current directory path being displayed.
    /// Setting this property triggers asynchronous loading of the directory contents.
    /// </summary>
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            Set(ref _currentPath, value);
            _ = LoadDirectoryAsync(value);
        }
    }

    /// <summary>
    /// Gets or sets the filename (or space-separated list of filenames when multiple selection is enabled).
    /// </summary>
    public string FileName
    {
        get;
        set => Set(ref field, value);
    } = string.Empty;

    /// <summary>
    /// Gets or sets the text used to filter items by name in the current directory.
    /// Changing this value automatically updates the visible <see cref="Items"/>.
    /// </summary>
    public string FilterText
    {
        get => _filterText;
        set
        {
            Set(ref _filterText, value);
            ApplyFilter();
        }
    }

    /// <summary>
    /// Gets or sets the index of the currently selected filter from the <see cref="Filters"/> collection.
    /// Changing this value automatically updates the visible <see cref="Items"/>.
    /// </summary>
    public int SelectedFilterIndex
    {
        get => _selectedFilterIndex;
        set
        {
            Set(ref _selectedFilterIndex, value);
            ApplyFilter();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the directory contents are currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get;
        private set => Set(ref field, value);
    }

    /// <summary>
    /// Gets the error message if directory loading failed; otherwise an empty string.
    /// </summary>
    public string ErrorMessage
    {
        get;
        private set => Set(ref field, value);
    } = string.Empty;

    /// <summary>Quick-access locations (drives + common folders).</summary>
    public ObservableCollection<PleasantFileChooserItem> QuickLinks { get; } = [];

    /// <summary>All items in the current directory (unfiltered).</summary>
    private readonly List<PleasantFileChooserItem> _allItems = [];

    /// <summary>Filtered items shown in the file list.</summary>
    public ObservableCollection<PleasantFileChooserItem> Items { get; } = [];

    /// <summary>Currently selected items.</summary>
    public ObservableCollection<PleasantFileChooserItem> SelectedItems { get; } = [];

    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();

    /// <summary>Set to the chosen paths when the user confirms, or null on cancel.</summary>
    public IReadOnlyList<string>? Result { get; private set; }

    /// <summary>Raised when the dialog should close (confirm or cancel).</summary>
    public event EventHandler? CloseRequested;

    public PleasantFileChooserViewModel()
    {
        SelectedItems.CollectionChanged += (_, _) => UpdateFileNameFromSelection();
        BuildQuickLinks();
    }

    /// <summary>
    /// Gets a value indicating whether the user can navigate to the parent directory.
    /// </summary>
    public bool CanGoUp => !string.IsNullOrEmpty(Path.GetDirectoryName(_currentPath));
    
    /// <summary>
    /// Gets a value indicating whether the user can navigate backward in history.
    /// </summary>
    public bool CanGoBack => _backStack.Count > 0;
    
    /// <summary>
    /// Gets a value indicating whether the user can navigate forward in history.
    /// </summary>
    public bool CanGoForward => _forwardStack.Count > 0;

    /// <summary>
    /// Navigates to the parent directory of the current path, if one exists.
    /// </summary>
    public void GoUp()
    {
        string? parent = Path.GetDirectoryName(_currentPath);
        if (parent is not null) Navigate(parent);
    }

    /// <summary>
    /// Navigates backward in the navigation history.
    /// </summary>
    public void GoBack()
    {
        if (!CanGoBack) return;
        _forwardStack.Push(_currentPath);
        string prev = _backStack.Pop();
        Set(ref _currentPath, prev, nameof(CurrentPath));
        _ = LoadDirectoryAsync(prev);
        NotifyNavigation();
    }

    /// <summary>
    /// Navigates forward in the navigation history.
    /// </summary>
    public void GoForward()
    {
        if (!CanGoForward) return;
        _backStack.Push(_currentPath);
        string next = _forwardStack.Pop();
        Set(ref _currentPath, next, nameof(CurrentPath));
        _ = LoadDirectoryAsync(next);
        NotifyNavigation();
    }

    /// <summary>
    /// Navigates to the specified directory path and updates navigation history.
    /// </summary>
    /// <param name="path">The absolute path to navigate to.</param>
    public void Navigate(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == _currentPath) return;
        _backStack.Push(_currentPath);
        _forwardStack.Clear();
        Set(ref _currentPath, path, nameof(CurrentPath));
        _ = LoadDirectoryAsync(path);
        NotifyNavigation();
    }

    /// <summary>
    /// Opens the specified item. If it is a directory, navigates into it; 
    /// otherwise confirms the selection.
    /// </summary>
    /// <param name="item">The item to open.</param>
    public void OpenItem(PleasantFileChooserItem item)
    {
        if (item.IsDirectory)
            Navigate(item.FullPath);
        else
            Confirm();
    }

    private void NotifyNavigation()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoUp)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoBack)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanGoForward)));
    }

    /// <summary>
    /// Asynchronously loads the contents of the specified directory and updates the item collections.
    /// </summary>
    /// <param name="path">The directory path to load.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadDirectoryAsync(string path)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            List<PleasantFileChooserItem> items = await Task.Run(() => EnumerateItems(path));

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _allItems.Clear();
                _allItems.AddRange(items);
                SelectedItems.Clear();
                ApplyFilter();
                IsLoading = false;
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ErrorMessage = ex.Message;
                IsLoading = false;
            });
        }
    }

    private List<PleasantFileChooserItem> EnumerateItems(string path)
    {
        List<PleasantFileChooserItem> result = new();

        DirectoryInfo dirInfo = new(path);
        if (!dirInfo.Exists) return result;

        // Directories first
        foreach (DirectoryInfo dir in dirInfo.EnumerateDirectories()
                     .Where(d => ShowHidden || (d.Attributes & FileAttributes.Hidden) == 0)
                     .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
        {
            result.Add(new PleasantFileChooserItem(dir.FullName, true, dir.LastWriteTime));
        }

        if (!FoldersOnly)
        {
            foreach (FileInfo file in dirInfo.EnumerateFiles()
                         .Where(f => ShowHidden || (f.Attributes & FileAttributes.Hidden) == 0)
                         .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(new PleasantFileChooserItem(file.FullName, false, file.LastWriteTime, file.Length));
            }
        }

        return result;
    }

    private void ApplyFilter()
    {
        HashSet<string>? activeExtensions = GetActiveExtensions();
        string nameFilter = _filterText.Trim();

        Items.Clear();
        foreach (PleasantFileChooserItem item in _allItems)
        {
            if (item.IsDirectory)
            {
                Items.Add(item);
                continue;
            }

            if (!string.IsNullOrEmpty(nameFilter) &&
                !item.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (activeExtensions is not null &&
                !activeExtensions.Contains(item.Extension))
                continue;

            Items.Add(item);
        }
    }

    private HashSet<string>? GetActiveExtensions()
    {
        if (Filters.Count == 0) return null;
        PleasantFileChooserFilter filter = Filters[Math.Clamp(_selectedFilterIndex, 0, Filters.Count - 1)];
        if (filter.Extensions.Count == 0) return null;
        return new HashSet<string>(filter.Extensions, StringComparer.OrdinalIgnoreCase);
    }

    private void UpdateFileNameFromSelection()
    {
        if (SelectedItems.Count == 0) return;

        FileName = SelectedItems.Count == 1
            ? SelectedItems[0].Name
            : string.Join(" ", SelectedItems.Select(i => $"\"{i.Name}\""));
    }

    /// <summary>
    /// Confirms the current selection and closes the dialog with the selected paths.
    /// Uses <see cref="SelectedItems"/> if any are selected, otherwise falls back to the <see cref="FileName"/> property.
    /// </summary>
    public void Confirm()
    {
        List<string> paths = new();

        if (SelectedItems.Count > 0)
        {
            paths.AddRange(SelectedItems.Select(i => i.FullPath));
        }
        else if (!string.IsNullOrWhiteSpace(FileName))
        {
            string typed = FileName.Trim();
            paths.Add(Path.IsPathRooted(typed) ? typed : Path.Combine(_currentPath, typed));
        }

        if (paths.Count == 0) return;

        Result = paths;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Cancels the dialog operation and closes it without returning any result.
    /// </summary>
    public void Cancel()
    {
        Result = null;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void BuildQuickLinks()
    {
        QuickLinks.Clear();

        // Common folders
        AddQuickLink(Environment.SpecialFolder.Desktop);
        AddQuickLink(Environment.SpecialFolder.MyDocuments);
        AddQuickLink(Environment.SpecialFolder.MyPictures);
        AddQuickLink(Environment.SpecialFolder.MyMusic);
        AddQuickLink(Environment.SpecialFolder.MyVideos);
        AddQuickLink(Environment.SpecialFolder.UserProfile);

        // Drives
        foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            QuickLinks.Add(new PleasantFileChooserItem(drive.RootDirectory.FullName, true));
    }

    private void AddQuickLink(Environment.SpecialFolder folder)
    {
        try
        {
            string path = Environment.GetFolderPath(folder);
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                QuickLinks.Add(new PleasantFileChooserItem(path, true));
        }
        catch
        {
            /* skip unavailable folders */
        }
    }
}