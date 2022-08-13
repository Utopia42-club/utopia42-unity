using System;

namespace Source.Reactive.Consumer
{
    public class DistinctUntilChangedObserver<TE> : FilterObserver<TE>
    {
        public DistinctUntilChangedObserver(Func<TE, object> keyExtractor = null) :
            base(new Filter<TE>(keyExtractor).Apply)
        {
        }

        private class Filter<TE>
        {
            private readonly Func<TE, object> keyExtractor;
            private bool first = true;
            private object last;

            public Filter(Func<TE, object> keyExtractor)
            {
                this.keyExtractor = keyExtractor ?? (e => e);
            }

            public bool Apply(TE e)
            {
                var pre = last;
                if (first)
                {
                    first = false;
                    return true;
                }

                last = keyExtractor(e);
                return !Equals(pre, last);
            }
        }
    }
}