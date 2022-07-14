using System;
using System.Collections;
using UnityEngine;

namespace Source.Utils
{
    public abstract class TdObjectLoader<T, K> : MonoBehaviour // T -> Data; K -> Error Type
    {
        private IEnumeratorQueue queue;

        private void Start()
        {
            queue = gameObject.GetComponent<IEnumeratorQueue>();
        }

        public void AddJob(GameObject refGo, T data, Action<GameObject> onSuccess, Action<K> onFailure)
        {
            var job = GetJob(refGo, data, onSuccess, onFailure);
            queue.AddJob(job, IsCritical());
        }

        protected virtual bool IsCritical()
        {
            return false;
        }

        protected abstract IEnumerator GetJob(GameObject refGo, T data, Action<GameObject> onSuccess, Action<K> onFailure);
    }
}