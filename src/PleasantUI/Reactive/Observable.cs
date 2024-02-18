using PleasantUI.Reactive.Operators;

namespace PleasantUI.Reactive;

public static class Observable
{
    public static IObservable<TSource> Create<TSource>(Func<IObserver<TSource>, IDisposable> subscribe)
    {
        return new CreateWithDisposableObservable<TSource>(subscribe);
    }
    
    public static IObservable<TResult> Select<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TResult> selector)
    {
        return Create<TResult>(obs =>
        {
            return source.Subscribe(new AnonymousObserver<TSource>(
                input =>
                {
                    TResult value;
                    try
                    {
                        value = selector(input);
                    }
                    catch (Exception ex)
                    {
                        obs.OnError(ex);
                        return;
                    }

                    obs.OnNext(value);
                }, obs.OnError, obs.OnCompleted));
        });
    }
    
    public static IObservable<TSource> Where<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
    {
        return Create<TSource>(obs =>
        {
            return source.Subscribe(new AnonymousObserver<TSource>(
                input =>
                {
                    bool shouldRun;
                    try
                    {
                        shouldRun = predicate(input);
                    }
                    catch (Exception ex)
                    {
                        obs.OnError(ex);
                        return;
                    }
                    if (shouldRun)
                    {
                        obs.OnNext(input);
                    }
                }, obs.OnError, obs.OnCompleted));
        });
    }
    
    public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> action)
    {
        return source.Subscribe(new AnonymousObserver<T>(action));
    }
    
    public static IObservable<TResult> CombineLatest<TFirst, TSecond, TResult>(
        this IObservable<TFirst> first, IObservable<TSecond> second,
        Func<TFirst, TSecond, TResult> resultSelector)
    {
        return new CombineLatest<TFirst, TSecond, TResult>(first, second, resultSelector);
    }
    
    public static IObservable<TInput[]> CombineLatest<TInput>(
        this IEnumerable<IObservable<TInput>> inputs)
    {
        return new CombineLatest<TInput, TInput[]>(inputs, items => items);
    }
    
    public static IObservable<T> Skip<T>(this IObservable<T> source, int skipCount)
    {
        if (skipCount <= 0)
        {
            throw new ArgumentException("Skip count must be bigger than zero", nameof(skipCount));
        }

        return Create<T>(obs =>
        {
            int remaining = skipCount;
            return source.Subscribe(new AnonymousObserver<T>(
                input =>
                {
                    if (remaining <= 0)
                    {
                        obs.OnNext(input);
                    }
                    else
                    {
                        remaining--;
                    }
                }, obs.OnError, obs.OnCompleted));
        });
    }
    
    public static IObservable<EventArgs> FromEventPattern(Action<EventHandler> addHandler, Action<EventHandler> removeHandler)
    {
        return Create<EventArgs>(observer =>
        {
            Action<EventArgs>? handler = new(observer.OnNext);
            EventHandler? converted = new((_, args) => handler(args));
            addHandler(converted);

            return Disposable.Create(() => removeHandler(converted));
        });
    }
    
    public static IObservable<T> Take<T>(this IObservable<T> source, int takeCount)
    {
        if (takeCount <= 0)
        {
            return Empty<T>();
        }

        return Create<T>(obs =>
        {
            int remaining = takeCount;
            IDisposable? sub = null;
            sub = source.Subscribe(new AnonymousObserver<T>(
                input =>
                {
                    if (remaining > 0)
                    {
                        --remaining;
                        obs.OnNext(input);

                        if (remaining == 0)
                        {
                            sub?.Dispose();
                            obs.OnCompleted();
                        }
                    }
                }, obs.OnError, obs.OnCompleted));
            return sub;
        });
    }
    
    public static IObservable<T> Empty<T>()
    {
        return EmptyImpl<T>.Instance;
    }
    
    internal sealed class EmptyImpl<TResult> : IObservable<TResult>
    {
        internal static readonly IObservable<TResult> Instance = new EmptyImpl<TResult>();

        private EmptyImpl() { }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
    
    private sealed class CreateWithDisposableObservable<TSource> : IObservable<TSource>
    {
        private readonly Func<IObserver<TSource>, IDisposable> _subscribe;

        public CreateWithDisposableObservable(Func<IObserver<TSource>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            return _subscribe(observer);
        }
    }
}