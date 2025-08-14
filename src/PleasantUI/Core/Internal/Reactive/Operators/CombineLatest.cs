namespace PleasantUI.Core.Internal.Reactive.Operators;

internal sealed class CombineLatest<TFirst, TSecond, TResult> : IObservable<TResult>
{
    private readonly IObservable<TFirst> _first;
    private readonly Func<TFirst, TSecond, TResult> _resultSelector;
    private readonly IObservable<TSecond> _second;

    public CombineLatest(IObservable<TFirst> first, IObservable<TSecond> second,
        Func<TFirst, TSecond, TResult> resultSelector)
    {
        _first = first;
        _second = second;
        _resultSelector = resultSelector;
    }

    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        _? sink = new(_resultSelector, observer);
        sink.Run(_first, _second);
        return sink;
    }

    internal sealed class _ : IdentitySink<TResult>
    {
        private readonly object _gate = new();
        private readonly Func<TFirst, TSecond, TResult> _resultSelector;

        private IDisposable _firstDisposable;
        private IDisposable _secondDisposable;

        public _(Func<TFirst, TSecond, TResult> resultSelector, IObserver<TResult> observer)
            : base(observer)
        {
            _resultSelector = resultSelector;
            _firstDisposable = null!;
            _secondDisposable = null!;
        }

        public void Run(IObservable<TFirst> first, IObservable<TSecond> second)
        {
            FirstObserver? fstO = new(this);
            SecondObserver? sndO = new(this);

            fstO.SetOther(sndO);
            sndO.SetOther(fstO);

            _firstDisposable = first.Subscribe(fstO);
            _secondDisposable = second.Subscribe(sndO);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _firstDisposable.Dispose();
                _secondDisposable.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed class FirstObserver : IObserver<TFirst>
        {
            private readonly _ _parent;
            private SecondObserver _other;

            public FirstObserver(_ parent)
            {
                _parent = parent;
                _other = default!; // NB: Will be set by SetOther.
            }

            public bool HasValue { get; private set; }
            public TFirst? Value { get; private set; }
            public bool Done { get; private set; }

            public void OnNext(TFirst value)
            {
                lock (_parent._gate)
                {
                    HasValue = true;
                    Value = value;

                    if (_other.HasValue)
                    {
                        TResult res;
                        try
                        {
                            res = _parent._resultSelector(value, _other.Value!);
                        }
                        catch (Exception ex)
                        {
                            _parent.ForwardOnError(ex);
                            return;
                        }

                        _parent.ForwardOnNext(res);
                    }
                    else if (_other.Done)
                    {
                        _parent.ForwardOnCompleted();
                    }
                }
            }

            public void OnError(Exception error)
            {
                lock (_parent._gate)
                {
                    _parent.ForwardOnError(error);
                }
            }

            public void OnCompleted()
            {
                lock (_parent._gate)
                {
                    Done = true;

                    if (_other.Done)
                        _parent.ForwardOnCompleted();
                    else
                        _parent._firstDisposable.Dispose();
                }
            }

            public void SetOther(SecondObserver other)
            {
                _other = other;
            }
        }

        private sealed class SecondObserver : IObserver<TSecond>
        {
            private readonly _ _parent;
            private FirstObserver _other;

            public SecondObserver(_ parent)
            {
                _parent = parent;
                _other = default!; // NB: Will be set by SetOther.
            }

            public bool HasValue { get; private set; }
            public TSecond? Value { get; private set; }
            public bool Done { get; private set; }

            public void OnNext(TSecond value)
            {
                lock (_parent._gate)
                {
                    HasValue = true;
                    Value = value;

                    if (_other.HasValue)
                    {
                        TResult res;
                        try
                        {
                            res = _parent._resultSelector(_other.Value!, value);
                        }
                        catch (Exception ex)
                        {
                            _parent.ForwardOnError(ex);
                            return;
                        }

                        _parent.ForwardOnNext(res);
                    }
                    else if (_other.Done)
                    {
                        _parent.ForwardOnCompleted();
                    }
                }
            }

            public void OnError(Exception error)
            {
                lock (_parent._gate)
                {
                    _parent.ForwardOnError(error);
                }
            }

            public void OnCompleted()
            {
                lock (_parent._gate)
                {
                    Done = true;

                    if (_other.Done)
                        _parent.ForwardOnCompleted();
                    else
                        _parent._secondDisposable.Dispose();
                }
            }

            public void SetOther(FirstObserver other)
            {
                _other = other;
            }
        }
    }
}

internal sealed class CombineLatest<TSource, TResult> : IObservable<TResult>
{
    private readonly Func<TSource[], TResult> _resultSelector;
    private readonly IEnumerable<IObservable<TSource>> _sources;

    public CombineLatest(IEnumerable<IObservable<TSource>> sources, Func<TSource[], TResult> resultSelector)
    {
        _sources = sources;
        _resultSelector = resultSelector;
    }

    public IDisposable Subscribe(IObserver<TResult> observer)
    {
        _? sink = new(_resultSelector, observer);
        sink.Run(_sources);
        return sink;
    }

    internal sealed class _ : IdentitySink<TResult>
    {
        private readonly object _gate = new();
        private readonly Func<TSource[], TResult> _resultSelector;

        private bool[] _hasValue;
        private bool _hasValueAll;
        private bool[] _isDone;
        private IDisposable[] _subscriptions;
        private TSource[] _values;

        public _(Func<TSource[], TResult> resultSelector, IObserver<TResult> observer)
            : base(observer)
        {
            _resultSelector = resultSelector;

            // NB: These will be set in Run before getting used.
            _hasValue = null!;
            _values = null!;
            _isDone = null!;
            _subscriptions = null!;
        }

        public void Run(IEnumerable<IObservable<TSource>> sources)
        {
            IObservable<TSource>[]? srcs = sources.ToArray();

            int n = srcs.Length;

            _hasValue = new bool[n];
            _hasValueAll = false;

            _values = new TSource[n];

            _isDone = new bool[n];

            _subscriptions = new IDisposable[n];

            for (int i = 0; i < n; i++)
            {
                int j = i;

                SourceObserver? o = new(this, j);
                _subscriptions[j] = o;

                o.Disposable = srcs[j].Subscribe(o);
            }

            SetUpstream(new CompositeDisposable(_subscriptions));
        }

        private void OnNext(int index, TSource value)
        {
            lock (_gate)
            {
                _values[index] = value;

                _hasValue[index] = true;

                if (_hasValueAll || (_hasValueAll = _hasValue.All(v => v)))
                {
                    TResult res;
                    try
                    {
                        res = _resultSelector(_values);
                    }
                    catch (Exception ex)
                    {
                        ForwardOnError(ex);
                        return;
                    }

                    ForwardOnNext(res);
                }
                else if (_isDone.Where((_, i) => i != index).All(d => d))
                {
                    ForwardOnCompleted();
                }
            }
        }

        private new void OnError(Exception error)
        {
            lock (_gate)
            {
                ForwardOnError(error);
            }
        }

        private void OnCompleted(int index)
        {
            lock (_gate)
            {
                _isDone[index] = true;

                if (_isDone.All(d => d))
                    ForwardOnCompleted();
                else
                    _subscriptions[index].Dispose();
            }
        }

        private sealed class SourceObserver : IObserver<TSource>, IDisposable
        {
            private readonly int _index;
            private readonly _ _parent;

            public SourceObserver(_ parent, int index)
            {
                _parent = parent;
                _index = index;
            }

            public IDisposable? Disposable { get; set; }

            public void Dispose()
            {
                Disposable?.Dispose();
            }

            public void OnNext(TSource value)
            {
                _parent.OnNext(_index, value);
            }

            public void OnError(Exception error)
            {
                _parent.OnError(error);
            }

            public void OnCompleted()
            {
                _parent.OnCompleted(_index);
            }
        }
    }
}