using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Dummiesman;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectLoader : MonoBehaviour
    {
        private readonly ConcurrentQueue<TdObjectLoadTask> buildTasks = new ConcurrentQueue<TdObjectLoadTask>();
        private readonly ConcurrentQueue<TdObjectLoadTask> failedTasks = new ConcurrentQueue<TdObjectLoadTask>();

        public void InitTask(byte[] data, Action<GameObject> onSuccess, Action onFailure)
        {
            Task.Run(() =>
            {
                var task = new TdObjectLoadTask(data, onSuccess, onFailure);
                try
                {
                    task.zipObjectLoader.Init(task.stream);
                    buildTasks.Enqueue(task);
                }
                catch (Exception)
                {
                    failedTasks.Enqueue(task);
                    task.stream.Close();
                }
            });
        }

        private void Update()
        {
            if (buildTasks.Count > 0 && buildTasks.TryDequeue(out var loadedTask))
            {
                loadedTask.onSuccess.Invoke(loadedTask.zipObjectLoader.BuildObject());
                loadedTask.stream.Close();
            }

            if (failedTasks.Count > 0 && failedTasks.TryDequeue(out var failedTask))
                failedTask.onFailure.Invoke();
        }

        public static TdObjectLoader INSTANCE => GameObject.Find("TdObjectLoader").GetComponent<TdObjectLoader>();
    }

    internal class TdObjectLoadTask
    {
        public readonly Stream stream;
        public readonly Action<GameObject> onSuccess;
        public readonly Action onFailure;
        public readonly ZipObjectLoader zipObjectLoader;

        public TdObjectLoadTask(byte[] data, Action<GameObject> onSuccess, Action onFailure)
        {
            this.stream = new MemoryStream(data);
            this.onSuccess = onSuccess;
            this.onFailure = onFailure;
            zipObjectLoader = new ZipObjectLoader();
        }
    }
}