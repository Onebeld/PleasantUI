using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;

namespace PleasantUI.Controls;

/// <summary>
/// A control that combines a <see cref="TextBox"/> and a browse <see cref="Button"/> to let
/// the user pick one or more file-system paths via the platform storage picker.
/// </summary>
[TemplatePart(PART_Button, typeof(Button))]
[TemplatePart(PART_TextBox, typeof(TextBox))]
[PseudoClasses(PC_Empty, PC_HasValue)]
public partial class PathPicker : TemplatedControl
{
    private const string PART_Button = "PART_Button";
    private const string PART_TextBox = "PART_TextBox";
    private const string PC_Empty = ":empty";
    private const string PC_HasValue = ":hasvalue";

    private const string FilterPattern = @"^\[(?:[^*.,]+|([^*.,]+(,[^,]+)+))\]$";

#if NET8_0_OR_GREATER
    [GeneratedRegex(FilterPattern)]
    private static partial Regex FilterRegex();
#else
    private static readonly Regex _filterRegex = new(FilterPattern);
    private static Regex FilterRegex() => _filterRegex;
#endif

    private Button? _button;
    private bool _syncLock;

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<PathPicker, string?>(nameof(Title));

    /// <summary>Defines the <see cref="ButtonContent"/> property.</summary>
    public static readonly StyledProperty<object?> ButtonContentProperty =
        AvaloniaProperty.Register<PathPicker, object?>(nameof(ButtonContent));

    /// <summary>Defines the <see cref="Mode"/> property.</summary>
    public static readonly StyledProperty<PathPickerMode> ModeProperty =
        AvaloniaProperty.Register<PathPicker, PathPickerMode>(nameof(Mode));

    /// <summary>Defines the <see cref="AllowMultiple"/> property.</summary>
    public static readonly StyledProperty<bool> AllowMultipleProperty =
        AvaloniaProperty.Register<PathPicker, bool>(nameof(AllowMultiple));

    /// <summary>Defines the <see cref="SuggestedStartPath"/> property.</summary>
    public static readonly StyledProperty<string?> SuggestedStartPathProperty =
        AvaloniaProperty.Register<PathPicker, string?>(nameof(SuggestedStartPath));

    /// <summary>Defines the <see cref="SuggestedFileName"/> property.</summary>
    public static readonly StyledProperty<string?> SuggestedFileNameProperty =
        AvaloniaProperty.Register<PathPicker, string?>(nameof(SuggestedFileName));

    /// <summary>Defines the <see cref="FileFilter"/> property.</summary>
    public static readonly StyledProperty<string?> FileFilterProperty =
        AvaloniaProperty.Register<PathPicker, string?>(nameof(FileFilter));

    /// <summary>Defines the <see cref="DefaultFileExtension"/> property.</summary>
    public static readonly StyledProperty<string?> DefaultFileExtensionProperty =
        AvaloniaProperty.Register<PathPicker, string?>(nameof(DefaultFileExtension));

    /// <summary>Defines the <see cref="SelectedPaths"/> property.</summary>
    public static readonly DirectProperty<PathPicker, IReadOnlyList<string>> SelectedPathsProperty =
        AvaloniaProperty.RegisterDirect<PathPicker, IReadOnlyList<string>>(
            nameof(SelectedPaths), o => o.SelectedPaths, (o, v) => o.SelectedPaths = v);

    /// <summary>Defines the <see cref="SelectedPathsText"/> property.</summary>
    public static readonly StyledProperty<string?> SelectedPathsTextProperty =
        AvaloniaProperty.Register<PathPicker, string?>(nameof(SelectedPathsText),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the <see cref="Command"/> property.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<PathPicker, ICommand?>(nameof(Command));

    /// <summary>Defines the <see cref="IsOmitCommandOnCancel"/> property.</summary>
    public static readonly StyledProperty<bool> IsOmitCommandOnCancelProperty =
        AvaloniaProperty.Register<PathPicker, bool>(nameof(IsOmitCommandOnCancel));

    /// <summary>Defines the <see cref="IsClearSelectionOnCancel"/> property.</summary>
    public static readonly StyledProperty<bool> IsClearSelectionOnCancelProperty =
        AvaloniaProperty.Register<PathPicker, bool>(nameof(IsClearSelectionOnCancel));

    /// <summary>Defines the <see cref="UseCustomPicker"/> property.</summary>
    public static readonly StyledProperty<bool> UseCustomPickerProperty =
        AvaloniaProperty.Register<PathPicker, bool>(nameof(UseCustomPicker), defaultValue: false);

    /// <summary>Defines the <see cref="VerticalContentAlignment"/> property.</summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<PathPicker, VerticalAlignment>(nameof(VerticalContentAlignment),
            VerticalAlignment.Center);

    /// <summary>Gets or sets the dialog title shown in the platform picker.</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets the content of the browse button (defaults to a folder icon).</summary>
    public object? ButtonContent
    {
        get => GetValue(ButtonContentProperty);
        set => SetValue(ButtonContentProperty, value);
    }

    /// <summary>Gets or sets whether to open files, save a file, or open folders.</summary>
    public PathPickerMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>Gets or sets whether multiple paths can be selected (OpenFile / OpenFolder only).</summary>
    public bool AllowMultiple
    {
        get => GetValue(AllowMultipleProperty);
        set => SetValue(AllowMultipleProperty, value);
    }

    /// <summary>Gets or sets the folder the picker opens in by default.</summary>
    public string? SuggestedStartPath
    {
        get => GetValue(SuggestedStartPathProperty);
        set => SetValue(SuggestedStartPathProperty, value);
    }

    /// <summary>Gets or sets the pre-filled file name for SaveFile mode.</summary>
    public string? SuggestedFileName
    {
        get => GetValue(SuggestedFileNameProperty);
        set => SetValue(SuggestedFileNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the file-type filter string.
    /// Format: <c>[Name,*.ext,*.ext2][Name2,*.ext3]</c> or a built-in name such as
    /// <c>[All][ImageAll]</c>.
    /// </summary>
    public string? FileFilter
    {
        get => GetValue(FileFilterProperty);
        set => SetValue(FileFilterProperty, value);
    }

    /// <summary>Gets or sets the default file extension for SaveFile mode (e.g. <c>.txt</c>).</summary>
    public string? DefaultFileExtension
    {
        get => GetValue(DefaultFileExtensionProperty);
        set => SetValue(DefaultFileExtensionProperty, value);
    }

    /// <summary>Gets or sets the selected paths as a newline-separated string (two-way bindable).</summary>
    public string? SelectedPathsText
    {
        get => GetValue(SelectedPathsTextProperty);
        set => SetValue(SelectedPathsTextProperty, value);
    }

    /// <summary>Gets or sets the command executed after the picker closes (receives <see cref="SelectedPaths"/>).</summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>When true, <see cref="Command"/> is not executed if the user cancels the picker.</summary>
    public bool IsOmitCommandOnCancel
    {
        get => GetValue(IsOmitCommandOnCancelProperty);
        set => SetValue(IsOmitCommandOnCancelProperty, value);
    }

    /// <summary>When true, the selection is cleared if the user cancels the picker.</summary>
    public bool IsClearSelectionOnCancel
    {
        get => GetValue(IsClearSelectionOnCancelProperty);
        set => SetValue(IsClearSelectionOnCancelProperty, value);
    }

    /// <summary>
    /// When true, uses the built-in <see cref="PleasantFileChooser"/> UI instead of
    /// the platform-native storage picker dialog.
    /// </summary>
    public bool UseCustomPicker
    {
        get => GetValue(UseCustomPickerProperty);
        set => SetValue(UseCustomPickerProperty, value);
    }

    /// <summary>Gets or sets the vertical alignment of the content.</summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>Gets the list of selected paths.</summary>
    public IReadOnlyList<string> SelectedPaths
    {
        get;
        private set => SetAndRaise(SelectedPathsProperty, ref field, value);
    } = [];

    static PathPicker() { }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == SelectedPathsProperty || e.Property == SelectedPathsTextProperty)
        {
            SyncPathsToText();
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_button is not null) _button.Click -= OnButtonClick;

        _button = e.NameScope.Find<Button>(PART_Button);

        if (_button is not null) _button.Click += OnButtonClick;

        UpdatePseudoClasses();
    }

    private void SyncPathsToText()
    {
        if (_syncLock) return;
        _syncLock = true;
        SelectedPathsText = string.Join(Environment.NewLine, SelectedPaths);
        _syncLock = false;
        UpdatePseudoClasses();
    }

    private void SyncTextToPaths()
    {
        if (_syncLock) return;
        _syncLock = true;
        string[] separators = ["\r\n", "\r", "\n"];
        SelectedPaths = (SelectedPathsText?
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToArray()) ?? [];
        _syncLock = false;
        UpdatePseudoClasses();
    }

    private void UpdatePseudoClasses()
    {
        bool hasValue = SelectedPaths.Count > 0;
        PseudoClasses.Set(PC_Empty, !hasValue);
        PseudoClasses.Set(PC_HasValue, hasValue);
    }

    private async void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        try
        {
            _button?.IsEnabled = false;

            IReadOnlyList<string>? picked;

            if (UseCustomPicker)
            {
                List<PleasantFileChooserFilter> filters = ParseFileFilter(FileFilter)?
                                                              .Select(f => new PleasantFileChooserFilter(
                                                                  f.Name,
                                                                  f.Patterns?
                                                                      .Select(p => p.TrimStart('*'))
                                                                      .ToArray() ?? []))
                                                              .ToList()
                                                          ?? new List<PleasantFileChooserFilter>();

                picked = await PleasantFileChooser.ShowAsync(topLevel, new PleasantFileChooserOptions
                {
                    Title = Title,
                    AllowMultiple = AllowMultiple,
                    FoldersOnly = Mode == PathPickerMode.OpenFolder,
                    InitialDirectory = SuggestedStartPath,
                    Filters = filters
                });
            }
            else
            {
                if (topLevel.StorageProvider is not { } storage)
                {
                    return;
                }

                IReadOnlyList<string?> raw = Mode switch
                {
                    PathPickerMode.OpenFile => await PickOpenFileAsync(storage),
                    PathPickerMode.SaveFile => await PickSaveFileAsync(storage),
                    PathPickerMode.OpenFolder => await PickOpenFolderAsync(storage),
                    _ => []
                };

                picked = raw.Where(p => p is not null).Select(p => p!).ToList();
            }

            List<string> nonNull = picked?.Where(p => !string.IsNullOrEmpty(p)).ToList()
                                   ?? [];

            if (nonNull.Count > 0)
            {
                SelectedPaths = nonNull;
            }
            else if (IsClearSelectionOnCancel)
            {
                SelectedPaths = [];
            }

            if (nonNull.Count > 0 || !IsOmitCommandOnCancel)
                Command?.Execute(SelectedPaths);
        }
        finally
        {
            _button?.IsEnabled = true;
        }
    }

    private async Task<IReadOnlyList<string?>> PickOpenFileAsync(IStorageProvider storage)
    {
        IReadOnlyList<IStorageFile> result = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Title,
            AllowMultiple = AllowMultiple,
            SuggestedStartLocation = await TryGetFolderAsync(storage, SuggestedStartPath),
            FileTypeFilter = ParseFileFilter(FileFilter)
        });
        return result.Select(f => f.TryGetLocalPath()).ToArray();
    }

    private async Task<IReadOnlyList<string?>> PickSaveFileAsync(IStorageProvider storage)
    {
        IStorageFile? result = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Title,
            SuggestedStartLocation = await TryGetFolderAsync(storage, SuggestedStartPath),
            SuggestedFileName = SuggestedFileName,
            FileTypeChoices = ParseFileFilter(FileFilter),
            DefaultExtension = DefaultFileExtension
        });
        return result is null ? [] : [result.TryGetLocalPath()];
    }

    private async Task<IReadOnlyList<string?>> PickOpenFolderAsync(IStorageProvider storage)
    {
        IReadOnlyList<IStorageFolder> result = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Title,
            AllowMultiple = AllowMultiple,
            SuggestedStartLocation = await TryGetFolderAsync(storage, SuggestedStartPath)
        });
        return result.Select(f => f.TryGetLocalPath()).ToArray();
    }

    private static async Task<IStorageFolder?> TryGetFolderAsync(IStorageProvider storage, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        try
        {
            return await storage.TryGetFolderFromPathAsync(path);
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyList<FilePickerFileType>? ParseFileFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return null;

        if (!FilterRegex().IsMatch(filter))
            throw new ArgumentException(
                "Invalid FileFilter. Expected format: [Name,*.ext,*.ext2] or built-in names like [All][ImageAll].",
                nameof(filter));

        string[] separators = ["[", "][", "]"];
        return filter
            .Replace(" ", "")
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseSingleFilter)
            .ToArray();
    }

    private static FilePickerFileType ParseSingleFilter(string token) => token switch
    {
        nameof(FilePickerFileTypes.All) => FilePickerFileTypes.All,
        nameof(FilePickerFileTypes.Pdf) => FilePickerFileTypes.Pdf,
        nameof(FilePickerFileTypes.ImageAll) => FilePickerFileTypes.ImageAll,
        nameof(FilePickerFileTypes.ImageJpg) => FilePickerFileTypes.ImageJpg,
        nameof(FilePickerFileTypes.ImagePng) => FilePickerFileTypes.ImagePng,
        nameof(FilePickerFileTypes.ImageWebp) => FilePickerFileTypes.ImageWebp,
        nameof(FilePickerFileTypes.TextPlain) => FilePickerFileTypes.TextPlain,
        _ => ParseCustomFilter(token)
    };

    private static FilePickerFileType ParseCustomFilter(string token)
    {
        string[] parts = token.Split(',');
        return new FilePickerFileType(parts[0])
        {
            Patterns = parts.Skip(1).Select(p => p.Trim()).ToArray()
        };
    }
}