using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using PleasantUI.Core.Localization;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages;

/// <summary>
/// Base class for all pages. Resolves <see cref="Title"/> from the localizer
/// and fires <see cref="PropertyChanged"/> whenever the language changes so
/// bindings in the UI update instantly.
/// </summary>
public abstract class LocalizedPage : IPage
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public abstract string TitleKey { get; }

    public string Title =>
        Localizer.Instance.TryGetString(TitleKey, out string value) ? value : TitleKey;

    public abstract bool ShowTitle { get; }

    public abstract Control Content { get; }

    protected LocalizedPage()
    {
        Localizer.Instance.LocalizationChanged += OnLocalizationChanged;
    }

    private void OnLocalizationChanged(string _)
    {
        if (Dispatcher.UIThread.CheckAccess())
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
        else
            Dispatcher.UIThread.Post(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title))));
    }
}
