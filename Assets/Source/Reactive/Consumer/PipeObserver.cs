namespace Source.Reactive.Consumer
{
    public class PipeObserver<TI, TM, TO> : Observer<TI, TO>
    {
        private readonly IObserver<TI, TM> first;

        public PipeObserver(IObserver<TI, TM> first, IObserver<TM, TO> second)
        {
            this.first = first;
            first.Then(second.Observe);
            second.Then(ExecuteCallbacks);
        }

        public override void Observe(TI e)
        {
            first.Observe(e);
        }
    }
}