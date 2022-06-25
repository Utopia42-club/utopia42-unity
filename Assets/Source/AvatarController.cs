using System;
using System.Collections;
using Source.Canvas;
using Source.MetaBlocks.TeleportBlock;
using Source.Model;
using Source.Ui.Login;
using Source.Ui.Menu;
using UnityEngine;

namespace Source
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

        private Vector3 targetPosition;

        private IEnumerator teleportCoroutine;

        public void Start()
        {
            avatar = Instantiate(avatarPrefab, transform);
            animator = avatar.GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
            if (!isAnotherPlayer)
                StartCoroutine(UpdateAnimationCoroutine());
        }

        private void Update()
        {
            var currentPosition = transform.position;
            if (targetPosition != currentPosition)
            {
                var movement = targetPosition - currentPosition;
                if (movement.sqrMagnitude > 10000)
                {
                    Move(movement);
                }
                else
                {
                    var step = Player.INSTANCE.walkSpeed * Time.deltaTime;
                    var newPos = Vector3.MoveTowards(currentPosition, targetPosition, step);
                    Move(newPos - currentPosition);
                }
            }
        }

        public void SetIsAnotherPlayer(bool b)
        {
            isAnotherPlayer = b;
        }

        public void Move(Vector3 motion)
        {
            controller.Move(motion);
        }

        private void LookAt(Vector3 cameraForward)
        {
            cameraForward.y = 0;
            transform.rotation = Quaternion.LookRotation(cameraForward);
        }

        public void SetPosition(Vector3 target)
        {
            targetPosition = target;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void SetAvatarBodyDisabled(bool disabled)
        {
            if (avatar != null)
                avatar.SetActive(!disabled);
        }

        public void UpdatePlayerState(PlayerState playerState)
        {
            SetPosition(playerState.position.ToVector3());
            LookAt(playerState.forward.ToVector3());
            state = playerState;
            if (isAnotherPlayer)
                UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (state == null) return;
            var lastPos = lastAnimationState?.Position() ?? state.Position();
            var movement = state.Position() - lastPos;
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
                    new PlayerState(Login.WalletId(), state.position, state.forward, state.sprint, state.floating,
                        true));
        }

        IEnumerator UpdateAnimationCoroutine()
        {
            yield return 0;
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

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("TeleportPortal")) return;
            var metaBlock = other.GetComponent<MetaFocusable>()?.MetaBlockObject;
            if (metaBlock == null || metaBlock is not TeleportBlockObject teleportBlockObject) return;
            var props = teleportBlockObject.Block.GetProps() as TeleportBlockProperties;
            if (props == null) return;

            teleportCoroutine = CountDownTimer(5, i => { },
                () =>
                {
                    GameManager.INSTANCE.MovePlayerTo(new Vector3(props.destination[0], props.destination[1],
                        props.destination[2]));
                });
            StartCoroutine(teleportCoroutine);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("TeleportPortal"))
            {
                if (teleportCoroutine != null)
                    StopCoroutine(teleportCoroutine);
            }
        }

        private IEnumerator CountDownTimer(int time, Action<int> onValueChanged, Action onFinish)
        {
            while (true)
            {
                if (time == 0)
                {
                    onFinish();
                    yield break;
                }
                else
                {
                    onValueChanged(time);
                    time = time - 1;
                }

                yield return new WaitForSeconds(1);
            }
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