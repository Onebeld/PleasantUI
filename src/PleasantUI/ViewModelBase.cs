using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PleasantUI;

public class ViewModelBase : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Sets the value of a field and raises the property changed event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the field and value.</typeparam>
    /// <param name="field">The reference to the field.</param>
    /// <param name="value">The new value to be set.</param>
    /// <param name="propertyName">The name of the property that is being set. Optional if called inside the property's setter.</param>
    /// <returns>True if the value has changed and the property changed event was raised; otherwise, false.</returns>
    protected bool RaiseAndSet<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        RaisePropertyChanged(propertyName);

        return true;
    }

    /// <summary>
    /// Raises the property changed event.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed. If not provided, the name of the calling member will be used.</param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}