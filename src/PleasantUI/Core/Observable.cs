using Avalonia.Reactive;
using PleasantUI.Core.Internal.Reactive.Operators;

namespace PleasantUI.Core;

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