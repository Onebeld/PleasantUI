using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PleasantUI.Core.Localization;

/// <summary>
/// A reactive wrapper that re-evaluates a localization key on every language change.
/// Implements <see cref="INotifyPropertyChanged"/> so standard Avalonia bindings
/// pick up updates when <see cref="Localizer.LocalizationChanged"/> fires.
/// </summary>
public sealed class LocalizeKeyObservable : INotifyPropertyChanged
{
    private readonly Func<string> _resolver;
    private string _value;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The currently resolved localized string.</summary>
    public string Value
    {
        get => _value;
        private set
        {
            if (_value == value) return;
            _value = value;
            OnPropertyChanged();
            // Also fire with empty string and indexer names so all binding strategies pick it up
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }

    public LocalizeKeyObservable(Func<string> resolver)
    {
        _resolver = resolver;
        _value = resolver();

        Localizer.Instance.LocalizationChanged += _ =>
        {
            Value = _resolver();
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
