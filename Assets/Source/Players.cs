using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Source
{
    public class Players : MonoBehaviour
    {
        [SerializeField] private int maxAvatarsAllowed = 20;
        [SerializeField] private int maxInactivityDelay = 2; // in seconds
        [SerializeField] private int maxCreateAvatarDistance = 100;
        [SerializeField] private int minDestroyAvatarDistance = 200;
        [SerializeField] private GameObject avatarPrefab;

        private double lastInActivityCheck = 0;
        private readonly ConcurrentDictionary<string, AvatarController> playersMap = new();

        private void Update()
        {
            if (Time.unscaledTimeAsDouble - lastInActivityCheck < maxInactivityDelay) return; // TODO?
            lastInActivityCheck = Time.unscaledTimeAsDouble;
            var walletIds = playersMap.Keys.ToList();
            foreach (var id in walletIds)
            {
                var controller = playersMap[id];
                if (Time.unscaledTimeAsDouble - controller.UpdatedTime < maxInactivityDelay ||
                    !playersMap.TryRemove(id, out _)) continue;
                Debug.LogWarning($"{id} | Player was inactive for too long. Removing player...");
                DestroyImmediate(controller.gameObject);
            }
        }

        public void ReportOtherPlayersStateFromWeb(string state)
        {
            ReportOtherPlayersState(JsonConvert.DeserializeObject<AvatarController.PlayerState>(state));
        }

        public void ReportOtherPlayersState(AvatarController.PlayerState playerState)
        {
            if (playerState?.walletId == null) return;
            if (AuthService.IsCurrentUser(playerState.walletId))
            {
                Debug.LogWarning(
                    $"Cannot add another player with the same wallet ({playerState.walletId}). Ignoring state...");
            }
            else
            {
                CheckDistanceAndLimit(playerState, out var makeVisible, out var destroy);

                if (playersMap.TryGetValue(playerState.walletId, out var controller))
                {
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
                else if (!destroy)
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
                // else if (playersMap.Count < maxPlayersAllowed)
                //     Debug.Log($"{playerState.walletId} | New player detected but it is too far. Ignoring state...");
                // else
                //     Debug.Log(
                //         $"{playerState.walletId} | New player detected but exceeds the total number of players. Ignoring state...");}
            }
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

        public static Players INSTANCE => GameObject.Find("Players").GetComponent<Players>();
    }
}