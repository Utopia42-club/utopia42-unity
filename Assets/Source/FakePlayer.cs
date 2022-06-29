using System.Collections;
using Source.Model;
using UnityEngine;

namespace Source
{
    public class FakePlayer : MonoBehaviour
    {
        [SerializeField] private float delay = 0.15f;

        private void Start()
        {
            Player.INSTANCE.mainPlayerStateReport.AddListener(state => { StartCoroutine(SendState(state)); });
        }

        private IEnumerator SendState(AvatarController.PlayerState state)
        {
            yield return new WaitForSeconds(delay);
            var pos = state.GetPosition() + Vector3.forward * 4;
            Players.INSTANCE.ReportOtherPlayersState(new AvatarController.PlayerState("random",
                new SerializableVector3(pos), state.floating, state.jump, state.sprinting, state.velocityY
            ));
        }
    }
}