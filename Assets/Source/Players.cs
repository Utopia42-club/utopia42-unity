using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Source
{
    public class Players : MonoBehaviour
    {
        private const int MaxInactivityDelay = 2; // in seconds
        private double lastInActivityCheck = 0;

        [SerializeField] public GameObject avatarPrefab;

        public readonly ConcurrentDictionary<string, AvatarController> playersMap = new();

        private void Update()
        {
            if (Time.unscaledTimeAsDouble - lastInActivityCheck < MaxInactivityDelay) return; // TODO?
            lastInActivityCheck = Time.unscaledTimeAsDouble;
            var wallets = playersMap.Keys.ToList();
            foreach (var wallet in wallets)
            {
                var controller = playersMap[wallet];
                if (Time.unscaledTimeAsDouble - controller.UpdatedTime < MaxInactivityDelay) continue;
                Debug.LogWarning("Player " + wallet + " was inactive for too long. Removing avatar...");
                playersMap.TryRemove(wallet, out _);
                DestroyImmediate(controller);
            }
        }

        public void ReportOtherPlayersStateFromWeb(string state)
        {
            ReportOtherPlayersState(JsonConvert.DeserializeObject<AvatarController.PlayerState>(state));
        }

        public void ReportOtherPlayersState(AvatarController.PlayerState playerState)
        {
            if (playerState == null) return;
            if (playerState.walletId.Equals(AuthService.WalletId()))
            {
                Debug.LogWarning("Cannot add another player with the same wallet. Ignoring state...");
            }
            else if (playersMap.TryGetValue(playerState.walletId, out var controller))
            {
                controller.UpdatePlayerState(playerState);
            }
            else
            {
                Debug.Log("New player detected");
                var avatar = Instantiate(avatarPrefab, transform);
                var c = avatar.GetComponent<AvatarController>();
                c.SetAnotherPlayer(playerState.walletId);
                playersMap.TryAdd(playerState.walletId, c);
                StartCoroutine(UpdatePlayerStateInNextFrame(c, playerState));
            }
        }

        private IEnumerator UpdatePlayerStateInNextFrame(AvatarController controller,
            AvatarController.PlayerState playerState)
        {
            yield return null;
            controller.UpdatePlayerState(playerState);
        }

        public static Players INSTANCE => GameObject.Find("Players").GetComponent<Players>();
    }
}