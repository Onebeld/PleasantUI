using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PleasantUI.Controls;
using PleasantUI.Core;
using PleasantUI.Core.Extensions;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Localization;
using PleasantUI.Core.Models;
using PleasantUI.Core.Models.Interfaces;
using PleasantUI.ToolKit.Messages;
using PleasantUI.ToolKit.Models;
using PleasantUI.ToolKit.Services;
using PleasantUI.ToolKit.Services.Interfaces;
using PleasantUI.ToolKit.Utils;

namespace PleasantUI.ToolKit.ViewModels;

/// <summary>
/// ViewModel for the <see cref="PleasantUI.ToolKit.ThemeEditorWindow" />.
/// </summary>
public class ThemeEditorViewModel : ViewModelBase, IDisposable
{
    private readonly ThemeEditorWindow _themeEditorWindow;
    private readonly IPleasantWindow _pleasantWindowParent;

    private CustomTheme _customTheme;

    private readonly IThemeService _themeService;

    private IDisposable _copySub;
    private IDisposable _pasteColorSub;
    private IDisposable _requestColorSub;
    private IDisposable _colorChangedSub;

    /// <summary>
    /// Gets the resource dictionary containing the theme colors.
    /// </summary>
    public ResourceDictionary ResourceDictionary => _themeService.ResourceDictionary;

    /// <summary>
    /// Gets or sets the custom theme being edited.
    /// </summary>
    public CustomTheme CustomTheme
    {
        get => _customTheme;
        set => SetProperty(ref _customTheme, value);
    }

    /// <summary>
    /// Gets or sets the name of the theme.
    /// </summary>
    public string ThemeName
    {
        get => _themeService.ThemeName;
        set
        {
            _themeService.ThemeName = value;
            
            UpdateProperties();
        }
    }

    /// <summary>
    /// Gets or sets the list of available default themes.
    /// </summary>
    public AvaloniaList<Theme> Themes { get; } = new();

    /// <summary>
    /// Gets the list of theme colors.
    /// </summary>
    public AvaloniaList<ThemeColor> ThemeColors => _themeService.ThemeColors;

    /// <summary>
    /// Gets the colors of the custom theme in JSON format.
    /// </summary>
    public string JsonColors => _themeService.JsonColors;

    /// <summary>
    /// Gets a value indicating whether an undo operation is possible.
    /// </summary>
    public bool CanUndo => _themeService.CanUndo;

    /// <summary>
    /// Gets a value indicating whether a redo operation is possible.
    /// </summary>
    public bool CanRedo => _themeService.CanRedo;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeEditorViewModel" /> class.
    /// </summary>
    /// <param name="windowParent">The parent window.</param>
    /// <param name="themeEditorWindow">The theme editor window.</param>
    /// <param name="customTheme">The custom theme to edit (optional).</param>
    public ThemeEditorViewModel(IPleasantWindow windowParent, ThemeEditorWindow themeEditorWindow, CustomTheme? customTheme)
    {
        _pleasantWindowParent = windowParent;
        _customTheme = customTheme ?? new CustomTheme(null, "NewTheme", PleasantTheme.GetThemeTemplateDictionary());
        _themeService = new ThemeService(TopLevel.GetTopLevel(_pleasantWindowParent as Visual), new EventAggregator());
        
        _themeService.ChangeThemeName(_customTheme.Name);
        _themeService.CreateThemeColors(_customTheme);

        foreach (Theme theme in PleasantTheme.Themes)
        {
            if (theme.Name is "System" or "Custom")
                continue;

            Themes.Add(theme);
        }

        themeEditorWindow.ThemeVariantScope.Resources.ThemeDictionaries.Add(_customTheme.ThemeVariant,
            ResourceDictionary);
        _themeEditorWindow = themeEditorWindow;

        RaisePropertyChanged(nameof(JsonColors));
        RegisterMessages();
    }

    /// <summary>
    /// Undoes the last change.
    /// </summary>
    public void Undo()
    {
        if (!_themeService.Undo())
            return;

        UpdateProperties();
    }

    /// <summary>
    /// Redoes the last undone change.
    /// </summary>
    public void Redo()
    {
        if (!_themeService.Redo())
            return;
        
        UpdateProperties();
    }

    /// <summary>
    /// Copies the theme colors in JSON format to the clipboard.
    /// </summary>
    public async Task CopyThemeAsync()
    {
        await _themeService.CopyThemeAsync();
        
        PleasantSnackbar.Show(_pleasantWindowParent, new PleasantSnackbarOptions(Localizer.TrDefault("ThemeCopiedToClipboard", defaultString: "Theme copied to clipboard"))
        {
            Icon = ResourceExtensions.GetResource<Geometry>("CopyRegular")
        });
    }

    /// <summary>
    /// Parses a JSON string from the clipboard and applies its colors to the current theme.
    /// </summary>
    public async Task PasteThemeAsync()
    {
        bool result = await _themeService.PasteThemeAsync();
        
        ShowImportThemeResultMessage(result);
        
        UpdateProperties();
    }

    /// <summary>
    /// Opens a file picker for the user to select a JSON file containing a theme, and applies the theme's colors to the current theme.
    /// </summary>
    public async Task ImportThemeAsync()
    {
        IReadOnlyList<IStorageFile> pickerFileTypes = await _themeService.GetTopLevel().StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                FileTypeFilter = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }],
                AllowMultiple = false
            });

        if (pickerFileTypes.Count == 0)
            return;

        string json = await File.ReadAllTextAsync(pickerFileTypes[0].Path.ToDecodedLocalPath());

        bool result = _themeService.ImportJson(json);

        ShowImportThemeResultMessage(result);
        
        UpdateProperties();
    }

    /// <summary>
    /// Opens a save file picker for the user to select a location to export the current theme as a JSON file.
    /// </summary>
    public async Task ExportThemeAsync()
    {
        IStorageFile? result = await _themeService.GetTopLevel().StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            FileTypeChoices = [new FilePickerFileType("JSON") { Patterns = ["*.json"] }],
            DefaultExtension = "json",
            SuggestedFileName = ThemeName
        });

        if (result is null)
            return;

        await _themeService.ExportThemeAsync(result.Path.ToDecodedLocalPath());
        
        PleasantSnackbar.Show(_pleasantWindowParent, new PleasantSnackbarOptions(Localizer.TrDefault("ThemeExported", defaultString: "Theme exported"))
        {
            Icon = ResourceExtensions.GetResource<Geometry>("FileExportRegular"),
            NotificationType = NotificationType.Success
        });
    }

    /// <summary>
    /// Applies the colors from the specified theme.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public void GetColorsFromTheme(ITheme theme)
    {
        _themeEditorWindow.ButtonThemesFlyout.Flyout?.Hide();
        
        _themeService.GetColorsFromTheme(theme);

        UpdateProperties();
    }

    /// <summary>
    /// Applies the theme colors from the specified JSON file.
    /// </summary>
    /// <param name="file">The JSON file to apply the theme colors from.</param>
    public void DropFile(IStorageItem file)
    {
        string json = File.ReadAllText(file.Path.ToDecodedLocalPath());

        bool result = _themeService.ImportJson(json);

        ShowImportThemeResultMessage(result);
        
        UpdateProperties();
    }

    private void ShowImportThemeResultMessage(bool result)
    {
        if (result)
        {
            PleasantSnackbar.Show(_pleasantWindowParent, new PleasantSnackbarOptions(Localizer.TrDefault("ThemeImported", defaultString: "The theme has been successfully imported"))
            {
                Icon = ResourceExtensions.GetResource<Geometry>("FileImportRegular"),
                NotificationType = NotificationType.Success
            });
        }
        else
        {
            PleasantSnackbar.Show(_pleasantWindowParent, new PleasantSnackbarOptions(Localizer.TrDefault("ThemeImportError", defaultString: "An error occurred while importing the theme"))
            {
                Icon = ResourceExtensions.GetResource<Geometry>("CloseCircleRegular"),
                NotificationType = NotificationType.Error
            });
        }
    }

    private void UpdateProperties()
    {
        RaisePropertyChanged(nameof(CanUndo));
        RaisePropertyChanged(nameof(CanRedo));
        RaisePropertyChanged(nameof(JsonColors));
        RaisePropertyChanged(nameof(ThemeName));
    }

    private void RegisterMessages()
    {
        _copySub = _themeService.EventAggregator.Subscribe<ColorCopyMessage>(async message =>
        {
            await _themeService.CopyColorAsync(message.Color);
            
            PleasantSnackbar.Show(_pleasantWindowParent, new PleasantSnackbarOptions(Localizer.TrDefault("ColorCopiedToClipboard", defaultString: "The color is copied to the clipboard"))
            {
                Icon = ResourceExtensions.GetResource<Geometry>("CopyRegular")
            });
        });

        _pasteColorSub = _themeService.EventAggregator.Subscribe<PasteColorMessage>(async message =>
        {
            Color? color = await _themeService.PasteColorAsync();
            message.TaskCompletionSource.TrySetResult(color);
        });

        _requestColorSub = _themeService.EventAggregator.Subscribe<ChangeColorMessage>(async message =>
        {
            Color? color = await ColorPickerWindow.SelectColor(_pleasantWindowParent, message.PreviousColor.ToUInt32());
            message.TaskCompletionSource.TrySetResult(color);
        });

        _colorChangedSub = _themeService.EventAggregator.Subscribe<ChangedColorMessage>(async message =>
        {
            _themeService.ChangeColor(message.ThemeColor, message.NewColor, message.PreviousColor);
            
            UpdateProperties();
            
            await Task.CompletedTask;
        });
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}