using System;
using System.Collections;
using Source.Model;
using UnityEngine;

namespace Source
{
    public class FakePlayer : MonoBehaviour
    {
        [SerializeField] private float delay = 0.15f;
        private string avatarId;

        private void Start()
        {
            avatarId = Guid.NewGuid().ToString();
            Player.INSTANCE.mainPlayerStateReport.AddListener(state => { StartCoroutine(SendState(state)); });
        }

        private IEnumerator SendState(AvatarController.PlayerState state)
        {
            yield return new WaitForSeconds(delay);
            var pos = state.GetPosition() + Vector3.forward * 4;
            var s = new AvatarController.PlayerState(avatarId, new SerializableVector3(pos), state.floating, state.jump,
                state.sprinting, state.velocityY, state.teleport)
            {
                rid = state.rid
            };
            Players.INSTANCE.ReportOtherPlayersState(s);
        }
    }
}