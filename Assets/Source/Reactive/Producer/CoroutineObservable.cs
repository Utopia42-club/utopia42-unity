using System;
using System.Collections;
using UnityEngine;

namespace Source.Reactive.Producer
{
    public class CoroutineObservable<TE> : Observable<TE>
    {
        private readonly Func<Action<TE>, Action<Exception>, IEnumerator> provider;

        public CoroutineObservable(Func<Action<TE>, IEnumerator> provider)
            : this((n, e) => provider(n))
        {
        }

        public CoroutineObservable(Func<Action<TE>, Action<Exception>, IEnumerator> provider)
        {
            this.provider = provider;
        }

        public override Producer.Subscription Subscribe(Action<TE> next, Action<Exception> error, Action complete)
        {
            return new Subscription(provider, next, error, complete);
        }

        private class Subscription : Producer.Subscription
        {
            private readonly Coroutine coroutine;

            public Subscription(Func<Action<TE>, Action<Exception>, IEnumerator> provider, Action<TE> callback,
                Action<Exception> error, Action complete)
            {
                coroutine = CoroutineManager.StartCoroutine(provider(e =>
                {
                    if (!Unsubscribed)
                        callback(e);
                }, error), complete);
            }

            protected override void DoUnsubscribe()
            {
                CoroutineManager.StopCoroutine(coroutine);
            }
        }
    }
}