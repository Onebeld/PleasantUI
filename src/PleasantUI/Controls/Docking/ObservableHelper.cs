using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Reactive;
using PleasantUI.Core.Internal.Reactive;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Provides helper methods for working with observables in the docking system.
/// </summary>
public static class ObservableHelper
{
    /// <summary>
    /// Creates an observable that tracks the visibility of a content presenter's child.
    /// Returns true when the child exists, is visible, and content is not null.
    /// </summary>
    public static IObservable<bool> IsChildVisibleObservable(this ContentPresenter presenter)
    {
        return Core.Observable.Create<bool>(observer =>
        {
            bool lastValue = presenter.IsChildVisible();
            observer.OnNext(lastValue);

            IDisposable? childVisibilityDisposable = null;
            bool hasContent = presenter.Content != null;
            bool childVisible = presenter.Child?.IsVisible ?? false;

            void Emit()
            {
                var newValue = hasContent && childVisible;
                if (newValue != lastValue)
                {
                    lastValue = newValue;
                    observer.OnNext(newValue);
                }
            }

            void SubscribeToChild(Control? child)
            {
                childVisibilityDisposable?.Dispose();
                childVisibilityDisposable = null;

                if (child != null)
                {
                    childVisible = child.IsVisible;
                    childVisibilityDisposable = child.GetObservable(Visual.IsVisibleProperty)
                        .Subscribe(new AnonymousObserver<bool>(v =>
                        {
                            childVisible = v;
                            Emit();
                        }));
                }
                else
                {
                    childVisible = false;
                }

                Emit();
            }

            var contentDisposable = presenter.GetObservable(ContentPresenter.ContentProperty)
                .Subscribe(new AnonymousObserver<object?>(c =>
                {
                    hasContent = c != null;
                    Emit();
                }));

            var childWatchDisposable = presenter.GetObservable(ContentPresenter.ChildProperty)
                .Subscribe(new AnonymousObserver<Control?>(SubscribeToChild));

            return new AnonymousDisposable(() =>
            {
                contentDisposable.Dispose();
                childWatchDisposable.Dispose();
                childVisibilityDisposable?.Dispose();
            });
        });
    }

    /// <summary>
    /// Checks if the content presenter's child is visible.
    /// </summary>
    public static bool IsChildVisible(this ContentPresenter presenter)
    {
        return presenter.Child?.IsVisible == true && presenter.Content != null;
    }
}
