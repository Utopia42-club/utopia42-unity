using System;

namespace Source.Ui.Utils.Observer
{
    public class MapObserver<TE, TO> : Observer<TE, TO>
    {
        private readonly Func<TE, TO> mapper;

        public MapObserver(Func<TE, TO> mapper)
        {
            this.mapper = mapper;
        }

        public override void Observe(TE e)
        {
            ExecuteCallbacks(mapper(e));
        }
    }
}