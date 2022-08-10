namespace Source.Ui.Utils.Observer
{
    public class PipeObserver<TI, TM, TO> : Observer<TI, TO>
    {
        private readonly Observer<TI, TM> first;

        public PipeObserver(Observer<TI, TM> first, Observer<TM, TO> second)
        {
            this.first = first;
            first.Then(second);
            second.Then(ExecuteCallbacks);
        }

        public override void Observe(TI e)
        {
            first.Observe(e);
        }
    }
}