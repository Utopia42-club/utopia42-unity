using System;
using Source.Reactive.Consumer;

namespace Source.Reactive.Producer
{
    public interface IObservable<out TE>
    {
        Subscription Subscribe(Action<TE> next);
        Subscription Subscribe(Action<TE> next, Action<Exception> error);

        Subscription Subscribe(Action<TE> next, Action<Exception> error,
            Action complete);
    }

    public abstract class Observable<TE> : IObservable<TE>
    {
        public Subscription Subscribe(Action<TE> next)
        {
            return Subscribe(next, e => { });
        }

        public Subscription Subscribe(Action<TE> next, Action<Exception> error)
        {
            return Subscribe(next, error, () => { });
        }

        public abstract Subscription Subscribe(Action<TE> next, Action<Exception> error,
            Action complete);

        public Observable<TO> Pipe<TO>(IRxOperator<TE, TO> op)
        {
            return new PipeObservable<TE, TO>(this, op);
        }
    }
}