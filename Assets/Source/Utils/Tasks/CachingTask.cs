using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Source.Utils.Tasks
{
    public class CachingTask<T>
    {
        private readonly Func<TaskListener<T>, IEnumerator> loader;
        private bool loading = false;
        private bool cached = false;
        private T result;

        public CachingTask(Func<TaskListener<T>, IEnumerator> loader)
        {
            this.loader = loader;
        }

        public IEnumerator Get(Action<T> onSuccess, Action onFailure)
        {
            if (cached) onSuccess(result);
            else if (loading)
            {
                yield return new WaitUntil(() => !loading);
                if (cached)
                    onSuccess(result);
                else onFailure();
            }
            else
            {
                yield return Load(onSuccess, onFailure);
            }
        }

        private IEnumerator Load(Action<T> onSuccess, Action onFailure)
        {
            loading = true;
            yield return loader(new TaskListener<T>(() =>
            {
                loading = false;
                cached = false;
                onFailure();
            }, r =>
            {
                result = r;
                cached = true;
                loading = false;
                onSuccess(result);
            }));
        }
    }
}