using System;
using UnityEngine.UIElements;

namespace Source.Reactive.Producer
{
    public class EventObservable<TE> : Observable<TE>
        where TE : EventBase<TE>, new()
    {
        private readonly VisualElement element;

        public EventObservable(VisualElement element)
        {
            this.element = element;
        }

        public override Subscription Subscribe(Action<TE> next, Action<Exception> error, Action complete)
        {
            return new Subscription<TE>(element, next);
        }

        private class Subscription<TEvent> : Subscription
            where TEvent : EventBase<TEvent>, new()
        {
            private readonly VisualElement element;
            private readonly EventCallback<TEvent> listener;

            public Subscription(VisualElement element, Action<TEvent> callback)
            {
                this.element = element;
                listener = e => callback(e);
                element.RegisterCallback(listener);
            }

            protected override void DoUnsubscribe()
            {
                element.UnregisterCallback(listener);
            }
        }
    }
}