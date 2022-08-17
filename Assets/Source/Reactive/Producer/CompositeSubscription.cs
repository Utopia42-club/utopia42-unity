using System;
using System.Collections.Generic;
using Source.UtopiaException;

namespace Source.Reactive.Producer
{
    public class CompositeSubscription : Subscription
    {
        private readonly List<Subscription> subscriptions = new();

        public CompositeSubscription(params Subscription[] subscriptions)
        {
            foreach (var subscription in subscriptions)
                Add(subscription);
        }
        
        public CompositeSubscription Add(Action subscription)
        {
            return Add(new CustomSubscription(subscription));
        }

        public CompositeSubscription Add(Subscription subscription)
        {
            if (Unsubscribed)
                throw new IllegalStateException("Add can not be called after unsubscribe.");
            subscriptions.Add(subscription);
            return this;
        }

        protected override void DoUnsubscribe()
        {
            foreach (var subscription in subscriptions)
            {
                subscription.Unsubscribe();
            }
        }
    }
}