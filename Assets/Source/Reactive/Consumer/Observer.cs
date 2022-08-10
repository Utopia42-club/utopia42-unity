using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Source.Ui.Utils.Observer
{
    public abstract class Observer<TI, TO>
    {
        private readonly List<EventCallback<TO>> innerCallbacks = new();

        public Observer<TI, TO> Then(EventCallback<TO> callback)
        {
            innerCallbacks.Add(callback);
            return this;
        }
        protected void ExecuteCallbacks(TO e)
        {
            foreach (var innerCallback in innerCallbacks)
                innerCallback(e);
        }

        public Observer<TI, TO2> Pipe<TO2>(Observer<TO, TO2> other)
        {
            return new PipeObserver<TI, TO, TO2>(this, other);
        }

        public abstract void Observe(TI e);

        public static implicit operator EventCallback<TI>(Observer<TI, TO> d) => d.Observe;
        public static implicit operator Action<TI>(Observer<TI, TO> d) => d.Observe;
    }
}