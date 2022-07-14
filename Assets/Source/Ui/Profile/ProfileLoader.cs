using System;
using System.Collections;
using System.Collections.Generic;
using Source.Service;
using UnityEngine;

namespace Source.Ui.Profile
{
    public class ProfileLoader : MonoBehaviour
    {
        private static ProfileLoader instance;

        private readonly Dictionary<string, Model.Profile> profileCache = new();
        private readonly Dictionary<string, List<LoadData>> loadListeners = new();
        private readonly HashSet<string> loadingWallets = new();

        void Awake()
        {
            instance = this;
        }

        public void load(string walletId, Action<Model.Profile> consumer, Action failed)
        {
            if (profileCache.ContainsKey(walletId))
                consumer.Invoke(profileCache[walletId]);
            else if (!loadingWallets.Contains(walletId))
                StartCoroutine(doLoad(1, walletId, consumer, failed));
            else
            {
                var loadData = new LoadData(consumer, failed);
                if (loadListeners.ContainsKey(walletId))
                    loadListeners[walletId].Add(loadData);
                else
                    loadListeners[walletId] = new List<LoadData> {loadData};
            }
        }

        private IEnumerator doLoad(float timeout, string walletId, Action<Model.Profile> consumer, Action failed)
        {
            loadingWallets.Add(walletId);
            bool success = true;
            yield return WorldRestClient.INSTANCE.GetProfile(walletId, profile =>
            {
                loadingWallets.Remove(walletId);
                profileCache[walletId] = profile;
                consumer.Invoke(profile);
                if (loadListeners.ContainsKey(walletId))
                    loadListeners[walletId].ForEach(data => data.consumer.Invoke(profile));
                loadListeners.Remove(walletId);
            }, () => success = false);
            if (!success)
            {
                if (timeout > 8)
                {
                    failed.Invoke();
                    loadingWallets.Remove(walletId);
                    if (loadListeners.ContainsKey(walletId))
                        loadListeners[walletId].ForEach(data => data.onFailed.Invoke());
                    loadListeners.Remove(walletId);
                    yield break;
                }

                yield return new WaitForSeconds(timeout);
                yield return doLoad(2f * timeout, walletId, consumer, failed);
            }
        }

        public bool IsWalletLoading(String walletId)
        {
            return loadingWallets.Contains(walletId);
        }

        public void InvalidateProfile(string walletId)
        {
            profileCache.Remove(walletId);
        }

        public static ProfileLoader INSTANCE => instance;

        public class LoadData
        {
            public Action<Model.Profile> consumer;
            public Action onFailed;

            public LoadData(Action<Model.Profile> consumer, Action onFailed)
            {
                this.consumer = consumer;
                this.onFailed = onFailed;
            }
        }
    }
}