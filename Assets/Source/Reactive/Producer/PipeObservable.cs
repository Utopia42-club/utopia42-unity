using System;
using Source.Reactive.Consumer;

namespace Source.Reactive.Producer
{
    public class PipeObservable<TI, TO> : Observable<TO>
    {
        private readonly Observable<TI> observable;
        private readonly IRxOperator<TI, TO> rxOperator;

        public PipeObservable(Observable<TI> observable, IRxOperator<TI, TO> rxOperator)
        {
            this.observable = observable;
            this.rxOperator = rxOperator;
        }


        public override Producer.Subscription Subscribe(Action<TO> next, Action<Exception> error, Action complete)
        {
            return new Subscription(this, next, error, complete);
        }

        private class Subscription : CompositeSubscription
        {
            public Subscription(PipeObservable<TI, TO> obs,
                Action<TO> next, Action<Exception> error, Action complete)
            {
                Add(obs.observable.Subscribe(obs.rxOperator.CreateObserver()
                    .Then(e => next(e)).Observe, error, complete));
            }
        }
    }
}