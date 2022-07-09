using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace Source.Utils
{
    public class IEnumeratorQueue : MonoBehaviour
    {
        [SerializeField] private float delay;
        private readonly ConcurrentQueue<IEnumerator> jobs = new();
        private readonly ConcurrentQueue<IEnumerator> criticalJobs = new();

        private void Start()
        {
            StartCoroutine(DoJobs());
        }

        private IEnumerator DoJobs()
        {
            while (true)
            {
                yield return new WaitForSeconds(delay);

                if (criticalJobs.Count != 0 && criticalJobs.TryDequeue(out var job))
                {
                    yield return job;
                    continue;
                }

                if (jobs.Count != 0 && jobs.TryDequeue(out job))
                    yield return job; // do we have to wait for the job to be done?
            }
        }

        public void AddJob(IEnumerator job, bool critical = false)
        {
            if (critical)
            {
                criticalJobs.Enqueue(job);
                return;
            }

            jobs.Enqueue(job);
        }
    }
}