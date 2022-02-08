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
        private readonly ConcurrentQueue<TdObjectLoadTask> initMaterialTasks = new ConcurrentQueue<TdObjectLoadTask>();
        private readonly ConcurrentQueue<TdObjectLoadTask> createBuilderTasks = new ConcurrentQueue<TdObjectLoadTask>();
        private readonly ConcurrentQueue<TdObjectLoadTask> buildTasks = new ConcurrentQueue<TdObjectLoadTask>();
        private readonly ConcurrentQueue<TdObjectLoadTask> failedTasks = new ConcurrentQueue<TdObjectLoadTask>();

        public void InitTask(byte[] data, Action<GameObject> onSuccess, Action onFailure)
        {
            TryBackgroundAction((t =>
            {
                t.objLoader.InitZipMap(new ZipArchive(t.stream));
                initMaterialTasks.Enqueue(t);
            }), new TdObjectLoadTask(data, onSuccess, onFailure));
        }

        private void Update()
        {
            if (initMaterialTasks.Count > 0 && initMaterialTasks.TryDequeue(out var initMaterialTask))
            {
                TryAction(t =>
                {
                    t.objLoader.InitMaterialsForZip();
                    createBuilderTasks.Enqueue(t);
                }, initMaterialTask);
            }

            if (createBuilderTasks.Count > 0 && createBuilderTasks.TryDequeue(out var createBuilderTask))
            {
                TryBackgroundAction(t =>
                {
                    t.objLoader.CreateBuilderDictionary();
                    buildTasks.Enqueue(t);
                }, createBuilderTask);
            }

            if (buildTasks.Count > 0 && buildTasks.TryDequeue(out var loadedTask))
            {
                loadedTask.onSuccess.Invoke(loadedTask.objLoader.BuildBuilderDictionary());
                loadedTask.stream.Close();
            }

            if (failedTasks.Count > 0 && failedTasks.TryDequeue(out var failedTask))
                failedTask.onFailure.Invoke();
        }

        private void TryBackgroundAction(Action<TdObjectLoadTask> action, TdObjectLoadTask task)
        {
            Task.Run(() => { TryAction(action, task); });
        }

        private void TryAction(Action<TdObjectLoadTask> action, TdObjectLoadTask task)
        {
            try
            {
                action.Invoke(task);
            }
            catch (Exception)
            {
                failedTasks.Enqueue(task);
                task.stream.Close();
            }
        }

        public static TdObjectLoader INSTANCE => GameObject.Find("TdObjectLoader").GetComponent<TdObjectLoader>();
    }

    internal class TdObjectLoadTask
    {
        public readonly Stream stream;
        public readonly Action<GameObject> onSuccess;
        public readonly Action onFailure;
        public readonly OBJLoader objLoader;

        public TdObjectLoadTask(byte[] data, Action<GameObject> onSuccess, Action onFailure)
        {
            this.stream = new MemoryStream(data);
            this.onSuccess = onSuccess;
            this.onFailure = onFailure;
            objLoader = new OBJLoader();
        }
    }
}