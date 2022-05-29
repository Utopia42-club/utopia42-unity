using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace src
{
    public class Players : MonoBehaviour
    {
        [SerializeField] public GameObject avatarPrefab;

        public Dictionary<string, AvatarController> playersMap = new Dictionary<string, AvatarController>();

        public void ReportOtherPlayersState(AvatarController.PlayerState playerState)
        {
            if (playersMap.TryGetValue(playerState.walletId, out var controller))
            {
                controller.UpdatePlayerState(playerState);
            }
            else
            {
                var avatar = Instantiate(avatarPrefab, transform);
                var c = avatar.GetComponent<AvatarController>();
                c.SetIsAnotherPlayer(true);
                c.UpdatePlayerState(playerState);
                playersMap.Add(playerState.walletId, c);
            }
        }

        public void ReportOtherPlayersState(string state)
        {
            ReportOtherPlayersState(JsonConvert.DeserializeObject<AvatarController.PlayerState>(state));
        }
    }
}