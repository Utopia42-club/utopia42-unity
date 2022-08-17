using System;

namespace Source.Reactive.Producer
{
    public class Subject<TE> : Observable<TE>
    {
        private event Action<TE> next = (e) => { };
        private event Action<Exception> error = (e) => { };
        private event Action complete = () => { };
        private ObservableView observableViewView;

        public Subject()
        {
            observableViewView = new ObservableView(this);
        }

        public override Producer.Subscription Subscribe(Action<TE> next, Action<Exception> error, Action complete)
        {
            return new Subscription(this, next, error, complete);
        }

        public void Next(TE o)
        {
            next(o);
        }

        public void Error(Exception e)
        {
            error(e);
        }

        public void Complete()
        {
            complete();
        }

        public Observable<TE> AsObservable()
        {
            return observableViewView;
        }

        private class ObservableView : Observable<TE>
        {
            private readonly Subject<TE> subject;

            public ObservableView(Subject<TE> subject)
            {
                this.subject = subject;
            }

            public override Producer.Subscription Subscribe(Action<TE> next, Action<Exception> error, Action complete)
            {
                return subject.Subscribe(next, error, complete);
            }
        }

        private class Subscription : Producer.Subscription
        {
            private readonly Subject<TE> subject;
            private readonly Action<TE> next;
            private readonly Action<Exception> error;
            private readonly Action complete;

            public Subscription(Subject<TE> subject, Action<TE> next, Action<Exception> error, Action complete)
            {
                this.subject = subject;
                this.next = next;
                this.error = error;
                this.complete = complete;

                subject.next += OnNext;
                subject.error += OnError;
                subject.complete += OnComplete;
            }

            private void OnComplete()
            {
                if (Unsubscribed) return;
                complete.Invoke();
                DoUnsubscribe();
            }

            private void OnError(Exception e)
            {
                if (Unsubscribed) return;
                error(e);
            }

            private void OnNext(TE obj)
            {
                if (Unsubscribed) return;
                next(obj);
            }

            protected override void DoUnsubscribe()
            {
                subject.next -= OnNext;
                subject.error -= OnError;
                subject.complete -= OnComplete;
            }
        }
    }
}