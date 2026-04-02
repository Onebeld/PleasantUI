using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;
using PleasantUI.Core.Localization;
using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Pages;

/// <summary>
/// Base class for all pages. Manages a cached <see cref="Content"/> view that is
/// invalidated on every language change so the next navigation always gets a fresh
/// view with correct localized strings.
/// </summary>
public abstract class LocalizedPage : IPage
{
    private Control? _cachedContent;

    public event PropertyChangedEventHandler? PropertyChanged;

    public abstract string TitleKey { get; }

    public string Title =>
        Localizer.Instance.TryGetString(TitleKey, out string value) ? value : TitleKey;

    public abstract bool ShowTitle { get; }

    /// <summary>
    /// Returns the cached view, creating it fresh if the cache is empty.
    /// The cache is cleared on every language change so the next access
    /// creates a new view that reads the current language from scratch.
    /// </summary>
    public Control Content => _cachedContent ??= CreateContent();

    /// <summary>
    /// Override to create the view for this page.
    /// </summary>
    protected abstract Control CreateContent();

    protected LocalizedPage()
    {
        Localizer.Instance.LocalizationChanged += OnLocalizationChanged;
    }

    private void OnLocalizationChanged(string lang)
    {
        void Notify()
        {
            Debug.WriteLine($"[LocalizedPage] Invalidating content cache for key={TitleKey} lang={lang}");

            // Drop the cached view — next time Content is accessed (on navigation)
            // a fresh view is created that reads the new language immediately.
            _cachedContent = null;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            // Also notify Content so if the page is currently visible the DataTemplate
            // re-reads Content and gets the fresh view.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
        }

        if (Dispatcher.UIThread.CheckAccess())
            Notify();
        else
            Dispatcher.UIThread.Post(Notify);
    }
}
