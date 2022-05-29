using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace src
{
    public class Players : MonoBehaviour
    {
        [SerializeField] public GameObject avatarPrefab;

        private Dictionary<string, AvatarController> playersMap = new Dictionary<string, AvatarController>();

        public void ReportOtherPlayersState(string state)
        {
            var playerState = JsonConvert.DeserializeObject<AvatarController.PlayerState>(state);
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
                Debug.Log("Updating Other Player State " + playerState);
                playersMap.Add(playerState.walletId, c);
            }
        }
    }
}