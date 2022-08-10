using System;

namespace Source.Reactive.Producer
{
    public class CustomSubscription : Subscription
    {
        private readonly Action innerUnsubscribe;

        public CustomSubscription(Action innerUnsubscribe)
        {
            this.innerUnsubscribe = innerUnsubscribe;
        }

        protected override void DoUnsubscribe()
        {
            innerUnsubscribe();
        }
    }
}