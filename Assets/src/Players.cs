using System.Collections;
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
                StartCoroutine(UpdatePlayerState(c, playerState, smooth)); // apparently it needs a delay
                playersMap.Add(playerState.walletId, c);
            }
        }

        private IEnumerator UpdatePlayerState(AvatarController controller, AvatarController.PlayerState playerState,
            bool smooth)
        {
            yield return 0;
            controller.UpdatePlayerState(playerState, smooth);
        }

        public void ReportOtherPlayersState(string state)
        {
            ReportOtherPlayersState(JsonConvert.DeserializeObject<AvatarController.PlayerState>(state));
        }
    }
}