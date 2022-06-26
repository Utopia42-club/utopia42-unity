using System;
using System.Collections;
using ReadyPlayerMe;
using Source.Canvas;
using Source.MetaBlocks.TeleportBlock;
using Source.Model;
using Source.Ui.Menu;
using UnityEngine;

namespace Source
{
    public class AvatarController : MonoBehaviour
    {
        public static readonly string DefaultAvatarUrl =
            "https://d1a370nemizbjq.cloudfront.net/d7a562b0-2378-4284-b641-95e5262e28e5.glb";

        private AvatarLoader avatarLoader;

        public GameObject avatarPrefab;

        public float positionChangeThreshold = 0.1f;
        public float cameraRotationThreshold = 0.1f;

        private Animator animator;
        private CharacterController controller;
        public GameObject Avatar { private set; get; }
        private string loadingAvatarUrl;

        private float updatedTime;
        private PlayerState lastAnimationState;
        private PlayerState lastReportedState;
        private PlayerState state;

        private bool isAnotherPlayer = false;

        private Vector3 targetPosition;

        private IEnumerator teleportCoroutine;

        private int animIDSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;

        // private int animIDMotionSpeed;

        private int remainingAttempts;

        public void Start()
        {
            controller = GetComponent<CharacterController>();
            if (!isAnotherPlayer)
                StartCoroutine(UpdateAnimationCoroutine());
            AssignAnimationIDs();
        }

        public void LoadDefaultAvatar()
        {
            ReloadAvatar(DefaultAvatarUrl);
        }

        private void ReloadAvatar(string url, Action onDone = null)
        {
            if (url == null || url.Equals(loadingAvatarUrl) || url.Equals(lastAnimationState?.avatarUrl) ||
                remainingAttempts != 0) return; // TODO?
            remainingAttempts = 5;
            loadingAvatarUrl = url;

            avatarLoader = new AvatarLoader {UseAvatarCaching = true};
            avatarLoader.OnCompleted += (sender, args) =>
            {
                if (isAnotherPlayer)
                    Debug.Log("Avatar loaded for another player");
                if (Avatar != null) DestroyImmediate(Avatar);
                Avatar = args.Avatar;
                Avatar.gameObject.transform.SetParent(transform);
                Avatar.gameObject.transform.localPosition = Vector3.zero;
                animator = Avatar.GetComponent<Animator>();
                onDone?.Invoke();
                remainingAttempts = 0;
            };
            avatarLoader.OnFailed += (sender, args) =>
            {
                remainingAttempts -= 1;
                switch (args.Type)
                {
                    case FailureType.None or FailureType.ModelDownloadError or FailureType.MetadataDownloadError
                        or FailureType.NoInternetConnection:
                        Debug.Log("Failed to load the avatar: " + args.Type + " | Remaining attempts: " +
                                  remainingAttempts);
                        if (remainingAttempts > 0)
                            avatarLoader.LoadAvatar(url);
                        break;
                    //retry 
                    default:
                        Debug.Log("Invalid avatar: " + args.Type);
                        break;
                }
            };
            avatarLoader.LoadAvatar(url);
        }

        private void AssignAnimationIDs()
        {
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            // animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
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

        private void UpdateLookDirection()
        {
            var movement = state.GetPosition() - (lastAnimationState?.GetPosition() ?? state.GetPosition());
            movement.y = 0;
            if (movement.magnitude > 0.001)
                LookAt(movement.normalized);
        }

        private void LookAt(Vector3 forward)
        {
            if (Avatar == null) return;
            forward.y = 0;
            Avatar.transform.rotation = Quaternion.LookRotation(forward);
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
            if (Avatar != null)
                Avatar.SetActive(!disabled);
        }

        public void UpdatePlayerState(PlayerState playerState)
        {
            state = playerState;
            if (state == null) return;
            if (state.avatarUrl != null && !state.avatarUrl.Equals(loadingAvatarUrl)) ReloadAvatar(state.avatarUrl);
            SetPosition(state.position.ToVector3());
            UpdateLookDirection();
            if (isAnotherPlayer)
                UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (state == null || Avatar == null) return;

            var lastPos = lastAnimationState?.GetPosition() ?? state.GetPosition();
            var movement = state.GetPosition() - lastPos;
            var velocity = movement / (Time.time - updatedTime);
            var xzVelocity = new Vector3(velocity.x, 0, velocity.z);
            updatedTime = Time.time;
            lastAnimationState = state;

            if (state.grounded)
            {
                SetJump(false);
                SetFreeFall(false);
            }

            if (state.jump)
                SetJump(true);
            SetFreeFall(Mathf.Abs(velocity.y) > 0.1 && !state.grounded && !state.floating);

            SetGrounded(state.grounded);
            SetSpeed(Mathf.Pow(xzVelocity.sqrMagnitude, 0.5f));
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
                   || !Equals(s1.avatarUrl, s2.avatarUrl)
                   || !Equals(s1.position, s2.position)
                   || s1.floating != s2.floating
                   || s1.jump != s2.jump
                   || s1.grounded != s2.grounded;
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
            public bool floating;
            public bool jump;
            public bool grounded;
            public string avatarUrl;

            public PlayerState(string walletId, SerializableVector3 position, bool floating, bool jump, bool grounded)
            {
                this.walletId = walletId;
                this.position = position;
                this.floating = floating;
                this.jump = jump;
                this.grounded = grounded;
                avatarUrl = DefaultAvatarUrl;
            }

            public Vector3 GetPosition()
            {
                return position.ToVector3();
            }
        }

        private void SetJump(bool jump)
        {
            animator.SetBool(animIDJump, jump);
        }

        private void SetFreeFall(bool freeFall)
        {
            animator.SetBool(animIDFreeFall, freeFall);
        }

        private void SetGrounded(bool grounded)
        {
            animator.SetBool(animIDGrounded, grounded);
        }

        private void SetSpeed(float speed)
        {
            animator.SetFloat(animIDSpeed, speed);
        }
    }
}