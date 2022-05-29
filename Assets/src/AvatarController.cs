using System.Collections;
using src.Canvas;
using src.Model;
using UnityEngine;

namespace src
{
    public class AvatarController : MonoBehaviour
    {
        public GameObject avatarPrefab;

        public float positionChangeThreshold = 0.1f;
        public float cameraRotationThreshold = 0.1f;

        private Animator animator;
        private CharacterController controller;
        private GameObject avatar;

        private float updatedTime;
        private PlayerState lastAnimationState;
        private PlayerState lastReportedState;
        private PlayerState state;

        private bool isAnotherPlayer = false;

        public void Start()
        {
            avatar = Instantiate(avatarPrefab, transform);
            animator = avatar.GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
            StartCoroutine(UpdateAnimationCoroutine());
        }

        public void SetIsAnotherPlayer(bool b)
        {
            isAnotherPlayer = b;
        }

        public void Move(Vector3 motion)
        {
            if (controller != null)
                controller.Move(motion);
        }

        public void LookAt(Vector3 cameraForward)
        {
            cameraForward.y = 0;
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }

        private void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void UpdatePlayerState(PlayerState playerState)
        {
            SetPosition(playerState.position.ToVector3());
            LookAt(playerState.forward.ToVector3());
            state = playerState;
        }

        private void UpdateAnimation()
        {
            var movement = state.Position() - (lastAnimationState?.Position() ?? state.Position());
            var velocity = movement / (Time.time - updatedTime);

            updatedTime = Time.time;
            lastAnimationState = state;

            if (state.floating || movement.y != 0 && movement.x == 0 && movement.z == 0)
            {
                animator.SetFloat("X", 0);
                animator.SetFloat("Z", 0);
                animator.SetFloat("Floating", state.floating ? 1 : 0);
                // if (movement.y < -0.5f && !floating)
                //     animator.CrossFade("Falling", 0.05f);
                return;
            }

            var newX = Vector3.Dot(velocity, Quaternion.Euler(0, 90, 0) * state.Forward());
            var newY = Vector3.Dot(velocity, state.Forward());

            animator.SetFloat("X", newX);
            animator.SetFloat("Z", newY);

            animator.SetFloat("Speed", state.sprint ? 0.9f : 0.1f);
        }

        public void JumpAnimation()
        {
            animator.CrossFade("Jump", 0.01f);
            if (!isAnotherPlayer)
                BrowserConnector.INSTANCE.ReportPlayerState(
                    new PlayerState(Settings.WalletId(), state.position, state.forward, state.sprint, state.floating,
                        true));
        }

        IEnumerator UpdateAnimationCoroutine()
        {
            while (true)
            {
                if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
                {
                    UpdateAnimation();
                    if (!isAnotherPlayer)
                        ReportToServer();
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        public void ReportToServer()
        {
            if (IsDifferent(state, lastReportedState))
            {
                BrowserConnector.INSTANCE.ReportPlayerState(state);
                lastReportedState = state;
            }
        }

        private bool IsDifferent(PlayerState s1, PlayerState s2)
        {
            return s1 == null
                   || s2 == null
                   || !Equals(s1.position, s2.position)
                   || !Equals(s1.forward, s2.forward)
                   // || Vector3.Distance(s1.Position(), s2.Position()) > positionChangeThreshold
                   // || Vector3.Distance(s1.Forward(), s2.Forward()) > cameraRotationThreshold
                   || s1.sprint != s2.sprint
                   || s1.floating != s2.floating;
        }

        public PlayerState GetState()
        {
            return state;
        }

        public class PlayerState
        {
            public string walletId;

            public SerializableVector3 position;

            public SerializableVector3 forward;

            public bool floating;

            public bool sprint;

            public bool jump;

            public PlayerState(string walletId, SerializableVector3 position, SerializableVector3 forward,
                bool floating, bool sprint, bool jump = false)
            {
                this.walletId = walletId;
                this.position = position;
                this.forward = forward;
                this.floating = floating;
                this.sprint = sprint;
                this.jump = jump;
            }

            public Vector3 Position()
            {
                return position.ToVector3();
            }

            public Vector3 Forward()
            {
                return forward.ToVector3();
            }
        }
    }
}