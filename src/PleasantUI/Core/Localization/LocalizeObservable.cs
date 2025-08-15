using PleasantUI.Core.Internal.Reactive;

namespace PleasantUI.Core.Localization;

/// <summary>
/// Represents an observable sequence that emits localization changes for a specified key.
/// </summary>
public class LocalizeObservable : IObservable<string>
{
    private List<IObserver<string>>? _observers = new();

    private readonly string _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizeObservable"/> class with the specified localization key.
    /// </summary>
    /// <param name="key">The key for which this observable will track localization changes.</param>
    public LocalizeObservable(string key)
    {
        _key = key;
    }
    
    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<string> observer)
    {
        _ = observer ?? throw new ArgumentNullException(nameof(observer));
        
        bool first;

        for (; ; )
        {
            if (Volatile.Read(ref _observers) == null)
            {
                observer.OnCompleted();
                
                return EmptyDisposable.Instance;
            }

            lock (this)
            {
                if (_observers == null)
                    continue;

                first = _observers.Count == 0;
                _observers.Add(observer);
                break;
            }
        }

        if (first)
        {
            Initialize();
        }

        Subscribed(observer);

        return new RemoveLocalizeObserver(this, observer);
    }

    /// <summary>
    /// Removes the specified observer from the list of observers.
    /// </summary>
    /// <param name="observer">The observer to be removed.</param>
    /// <remarks>
    /// If the observer is successfully removed and the list of observers becomes empty,
    /// it trims the excess capacity of the list and deinitializes localization changes.
    /// </remarks>
    public void Remove(IObserver<string> observer)
    {
        if (Volatile.Read(ref _observers) is null) return;
        
        lock (this)
        {
            List<IObserver<string>>? observers = _observers;

            if (observers is null) return;
            
            observers.Remove(observer);

            if (observers.Count != 0) return;
            
            observers.TrimExcess();
            Deinitialize();
        }
    }
    
    private void Subscribed(IObserver<string> observer)
    {
        observer.OnNext(GetValue());
    }

    private void Initialize()
    {
        Localizer.Instance.LocalizationChanged += InstanceOnLocalizationChanged;
    }

    private void Deinitialize()
    {
        Localizer.Instance.LocalizationChanged -= InstanceOnLocalizationChanged;
    }
    
    private void InstanceOnLocalizationChanged(string obj)
    {
        PublishNext(GetValue());
    }

    private void PublishNext(string value)
    {
        if (Volatile.Read(ref _observers) == null) return;
        
        IObserver<string>[]? observers = null;
        IObserver<string>? singleObserver = null;
            
        lock (this)
        {
            if (_observers == null)
                return;
            if (_observers.Count == 1)
                singleObserver = _observers[0];
            else
                observers = _observers.ToArray();
        }
            
        if (singleObserver != null)
        {
            singleObserver.OnNext(value);
        }
        else
        {
            foreach (IObserver<string> observer in observers!)
                observer.OnNext(value);
        }
    }

    private string GetValue()
    {
        string value = Localizer.Instance[_key];
        
        return value;
    }
}