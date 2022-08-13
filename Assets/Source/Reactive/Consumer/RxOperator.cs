using System;

namespace Source.Reactive.Consumer
{
    public interface IRxOperator<in TI, out TO>
    {
        IObserver<TI, TO> CreateObserver();
    }

    public class RxOperator<TI, TO> : IRxOperator<TI, TO>
    {
        private readonly Func<Observer<TI, TO>> observerFactory;

        public RxOperator(Func<Observer<TI, TO>> observerFactory)
        {
            this.observerFactory = observerFactory;
        }

        public RxOperator<TI, TO2> Pipe<TO2>(RxOperator<TO, TO2> other)
        {
            return new RxOperator<TI, TO2>(() => new PipeObserver<TI, TO, TO2>(CreateObserver(),
                other.CreateObserver()));
        }

        public IObserver<TI, TO> CreateObserver()
        {
            return observerFactory();
        }

        public static implicit operator RxOperator<TI, TO>(Func<Observer<TI, TO>> observerFactory) =>
            new(observerFactory);
    }
}