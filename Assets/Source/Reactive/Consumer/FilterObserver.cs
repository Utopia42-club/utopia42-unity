using System;

namespace Source.Reactive.Consumer
{
    public class FilterObserver<TE> : Observer<TE, TE>
    {
        private readonly Func<TE, bool> filterPredicate;

        public FilterObserver(Func<TE, bool> filterPredicate)
        {
            this.filterPredicate = filterPredicate;
        }

        public override void Observe(TE e)
        {
            if (filterPredicate(e))
                ExecuteCallbacks(e);
        }
    }
}