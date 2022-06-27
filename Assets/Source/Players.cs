using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Source
{
    public class Players : MonoBehaviour
    {
        [SerializeField] public GameObject avatarPrefab;

        public readonly Dictionary<string, AvatarController> playersMap = new Dictionary<string, AvatarController>();

        public void ReportOtherPlayersStateFromWeb(string state)
        {
            ReportOtherPlayersState(JsonConvert.DeserializeObject<AvatarController.PlayerState>(state));
        }
        
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
                StartCoroutine(UpdatePlayerState(c, playerState));
                playersMap.Add(playerState.walletId, c);
            }
        }

        private IEnumerator UpdatePlayerState(AvatarController controller, AvatarController.PlayerState playerState)
        {
            yield return 0;
            controller.UpdatePlayerState(playerState);
        }
        
        public static Players INSTANCE => GameObject.Find("Players").GetComponent<Players>();
    }
}