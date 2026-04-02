using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
            if (_value == value) return;
            _value = value;
            // Fire all variants so both compiled and reflection bindings pick it up
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }

    /// <summary>
    /// Creates a new observable and registers it with <see cref="Localizer"/> so it
    /// is never garbage-collected while the localizer is alive.
    /// </summary>
    public LocalizeKeyObservable(Func<string> resolver)
    {
        _resolver = resolver;
        _value = resolver();

        // Keep a strong reference so GC cannot collect this instance while the
        // Localizer singleton is alive — otherwise the event subscription below
        // would silently stop firing after the first GC pass.
        Localizer.RegisterObservable(this);

        Localizer.Instance.LocalizationChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(string language)
    {
        string newValue = _resolver();
        Debug.WriteLine($"[LocalizeKeyObservable] lang={language} old=\"{_value}\" new=\"{newValue}\"");
        Value = newValue;
    }
}
