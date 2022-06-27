using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Source.Ui.Utils
{
    public class DebounceEventListener<TE>
    {
        private readonly MonoBehaviour monoBehaviour;
        private readonly EventCallback<TE> innerCallback;
        private readonly float debounceSeconds;
        private Coroutine coRoutine;
        private float lastEventTime;
        private TE lastEvent;
        
        public readonly EventCallback<TE> Deligate;

        public DebounceEventListener(MonoBehaviour monoBehaviour, float debounceSeconds, EventCallback<TE> innerCallback)
        {
            this.monoBehaviour = monoBehaviour;
            this.innerCallback = innerCallback;
            this.debounceSeconds = debounceSeconds;
            Deligate = HandleEvent;
        }

        private void HandleEvent(TE e)
        {
            lastEventTime = Time.realtimeSinceStartup;
            lastEvent = e;
            if (coRoutine != null)
                coRoutine = monoBehaviour.StartCoroutine(StartTimer());
        }

        private IEnumerator StartTimer()
        {
            float elapsed;
            while ((elapsed = Time.realtimeSinceStartup - lastEventTime) >= debounceSeconds)
                yield return new WaitForSecondsRealtime(debounceSeconds - elapsed);
            coRoutine = null;
            innerCallback(lastEvent);
        }
    }
}