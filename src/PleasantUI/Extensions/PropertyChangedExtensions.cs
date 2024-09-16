using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using PleasantUI.Reactive;

namespace PleasantUI.Extensions;

/// <summary>
/// Extension methods for observing property changes using INotifyPropertyChanged.
/// </summary>
public static class PropertyChangedExtensions
{
    /// <summary>
    /// Creates an observable that produces values whenever the specified property changes.
    /// </summary>
    /// <typeparam name="TModel">The type of the model implementing INotifyPropertyChanged.</typeparam>
    /// <typeparam name="TRes">The type of the property being observed.</typeparam>
    /// <param name="model">The model instance.</param>
    /// <param name="expr">An expression selecting the property to observe.</param>
    /// <returns>An observable that produces the property value whenever it changes.</returns>
    public static IObservable<TRes> WhenAnyValue<TModel, TRes>(this TModel model,
        Expression<Func<TModel, TRes>> expr) where TModel : INotifyPropertyChanged
    {
        LambdaExpression l = expr;
        MemberExpression ma = (MemberExpression)l.Body;
        PropertyInfo prop = (PropertyInfo)ma.Member;
        return new PropertyObservable<TRes>(model, prop);
    }

    /// <summary>
    /// Creates an observable that produces values whenever the specified property changes, applying a transformation function.
    /// </summary>
    /// <typeparam name="TModel">The type of the model implementing INotifyPropertyChanged.</typeparam>
    /// <typeparam name="T1">The type of the property being observed.</typeparam>
    /// <typeparam name="TRes">The type of the result produced by the transformation function.</typeparam>
    /// <param name="model">The model instance.</param>
    /// <param name="v1">An expression selecting the property to observe.</param>
    /// <param name="cb">A function to transform the property value.</param>
    /// <returns>An observable that produces the transformed property value whenever the original property changes.</returns>
    public static IObservable<TRes> WhenAnyValue<TModel, T1, TRes>(this TModel model,
        Expression<Func<TModel, T1>> v1,
        Func<T1, TRes> cb
    ) where TModel : INotifyPropertyChanged
    {
        return model.WhenAnyValue(v1).Select(cb);
    }

    /// <summary>
    /// Creates an observable that produces values whenever either of the specified properties change, applying a transformation function to both values.
    /// </summary>
    /// <typeparam name="TModel">The type of the model implementing INotifyPropertyChanged.</typeparam>
    /// <typeparam name="T1">The type of the first property being observed.</typeparam>
    /// <typeparam name="T2">The type of the second property being observed.</typeparam>
    /// <typeparam name="TRes">The type of the result produced by the transformation function.</typeparam>
    /// <param name="model">The model instance.</param>
    /// <param name="v1">An expression selecting the first property to observe.</param>
    /// <param name="v2">An expression selecting the second property to observe.</param>
    /// <param name="cb">A function to transform the values of both properties.</param>
    /// <returns>An observable that produces the transformed value whenever either of the original properties change.</returns>
    public static IObservable<TRes> WhenAnyValue<TModel, T1, T2, TRes>(this TModel model,
        Expression<Func<TModel, T1>> v1,
        Expression<Func<TModel, T2>> v2,
        Func<T1, T2, TRes> cb
    ) where TModel : INotifyPropertyChanged
    {
        return model.WhenAnyValue(v1).CombineLatest(model.WhenAnyValue(v2),
            cb);
    }

    /// <summary>
    /// Creates an observable that produces a tuple of values whenever either of the specified properties change.
    /// </summary>
    /// <typeparam name="TModel">The type of the model implementing INotifyPropertyChanged.</typeparam>
    /// <typeparam name="T1">The type of the first property being observed.</typeparam>
    /// <typeparam name="T2">The type of the second property being observed.</typeparam>
    /// <param name="model">The model instance.</param>
    /// <param name="v1">An expression selecting the first property to observe.</param>
    /// <param name="v2">An expression selecting the second property to observe.</param>
    /// <returns>An observable that produces a tuple containing the values of both properties whenever either of them change.</returns>
    public static IObservable<ValueTuple<T1, T2>> WhenAnyValue<TModel, T1, T2>(this TModel model,
        Expression<Func<TModel, T1>> v1,
        Expression<Func<TModel, T2>> v2
    ) where TModel : INotifyPropertyChanged
    {
        return model.WhenAnyValue(v1, v2, (a1, a2) => (a1, a2));
    }

    private class PropertyObservable<T> : IObservable<T>
    {
        private readonly PropertyInfo _info;
        private readonly INotifyPropertyChanged _target;

        public PropertyObservable(INotifyPropertyChanged target, PropertyInfo info)
        {
            _target = target;
            _info = info;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return new Subscription(_target, _info, observer);
        }

        private class Subscription : IDisposable
        {
            private readonly PropertyInfo _info;
            private readonly IObserver<T> _observer;
            private readonly INotifyPropertyChanged _target;

            public Subscription(INotifyPropertyChanged target, PropertyInfo info, IObserver<T> observer)
            {
                _target = target;
                _info = info;
                _observer = observer;
                _target.PropertyChanged += OnPropertyChanged;
                _observer.OnNext((T)_info.GetValue(_target)!);
            }

            public void Dispose()
            {
                _target.PropertyChanged -= OnPropertyChanged;
                _observer.OnCompleted();
            }

            private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == _info.Name)
                    _observer.OnNext((T)_info.GetValue(_target)!);
            }
        }
    }
}