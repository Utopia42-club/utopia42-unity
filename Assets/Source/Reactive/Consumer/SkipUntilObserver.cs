using System;

namespace Source.Reactive.Consumer
{
    public class SkipUntilObserver<TE> : Observer<TE, TE>
    {
        private readonly Func<TE, bool> predicate;
        private bool accepted = false;

        public SkipUntilObserver(Func<TE, bool> predicate)
        {
            this.predicate = predicate;
        }

        public override void Observe(TE e)
        {
            if (!accepted)
                accepted = predicate(e);
            if (accepted)
                ExecuteCallbacks(e);
        }
    }
}