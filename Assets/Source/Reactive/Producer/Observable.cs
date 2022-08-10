using System;

namespace Source.Reactive.Producer
{
    public abstract class Observable<TE>
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
    }
}