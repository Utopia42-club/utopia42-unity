using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Dummiesman;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class ObjLoader : MonoBehaviour
    {
        private readonly ConcurrentQueue<ObjLoadTask> buildTasks = new ConcurrentQueue<ObjLoadTask>();
        private readonly ConcurrentQueue<ObjLoadTask> failedTasks = new ConcurrentQueue<ObjLoadTask>();

        public void InitTask(byte[] data, Action<GameObject> onSuccess, Action onFailure)
        {
            var task = new ObjLoadTask(data, onSuccess, onFailure);
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
                System.Threading.Tasks.Task.Run(() =>
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

        public static ObjLoader INSTANCE => GameObject.Find("TdObjectLoader").GetComponent<ObjLoader>();
    }

    internal class ObjLoadTask
    {
        public readonly Stream stream;
        public readonly Action<GameObject> onSuccess;
        public readonly Action onFailure;
        public readonly ZipObjectLoader zipObjectLoader;

        public ObjLoadTask(byte[] data, Action<GameObject> onSuccess, Action onFailure)
        {
            stream = new MemoryStream(data);
            this.onSuccess = onSuccess;
            this.onFailure = onFailure;
            zipObjectLoader = new ZipObjectLoader();
        }
    }
}