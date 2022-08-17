using System;

namespace Source.Reactive.Producer
{
    public class StaticObservable<TE> : Observable<TE>
    {
        private readonly TE[] values;

        public StaticObservable(params TE[] values)
        {
            this.values = values;
        }

        public override Producer.Subscription Subscribe(Action<TE> next, Action<Exception> error, Action complete)
        {
            return new Subscription(values, next, complete);
        }

        private class Subscription : Producer.Subscription
        {
            public Subscription(TE[] values, Action<TE> next, Action complete)
            {
                foreach (var value in values)
                {
                    if (Unsubscribed) return;
                    next(value);
                }

                complete();
            }

            protected override void DoUnsubscribe()
            {
            }
        }
    }
}