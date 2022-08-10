using UnityEngine.UIElements;

namespace Source.Ui.Utils.Observer
{
    public class DebounceObserver<TE> : Observer<TE, TE>
    {
        private readonly IVisualElementScheduler scheduler;
        private readonly long debounceMillis;
        private IVisualElementScheduledItem task;
        private TE lastEvent;

        public DebounceObserver(IVisualElementScheduler scheduler, long debounceMillis)
        {
            this.scheduler = scheduler;
            this.debounceMillis = debounceMillis;
        }

        public override void Observe(TE e)
        {
            lastEvent = e;
            if (task == null)
                task = scheduler.Execute(() => ExecuteCallbacks(lastEvent));
            task.Pause();
            task.ExecuteLater(debounceMillis);
        }
    }
}