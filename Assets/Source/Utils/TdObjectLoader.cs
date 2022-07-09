using System;
using System.Collections;
using System.Collections.Concurrent;
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

        public void AddJob(T data, Action<GameObject> onSuccess, Action<K> onFailure)
        {
            var job = GetJob(data, onSuccess, onFailure);
            queue.AddJob(job, IsCritical());
        }

        protected virtual bool IsCritical()
        {
            return false;
        }

        protected abstract IEnumerator GetJob(T data, Action<GameObject> onSuccess, Action<K> onFailure);
    }
}