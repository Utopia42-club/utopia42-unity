using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace Source.Utils
{
    public class IEnumeratorQueue : MonoBehaviour
    {
        [SerializeField] private float jobDelay;
        [SerializeField] private float criticalDelay;
        private readonly ConcurrentQueue<IEnumerator> jobs = new();
        private readonly ConcurrentQueue<IEnumerator> criticalJobs = new();

        private void Start()
        {
            StartCoroutine(DoJobs());
            StartCoroutine(DoCriticalJobs());
        }

        private IEnumerator DoJobs()
        {
            while (true)
            {
                if (jobs.Count != 0 && jobs.TryDequeue(out var job))
                    yield return job;
                yield return new WaitForSeconds(jobDelay);
            }
        }

        private IEnumerator DoCriticalJobs()
        {
            while (true)
            {
                if (criticalJobs.Count != 0 && criticalJobs.TryDequeue(out var job))
                    yield return job;
                yield return new WaitForSeconds(criticalDelay);
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