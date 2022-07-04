using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Source
{
    public class Players : MonoBehaviour
    {
        [SerializeField] private int maxPlayersAllowed = 10;
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
            if (playerState.walletId.Equals(AuthService.WalletId()))
            {
                Debug.LogWarning(
                    $"Cannot add another player with the same wallet ({playerState.walletId}). Ignoring state...");
            }
            else if (playersMap.TryGetValue(playerState.walletId, out var controller))
            {
                if (ShouldDestroyAvatar(playerState))
                {
                    Debug.Log($"{playerState.walletId} | Player distance is too far. Removing player...");
                    if (playersMap.TryRemove(playerState.walletId, out _))
                        DestroyImmediate(controller.gameObject);
                }
                else
                    controller.UpdatePlayerState(playerState);
            }
            else if (ShouldCreateAvatar(playerState))
            {
                var avatar = Instantiate(avatarPrefab, transform);
                var c = avatar.GetComponent<AvatarController>();
                c.SetAnotherPlayer(playerState.walletId, playerState.GetPosition());
                if (playersMap.TryAdd(playerState.walletId, c))
                {
                    playerState.teleport = true;
                    StartCoroutine(UpdatePlayerStateInNextFrame(c, playerState));
                    Debug.Log($"{playerState.walletId} | New player detected");
                }
                else
                    DestroyImmediate(c.gameObject);
                
            }
            // else if (playersMap.Count < maxPlayersAllowed)
            //     Debug.Log($"{playerState.walletId} | New player detected but it is too far. Ignoring state...");
            // else
            //     Debug.Log(
            //         $"{playerState.walletId} | New player detected but exceeds the total number of players. Ignoring state...");
        }

        private bool ShouldCreateAvatar(AvatarController.PlayerState state)
        {
            if (state?.position == null || playersMap.Count >= maxPlayersAllowed)
                return false;
            return GetDistanceToMainPlayer(state) <= maxCreateAvatarDistance;
        }

        private bool ShouldDestroyAvatar(AvatarController.PlayerState state)
        {
            if (state?.position == null)
                return true;
            return GetDistanceToMainPlayer(state) >= minDestroyAvatarDistance;
        }

        private static float GetDistanceToMainPlayer(AvatarController.PlayerState state)
        {
            var distance = state.GetPosition() - Player.INSTANCE.GetPosition();
            distance.y = 0;
            return distance.magnitude;
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