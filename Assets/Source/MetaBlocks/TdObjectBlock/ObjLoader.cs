using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Dummiesman;
using Source.Utils;
using UnityEngine;

namespace Source.MetaBlocks.TdObjectBlock
{
    public class ObjLoader : TdObjectLoader<byte[], int>
    {
        private readonly ConcurrentQueue<ObjLoadTask> buildTasks = new();
        private readonly ConcurrentQueue<ObjLoadTask> failedTasks = new();

        protected override IEnumerator GetJob(GameObject refGo, byte[] data, Action<GameObject> onSuccess,
            Action<int> onFailure)
        {
            if (refGo == null)
            {
                Debug.Log("ObjLoader job skipped since the reference game object has been destroyed");
                yield break;
            }

            var done = false;
            InitTask(data, go =>
            {
                done = true;
                if (refGo == null)
                {
                    Debug.Log(
                        "ObjLoader job success handling skipped since the reference game object has been destroyed");
                    MetaBlockObject.DeepDestroy3DObject(go);
                    return;
                }

                onSuccess.Invoke(go);
            }, () =>
            {
                done = true;
                if (refGo == null)
                {
                    Debug.Log(
                        "ObjLoader job failure handling skipped since the reference game object has been destroyed");
                    return;
                }

                onFailure.Invoke(0);
            });
            while (!done)
                yield return null;
        }

        private void InitTask(byte[] data, Action<GameObject> onSuccess, Action onFailure)
        {
            var task = new ObjLoadTask(data, onSuccess, onFailure);

            try
            {
                StartCoroutine(task.zipObjectLoader.Init(task.stream, () =>
                {
                    buildTasks.Enqueue(task);
                    task.stream.Dispose();
                }));
            }
            catch (Exception)
            {
                failedTasks.Enqueue(task);
                task.stream.Dispose();
            }
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