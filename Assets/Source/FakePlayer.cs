using System;
using System.Collections;
using Source.Model;
using Source.Service.Auth;
using UnityEngine;

namespace Source
{
    public class FakePlayer : MonoBehaviour
    {
        [SerializeField] private float delay = 0.15f;
        [SerializeField] private float forwardDistance = 4f;
        private string avatarId;
        private Vector3 forward;

        private void Start()
        {
            avatarId = Guid.NewGuid().ToString();
            Player.INSTANCE.mainPlayerStateReport.AddListener(state => { StartCoroutine(SendState(state)); });
            forward = Player.INSTANCE.PlayerForward;
        }

        private IEnumerator SendState(AvatarController.PlayerState state)
        {
            yield return new WaitForSeconds(delay);
            var pos = state.GetPosition() + forward * forwardDistance;
            var contract = AuthService.Instance.CurrentContract;
            var s = new AvatarController.PlayerState(contract.networkId, contract.address,
                avatarId, new SerializableVector3(pos), state.floating, state.jump,
                state.sprinting, state.velocityY, state.teleport)
            {
                rid = state.rid
            };
            Players.INSTANCE.ReportOtherPlayersState(s);
        }
    }
}