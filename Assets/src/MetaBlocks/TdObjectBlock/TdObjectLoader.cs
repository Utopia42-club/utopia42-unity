using System;
using System.Collections.Concurrent;
using System.IO;
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
            var task = new TdObjectLoadTask(data, onSuccess, onFailure);
            Action next = () =>
            {
                buildTasks.Enqueue(task);
                task.stream.Dispose();
            };
            Action failed = () =>
            {
                failedTasks.Enqueue(task);
                task.stream.Dispose();
            };


            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                try
                {
                    StartCoroutine(task.zipObjectLoader.Init(task.stream, next));
                }
                catch (Exception)
                {
                    failed.Invoke();
                }
            }
            else
                Task.Run(() =>
                {
                    try
                    {
                        task.zipObjectLoader.Init(task.stream);
                        next.Invoke();
                    }
                    catch (Exception)
                    {
                        failed.Invoke();
                    }
                });
        }

        private void Update()
        {
            if (buildTasks.Count > 0 && buildTasks.TryDequeue(out var loadedTask))
                StartCoroutine(loadedTask.zipObjectLoader.Build3DObject(loadedTask.onSuccess, 10)); // non-blocking

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
            stream = new MemoryStream(data);
            this.onSuccess = onSuccess;
            this.onFailure = onFailure;
            zipObjectLoader = new ZipObjectLoader();
        }
    }
}