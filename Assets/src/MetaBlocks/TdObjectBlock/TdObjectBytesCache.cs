using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBytesCache : MonoBehaviour
    {
        private const int DownloadLimitMb = 10;
        private const int CacheLimitMb = 100;
        private readonly ConcurrentDictionary<string, byte[]> cache = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<Action<byte[], State?>>> queue = new();

        private int CacheSize => cache.Values.Sum(bytes => bytes.Length);

        public void GetBytes(string url, Action<byte[], State?> action)
        {
            if (TryGetBytes(url, out var bytes))
            {
                action.Invoke(bytes, null);
                return;
            }

            AddInQueue(url, action);
        }

        private bool TryGetBytes(string url, out byte[] bytes)
        {
            return cache.TryGetValue(url, out bytes);
        }

        private void AddInQueue(string url, Action<byte[], State?> action)
        {
            if (!queue.TryGetValue(url, out var waitingActions))
            {
                waitingActions = new ConcurrentQueue<Action<byte[], State?>>();
                queue.TryAdd(url, waitingActions); // TODO ?
                LoadBytes(url);
            }

            waitingActions.Enqueue(action);
        }

        private void AddBytes(string url, byte[] bytes)
        {
            while (bytes.Length + CacheSize > CacheLimitMb * 1000000)
                cache.Remove(cache.TakeLast(1).GetEnumerator().Current.Key, out _);
            cache.TryAdd(url, bytes); // TODO ?
        }

        private void LoadBytes(string url)
        {
            StartCoroutine(LoadBytes(url, (bytes, state) =>
            {
                AddBytes(url, bytes);
                queue.Remove(url, out var actions);
                while (!actions.IsEmpty)
                    if (actions.TryDequeue(out var action))
                        action.Invoke(bytes, state);
                    else
                        Debug.LogWarning("Could not perform action for 3d object block"); // TODO ?
            }));
        }

        private static IEnumerator LoadBytes(string url, Action<byte[], State?> onDone)
        {
            using var webRequest = UnityWebRequest.Get(url);
            var op = webRequest.SendWebRequest();

            while (!op.isDone)
            {
                if (webRequest.downloadedBytes > DownloadLimitMb * 1000000)
                    break;
                yield return null;
            }

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.InProgress:
                    onDone.Invoke(null, State.SizeLimit);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError($"Get for {url} caused Error: {webRequest.error}");
                    onDone.Invoke(null, State.ConnectionError);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError($"Get for {url} caused HTTP Error: {webRequest.error}");
                    onDone.Invoke(null, State.InvalidUrlOrData);
                    break;
                case UnityWebRequest.Result.Success:
                    onDone.Invoke(webRequest.downloadHandler.data, null);
                    break;
            }
        }
    }
}