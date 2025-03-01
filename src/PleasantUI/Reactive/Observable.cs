using PleasantUI.Reactive.Operators;

namespace PleasantUI.Reactive;

/// <summary>
/// Provides extension methods for creating and composing observable sequences.
/// </summary>
public static class Observable
{
    /// <summary>
    /// Creates an observable sequence from the specified subscription function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the observable sequence.</typeparam>
    /// <param name="subscribe">The function that is called when an observer subscribes.</param>
    /// <returns>An observable sequence that runs the given subscription function upon subscription.</returns>
    public static IObservable<TSource> Create<TSource>(Func<IObserver<TSource>, IDisposable> subscribe)
    {
        return new CreateWithDisposableObservable<TSource>(subscribe);
    }

    /// <summary>
    /// Projects each element of an observable sequence into a new form.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
    /// <param name="source">The source observable sequence.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <returns>An observable sequence whose elements are the result of invoking the transform function on each element of the source.</returns>
    public static IObservable<TResult> Select<TSource, TResult>(this IObservable<TSource> source,
        Func<TSource, TResult> selector)
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

    /// <summary>
    /// Filters the elements of an observable sequence based on a predicate.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source observable sequence.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>An observable sequence that contains elements from the input sequence that satisfy the condition.</returns>
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

                    if (shouldRun) obs.OnNext(input);
                }, obs.OnError, obs.OnCompleted));
        });
    }

    /// <summary>
    /// Subscribes to an observable sequence using the specified action to handle each element.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the observable sequence.</typeparam>
    /// <param name="source">The source observable sequence.</param>
    /// <param name="action">An action to invoke for each element in the sequence.</param>
    /// <returns>An IDisposable object that can be used to unsubscribe from the observable sequence.</returns>
    public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> action)
    {
        return source.Subscribe(new AnonymousObserver<T>(action));
    }

    /// <summary>
    /// Combines the latest values from two observable sequences using the specified result selector function.
    /// </summary>
    /// <typeparam name="TFirst">The type of the elements in the first observable sequence.</typeparam>
    /// <typeparam name="TSecond">The type of the elements in the second observable sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
    /// <param name="first">The first observable sequence.</param>
    /// <param name="second">The second observable sequence.</param>
    /// <param name="resultSelector">A function that combines the latest values from the two sequences.</param>
    /// <returns>An observable sequence that contains the result of combining the latest values from the two sequences.</returns>
    public static IObservable<TResult> CombineLatest<TFirst, TSecond, TResult>(
        this IObservable<TFirst> first, IObservable<TSecond> second,
        Func<TFirst, TSecond, TResult> resultSelector)
    {
        return new CombineLatest<TFirst, TSecond, TResult>(first, second, resultSelector);
    }

    /// <summary>
    /// Combines the latest values from a sequence of observable sequences into an array.
    /// </summary>
    /// <typeparam name="TInput">The type of the elements in each observable sequence.</typeparam>
    /// <param name="inputs">A sequence of observable sequences.</param>
    /// <returns>An observable sequence whose elements are arrays containing the latest values from the input sequences.</returns>
    public static IObservable<TInput[]> CombineLatest<TInput>(
        this IEnumerable<IObservable<TInput>> inputs)
    {
        return new CombineLatest<TInput, TInput[]>(inputs, items => items);
    }

    /// <summary>
    /// Bypasses a specified number of elements in an observable sequence and then returns the remaining elements.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the observable sequence.</typeparam>
    /// <param name="source">The source observable sequence.</param>
    /// <param name="skipCount">The number of elements to skip.</param>
    /// <returns>An observable sequence that contains the elements that occur after the specified index in the input sequence.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="skipCount"/> is less than or equal to zero.</exception>
    public static IObservable<T> Skip<T>(this IObservable<T> source, int skipCount)
    {
        if (skipCount <= 0) throw new ArgumentException("Skip count must be bigger than zero", nameof(skipCount));

        return Create<T>(obs =>
        {
            int remaining = skipCount;
            return source.Subscribe(new AnonymousObserver<T>(
                input =>
                {
                    if (remaining <= 0)
                        obs.OnNext(input);
                    else
                        remaining--;
                }, obs.OnError, obs.OnCompleted));
        });
    }

    /// <summary>
    /// Creates an observable sequence from events using the specified add and remove handler actions.
    /// </summary>
    /// <param name="addHandler">An action to add the event handler.</param>
    /// <param name="removeHandler">An action to remove the event handler.</param>
    /// <returns>An observable sequence that produces an <see cref="EventArgs"/> value each time the event is raised.</returns>
    public static IObservable<EventArgs> FromEventPattern(Action<EventHandler> addHandler,
        Action<EventHandler> removeHandler)
    {
        return Create<EventArgs>(observer =>
        {
            Action<EventArgs>? handler = observer.OnNext;
            EventHandler? converted = (_, args) => handler(args);
            addHandler(converted);

            return Disposable.Create(() => removeHandler(converted));
        });
    }

    /// <summary>
    /// Returns an observable sequence that produces a specified number of elements from the start of the source sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the observable sequence.</typeparam>
    /// <param name="source">The source observable sequence.</param>
    /// <param name="takeCount">The number of elements to take.</param>
    /// <returns>
    /// An observable sequence that produces the specified number of elements from the start of the source sequence.
    /// If <paramref name="takeCount"/> is less than or equal to zero, the sequence completes immediately.
    /// </returns>
    public static IObservable<T> Take<T>(this IObservable<T> source, int takeCount)
    {
        if (takeCount <= 0) return Empty<T>();

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

    /// <summary>
    /// Returns an empty observable sequence that completes immediately.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the observable sequence.</typeparam>
    /// <returns>An empty observable sequence.</returns>
    public static IObservable<T> Empty<T>()
    {
        return EmptyImpl<T>.Instance;
    }

    internal sealed class EmptyImpl<TResult> : IObservable<TResult>
    {
        internal static readonly IObservable<TResult> Instance = new EmptyImpl<TResult>();

        private EmptyImpl()
        {
        }

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