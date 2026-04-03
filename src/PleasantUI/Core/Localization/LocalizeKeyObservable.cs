using System.ComponentModel;
using System.Diagnostics;

namespace PleasantUI.Core.Localization;

/// <summary>
/// A reactive wrapper that re-evaluates a localization resolver on every language change.
/// Implements <see cref="INotifyPropertyChanged"/> so Avalonia bindings pick up updates.
/// Instances are kept alive via a strong reference in <see cref="Localizer"/> to prevent GC
/// from silently killing the language-change subscription.
/// </summary>
public sealed class LocalizeKeyObservable : INotifyPropertyChanged
{
    private readonly Func<string> _resolver;
    private string _value;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The currently resolved localized string.</summary>
    public string Value
    {
        get => _value;
        private set
        {
            _value = value;
            // Fire ALL variants unconditionally — compiled bindings, reflection bindings,
            // indexer bindings all need to see the change regardless of value equality.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }

    /// <summary>
    /// Creates a new observable, evaluates the resolver immediately, and registers
    /// a strong reference in <see cref="Localizer"/> to prevent GC collection.
    /// </summary>
    public LocalizeKeyObservable(Func<string> resolver)
    {
        _resolver = resolver;
        _value = resolver();

        Localizer.RegisterObservable(this);
        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string language)
    {
        string newValue = _resolver();
        Debug.WriteLine($"[LocalizeKeyObservable] lang={language} old=\"{_value}\" new=\"{newValue}\"");
        // Always assign — setter fires PropertyChanged unconditionally
        Value = newValue;
    }
}
