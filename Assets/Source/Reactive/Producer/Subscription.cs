namespace Source.Reactive.Producer
{
    public abstract class Subscription
    {
        protected bool Unsubscribed { get; private set; } = false;
        
        public void Unsubscribe()
        {
            if (Unsubscribed) return;
            DoUnsubscribe();
        }

        protected abstract void DoUnsubscribe();
    }
}