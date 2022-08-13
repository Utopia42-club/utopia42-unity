using System;
using System.Collections;
using System.Collections.Generic;
using Source.Configuration;
using Source.Model;
using Source.Reactive.Producer;
using UnityEngine.Networking;
using static Source.Reactive.Consumer.Operators;
using static Source.Service.RestClient;

namespace Source.Service
{
    public class ProfileRestClient
    {
        private readonly Queue<string> avatarImageUrlCacheVictims = new();
        private readonly Dictionary<string, string> avatarImageUrlCache = new();
        public static readonly ProfileRestClient INSTANCE = new();
        private string baseUrl => $"{Configurations.Instance.apiURL}/profile";

        public IEnumerator GetProfile(string walletId, Action<Profile> consumer, Action failed)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Post(baseUrl, walletId))
            {
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Accept", "*/*");
                yield return ExecuteRequest(webRequest, consumer, failed);
            }
        }

        public Observable<string> GetProfileImageUrl(string avatarUrl)
        {
            if (avatarImageUrlCache.TryGetValue(avatarUrl, out var cached))
            {
                return Observables.Of(cached);
            }

            return Observables.FromCoroutine<AvatarSnapshotResponse>((n, e) =>
                Post(Configurations.Instance.avatarRenderApi,
                    new AvatarSnapshotRequest() {model = avatarUrl}, n,
                    () => e(new Exception()))
            ).Pipe(Map<AvatarSnapshotResponse, string>(a =>
            {
                var result = a?.renders != null && a.renders.Length > 0 ? a.renders[0] : null;
                if (!avatarImageUrlCache.ContainsKey(avatarUrl))
                {
                    if (avatarImageUrlCache.Count > 100)
                    {
                        var key = avatarImageUrlCacheVictims.Dequeue();
                        avatarImageUrlCache.Remove(key);
                    }

                    avatarImageUrlCacheVictims.Enqueue(result);
                }

                avatarImageUrlCache[avatarUrl] = result;
                return result;
            }));
        }

        [Serializable]
        private class AvatarSnapshotRequest
        {
            public string model;
            public string scene = "fullbody-portrait-v1";
            public string armature = "ArmatureTargetMale";
        }

        [Serializable]
        private class AvatarSnapshotResponse
        {
            public string[] renders;
        }
    }
}