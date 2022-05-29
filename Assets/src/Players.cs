using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace src
{
    public class Players : MonoBehaviour
    {
        [SerializeField] public GameObject avatarPrefab;

        public readonly Dictionary<string, AvatarController> playersMap = new Dictionary<string, AvatarController>();

        public void ReportOtherPlayersState(AvatarController.PlayerState playerState, bool smooth = true)
        {
            if (playersMap.TryGetValue(playerState.walletId, out var controller))
            {
                controller.UpdatePlayerState(playerState, smooth);
            }
            else
            {
                var avatar = Instantiate(avatarPrefab, transform);
                var c = avatar.GetComponent<AvatarController>();
                c.SetIsAnotherPlayer(true);
                c.UpdatePlayerState(playerState, smooth);
                playersMap.Add(playerState.walletId, c);
            }
        }

        public void ReportOtherPlayersState(string state)
        {
            ReportOtherPlayersState(JsonConvert.DeserializeObject<AvatarController.PlayerState>(state));
        }
    }
}