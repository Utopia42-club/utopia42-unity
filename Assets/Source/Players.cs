using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Source.Service.Auth;
using UnityEngine;

namespace Source
{
    public class Players : MonoBehaviour
    {
        [SerializeField] private int maxAvatarsAllowed;
        [SerializeField] private int maxInactivityDelay; // in seconds
        [SerializeField] private int maxCreateAvatarDistance;
        [SerializeField] private int minDestroyAvatarDistance;
        [SerializeField] private int maxStatesToProcessCount;
        [SerializeField] private GameObject avatarPrefab;

        private double lastInActivityCheck = 0;
        private readonly ConcurrentDictionary<string, AvatarController> playersMap = new();
        private readonly Queue<GameObject> destroyQueue = new();

        private readonly ConcurrentDictionary<string, AvatarController.PlayerState> states = new();
        private readonly ConcurrentQueue<string> walletsUpdateStateQueue = new();

        private void Update()
        {
            while (destroyQueue.Count > 0 && destroyQueue.TryDequeue(out var go))
            {
                if (go != null)
                {
                    DestroyImmediate(go);
                    break;
                }
            }

            if (Time.unscaledTimeAsDouble - lastInActivityCheck > maxInactivityDelay)
            {
                lastInActivityCheck = Time.unscaledTimeAsDouble;
                var walletIds = playersMap.Keys.ToList();
                foreach (var id in walletIds)
                {
                    var controller = playersMap[id];
                    if (Time.unscaledTimeAsDouble - controller.UpdatedTime < maxInactivityDelay ||
                        !playersMap.TryRemove(id, out _)) continue;
                    Debug.LogWarning($"{id} | Player was inactive for too long. Removing player...");
                    destroyQueue.Enqueue(controller.gameObject);
                }
            }

            for (var i = 0; i < maxStatesToProcessCount; i++)
            {
                if (walletsUpdateStateQueue.Count == 0) break;
                if (!walletsUpdateStateQueue.TryDequeue(out var walletId) ||
                    !playersMap.TryGetValue(walletId, out var controller) || controller == null ||
                    !states.TryGetValue(walletId, out var playerState) || !states.TryRemove(walletId, out _)) continue;

                CheckDistanceAndLimit(playerState, out var makeVisible, out var destroy);
                if (destroy)
                {
                    Debug.Log($"{playerState.walletId} | Player distance is too far. Removing player...");
                    if (playersMap.TryRemove(playerState.walletId, out _))
                        DestroyImmediate(controller.gameObject);
                }
                else
                {
                    if (makeVisible && !controller.AvatarAllowed)
                    {
                        controller.LoadAnotherPlayerAvatar(playerState.walletId);
                        Debug.Log($"{playerState.walletId} | Loading the avatar...");
                    }

                    controller.UpdatePlayerState(playerState);
                }
            }
        }

        public void ReportOtherPlayersStateFromWeb(string state)
        {
            var s = JsonConvert.DeserializeObject<AvatarController.PlayerState>(state);
            if (s?.walletId == null) return;
            s.walletId = s.walletId?.ToLower();
            ReportOtherPlayersState(s);
        }

        public void ReportOtherPlayersState(AvatarController.PlayerState playerState)
        {
            if (AuthService.Instance.IsCurrentUser(playerState.walletId))
            {
                Debug.LogWarning(
                    $"Cannot add another player with the same wallet ({playerState.walletId}). Ignoring state...");
            }
            else if (playersMap.TryGetValue(playerState.walletId, out var controller))
            {
                controller.UpdatedTime = Time.unscaledTimeAsDouble;

                if (states.ContainsKey(playerState.walletId))
                    states[playerState.walletId] = playerState;
                else if (states.TryAdd(playerState.walletId, playerState))
                    walletsUpdateStateQueue.Enqueue(playerState.walletId);
            }
            else
            {
                CheckDistanceAndLimit(playerState, out var makeVisible, out var destroy);
                if (!destroy)
                {
                    var avatar = Instantiate(avatarPrefab, transform);
                    avatar.name = "AnotherPlayer";
                    var c = avatar.GetComponent<AvatarController>();
                    c.SetAnotherPlayer(playerState.walletId, playerState.GetPosition(), makeVisible);
                    if (playersMap.TryAdd(playerState.walletId, c))
                    {
                        playerState.teleport = true;
                        StartCoroutine(UpdatePlayerStateInNextFrame(c, playerState));
                        Debug.Log($"{playerState.walletId} | New player detected " +
                                  (makeVisible ? "(Loading the avatar...)" : "(Label only)"));
                    }
                    else
                        DestroyImmediate(c.gameObject);
                }
            }
        }

        public void GetStatistics(out int totalTrackedPlayersCount, out int totalAvatarsCount)
        {
            totalTrackedPlayersCount = playersMap.Count;
            totalAvatarsCount = playersMap.Values.Count(controller => controller != null && controller.Avatar != null);
        }

        private void CheckDistanceAndLimit(AvatarController.PlayerState state, out bool makeVisible,
            out bool destroy)
        {
            makeVisible = false;
            destroy = true;
            if (state?.position == null) return;
            var distance = (state.GetPosition() - Player.INSTANCE.GetPosition()).magnitude;
            if (distance > minDestroyAvatarDistance) return;
            destroy = false;
            if (distance <= maxCreateAvatarDistance &&
                playersMap.Values.Count(controller => controller.AvatarAllowed) < maxAvatarsAllowed)
                makeVisible = true;
        }

        private static IEnumerator UpdatePlayerStateInNextFrame(AvatarController controller,
            AvatarController.PlayerState playerState)
        {
            yield return null;
            controller.UpdatePlayerState(playerState);
        }

        public void Clear()
        {
            var controllers = playersMap.Values.ToList();
            playersMap.Clear();
            foreach (var controller in controllers)
                destroyQueue.Enqueue(controller.gameObject);
            states.Clear();
            walletsUpdateStateQueue.Clear();
        }

        public static Players INSTANCE => GameObject.Find("Players").GetComponent<Players>();
    }
}