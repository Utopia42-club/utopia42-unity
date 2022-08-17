using System;
using UnityEngine.UIElements;

namespace Source.Reactive.Consumer
{
    public static class Operators
    {
        public static RxOperator<TE, TE> Debounce<TE>(IVisualElementScheduler scheduler, long debounceMillis)
        {
            return new RxOperator<TE, TE>(() => new DebounceObserver<TE>(scheduler, debounceMillis));
        }

        public static RxOperator<TE, TE> DistinctUntilChanged<TE>(Func<TE, object> keyExtractor = null)
        {
            return new RxOperator<TE, TE>(() => new DistinctUntilChangedObserver<TE>(keyExtractor));
        }

        public static RxOperator<TE, TE> Filter<TE>(Func<TE, bool> filterPredicate)
        {
            return new RxOperator<TE, TE>(() => new FilterObserver<TE>(filterPredicate));
        }

        public static RxOperator<TE, TE> SkipUntil<TE>(Func<TE, bool> predicate)
        {
            return new RxOperator<TE, TE>(() => new SkipUntilObserver<TE>(predicate));
        }

        public static RxOperator<TI, TO> Map<TI, TO>(Func<TI, TO> mapper)
        {
            return new RxOperator<TI, TO>(() => new MapObserver<TI, TO>(mapper));
        }
    }
}