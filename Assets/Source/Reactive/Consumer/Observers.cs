using System;
using UnityEngine.UIElements;

namespace Source.Ui.Utils.Observer
{
    public static class Observers
    {
        public static DebounceObserver<TE> Debounce<TE>(IVisualElementScheduler scheduler, long debounceMillis)
        {
            return new DebounceObserver<TE>(scheduler, debounceMillis);
        }

        public static DistinctUntilChangedObserver<TE> DistinctUntilChanged<TE>(Func<TE, object> keyExtractor = null)
        {
            return new DistinctUntilChangedObserver<TE>(keyExtractor);
        }

        public static FilterObserver<TE> Filter<TE>(Func<TE, bool> filterPredicate)
        {
            return new FilterObserver<TE>(filterPredicate);
        }

        public static SkipUntilObserver<TE> SkipUntil<TE>(Func<TE, bool> predicate)
        {
            return new SkipUntilObserver<TE>(predicate);
        }
        
        public static MapObserver<TE, TO> Map<TE, TO>(Func<TE, TO> mapper)
        {
            return new MapObserver<TE, TO>(mapper);
        }

        public static PipeObserver<TI, TM, TO> Pipe<TI, TM, TO>(Observer<TI, TM> first, Observer<TM, TO> second)
        {
            return new PipeObserver<TI, TM, TO>(first, second);
        }
    }
}