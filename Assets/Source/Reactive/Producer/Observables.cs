using System;
using System.Collections;
using UnityEngine.UIElements;

namespace Source.Reactive.Producer
{
    public static class Observables
    {
        public static CoroutineObservable<TE> FromCoroutine<TE>(Func<Action<TE>, IEnumerator> provider)
        {
            return new CoroutineObservable<TE>(provider);
        }

        public static CoroutineObservable<TE> FromCoroutine<TE>(
            Func<Action<TE>, Action<Exception>, IEnumerator> provider)
        {
            return new CoroutineObservable<TE>(provider);
        }

        public static EventObservable<TE> FromEvent<TE>(VisualElement element)
            where TE : EventBase<TE>, new()
        {
            return new EventObservable<TE>(element);
        }
    }
}