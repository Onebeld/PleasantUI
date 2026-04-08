using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace PleasantUI.Controls;

/// <summary>
/// ViewModel for <see cref="PleasantFileChooser"/>.
/// Drives navigation, filtering, selection, and result production.
/// </summary>
public sealed class PleasantFileChooserViewModel : INotifyPropertyChanged
{
    // ── INotifyPropertyChanged ────────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ── Configuration (set before showing) ───────────────────────────────────

    /// <summary>Dialog title.</summary>
    public string Title { get; set; } = "Open";

    /// <summary>Whether multiple items can be selected.</summary>
    public bool AllowMultiple { get; set; }

    /// <summary>Whether to show only directories (folder picker mode).</summary>
    public bool FoldersOnly { get; set; }

    /// <summary>Whether to show hidden files/folders.</summary>
    public bool ShowHidden { get; set; }

    /// <summary>
    /// Optional file-type filters.
    /// </summary>
    public IReadOnlyList<PleasantFileChooserFilter> Filters { get; set; } = [];

    // ── State ─────────────────────────────────────────────────────────────────

    private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private string _fileName    = string.Empty;
    private string _filterText  = string.Empty;
    private int    _selectedFilterIndex;
    private bool   _isLoading;
    private string _errorMessage = string.Empty;

    public string CurrentPath
    {
        get => _currentPath;
        set { Set(ref _currentPath, value); _ = LoadDirectoryAsync(value); }
    }

    public string FileName
    {
        get => _fileName;
        set => Set(ref _fileName, value);
    }

    public string FilterText
    {
        get => _filterText;
        set { Set(ref _filterText, value); ApplyFilter(); }
    }

    public int SelectedFilterIndex
    {
        get => _selectedFilterIndex;
        set { Set(ref _selectedFilterIndex, value); ApplyFilter(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => Set(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => Set(ref _errorMessage, value);
    }

    // ── Collections ───────────────────────────────────────────────────────────

    /// <summary>Quick-access locations (drives + common folders).</summary>
    public ObservableCollection<PleasantFileChooserItem> QuickLinks { get; } = [];

    /// <summary>All items in the current directory (unfiltered).</summary>
    private readonly List<PleasantFileChooserItem> _allItems = [];

    /// <summary>Filtered items shown in the file list.</summary>
    public ObservableCollection<PleasantFileChooserItem> Items { get; } = [];

    /// <summary>Currently selected items.</summary>
    public ObservableCollection<PleasantFileChooserItem> SelectedItems { get; } = [];

    /// <summary>Navigation history for Back/Forward.</summary>
    private readonly Stack<string> _backStack  = new();
    private readonly Stack<string> _forwardStack = new();

    // ── Result ────────────────────────────────────────────────────────────────

    /// <summary>Set to the chosen paths when the user confirms, or null on cancel.</summary>
    public IReadOnlyList<string>? Result { get; private set; }

    /// <summary>Raised when the dialog should close (confirm or cancel).</summary>
    public event EventHandler? CloseRequested;

    // ── Constructor ───────────────────────────────────────────────────────────

    public PleasantFileChooserViewModel()
    {
        SelectedItems.CollectionChanged += (_, _) => UpdateFileNameFromSelection();
        BuildQuickLinks();
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    public bool CanGoUp => !string.IsNullOrEmpty(Path.GetDirectoryName(_currentPath));
    public bool CanGoBack    => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;

    public void GoUp()
    {
        var parent = Path.GetDirectoryName(_currentPath);
        if (parent is not null) Navigate(parent);
    }

    public void GoBack()
    {
        if (!CanGoBack) return;
        _forwardStack.Push(_currentPath);
        var prev = _backStack.Pop();
        Set(ref _currentPath, prev, nameof(CurrentPath));
        _ = LoadDirectoryAsync(prev);
        NotifyNavigation();
    }

    public void GoForward()
    {
        if (!CanGoForward) return;
        _backStack.Push(_currentPath);
        var next = _forwardStack.Pop();
        Set(ref _currentPath, next, nameof(CurrentPath));
        _ = LoadDirectoryAsync(next);
        NotifyNavigation();
    }

    public void Navigate(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == _currentPath) return;
        _backStack.Push(_currentPath);
        _forwardStack.Clear();
        Set(ref _currentPath, path, nameof(CurrentPath));
        _ = LoadDirectoryAsync(path);
        NotifyNavigation();
    }

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

    // ── Directory loading ─────────────────────────────────────────────────────

    public async Task LoadDirectoryAsync(string path)
    {
        IsLoading    = true;
        ErrorMessage = string.Empty;

        try
        {
            var items = await Task.Run(() => EnumerateItems(path));

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
                IsLoading    = false;
            });
        }
    }

    private List<PleasantFileChooserItem> EnumerateItems(string path)
    {
        var result = new List<PleasantFileChooserItem>();

        var dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists) return result;

        // Directories first
        foreach (var dir in dirInfo.EnumerateDirectories()
                     .Where(d => ShowHidden || (d.Attributes & FileAttributes.Hidden) == 0)
                     .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
        {
            result.Add(new PleasantFileChooserItem(dir.FullName, true, dir.LastWriteTime));
        }

        if (!FoldersOnly)
        {
            foreach (var file in dirInfo.EnumerateFiles()
                         .Where(f => ShowHidden || (f.Attributes & FileAttributes.Hidden) == 0)
                         .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(new PleasantFileChooserItem(file.FullName, false, file.LastWriteTime, file.Length));
            }
        }

        return result;
    }

    // ── Filtering ─────────────────────────────────────────────────────────────

    private void ApplyFilter()
    {
        var activeExtensions = GetActiveExtensions();
        var nameFilter       = _filterText.Trim();

        Items.Clear();
        foreach (var item in _allItems)
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
        var filter = Filters[Math.Clamp(_selectedFilterIndex, 0, Filters.Count - 1)];
        if (filter.Extensions.Count == 0) return null;
        return new HashSet<string>(filter.Extensions, StringComparer.OrdinalIgnoreCase);
    }

    // ── Selection → FileName sync ─────────────────────────────────────────────

    private void UpdateFileNameFromSelection()
    {
        if (SelectedItems.Count == 0) return;

        FileName = SelectedItems.Count == 1
            ? SelectedItems[0].Name
            : string.Join(" ", SelectedItems.Select(i => $"\"{i.Name}\""));

        Debug.WriteLine($"[FileChooserVM] UpdateFileNameFromSelection → FileName=\"{FileName}\"");
    }

    // ── Confirm / Cancel ──────────────────────────────────────────────────────

    public void Confirm()
    {
        var paths = new List<string>();

        if (SelectedItems.Count > 0)
        {
            paths.AddRange(SelectedItems.Select(i => i.FullPath));
            Debug.WriteLine($"[FileChooserVM] Confirm — from SelectedItems: [{string.Join(", ", paths)}]");
        }
        else if (!string.IsNullOrWhiteSpace(FileName))
        {
            var typed = FileName.Trim();
            paths.Add(Path.IsPathRooted(typed) ? typed : Path.Combine(_currentPath, typed));
            Debug.WriteLine($"[FileChooserVM] Confirm — from FileName \"{FileName}\": [{string.Join(", ", paths)}]");
        }
        else
        {
            Debug.WriteLine("[FileChooserVM] Confirm — nothing to confirm (no selection, no filename)");
        }

        if (paths.Count == 0) return;

        Result = paths;
        Debug.WriteLine($"[FileChooserVM] Confirm — firing CloseRequested with Result=[{string.Join(", ", Result)}]");
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Cancel()
    {
        Result = null;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    // ── Quick links ───────────────────────────────────────────────────────────

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
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            QuickLinks.Add(new PleasantFileChooserItem(drive.RootDirectory.FullName, true));
    }

    private void AddQuickLink(Environment.SpecialFolder folder)
    {
        try
        {
            var path = Environment.GetFolderPath(folder);
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                QuickLinks.Add(new PleasantFileChooserItem(path, true));
        }
        catch { /* skip unavailable folders */ }
    }
}
