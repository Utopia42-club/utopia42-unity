using System;
using System.Threading.Tasks;

namespace Source.Reactive.Producer
{
    /**
     * Emits value whenever one of inner observables emit.
     * Whenever one of inner observables emits an error, this will emit the error and unsubscribe from all observables.
     * Completes when all the inner observables complete.
     */
    public class MergeObservable<TE> : Observable<TE>
    {
        private readonly IObservable<TE>[] observables;

        public MergeObservable(params IObservable<TE>[] observables)
        {
            this.observables = observables;
        }


        public override Producer.Subscription Subscribe(Action<TE> next, Action<Exception> error, Action complete)
        {
            return new Subscription(observables, next, error, complete);
        }
        
        private class Subscription : CompositeSubscription
        {
            private int toBeCompleted;

            public Subscription(IObservable<TE>[] observables,
                Action<TE> next, Action<Exception> error, Action complete)
            {
                toBeCompleted = observables.Length;
                foreach (var observable in observables)
                {
                    Add(observable.Subscribe(next, e =>
                    {
                        Unsubscribe();
                        error(e);
                    }, () =>
                    {
                        toBeCompleted--;
                        if (toBeCompleted == 0)
                            complete();
                    }));
                }
            }
        }
    }
}