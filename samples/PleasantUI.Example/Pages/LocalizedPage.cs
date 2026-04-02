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

    private void OnLocalizationChanged(string lang)
    {
        void Notify()
        {
            System.Diagnostics.Debug.WriteLine($"[LocalizedPage] PropertyChanged Title for key={TitleKey} lang={lang}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
        }

        if (Dispatcher.UIThread.CheckAccess())
            Notify();
        else
            Dispatcher.UIThread.Post(Notify);
    }
}
