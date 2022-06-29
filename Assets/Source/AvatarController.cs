using System;
using System.Collections;
using ReadyPlayerMe;
using Source.Canvas;
using Source.MetaBlocks.TeleportBlock;
using Source.Model;
using Source.Utils;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Source
{
    public class AvatarController : MonoBehaviour
    {
        public static readonly string DefaultAvatarUrl =
            "https://d1a370nemizbjq.cloudfront.net/d7a562b0-2378-4284-b641-95e5262e28e5.glb";

        private const int MaxReportDelay = 1; // in seconds 
        public const float AnimationUpdateRate = 0.1f; // in seconds

        private AvatarLoader avatarLoader;

        private Animator animator;
        private CharacterController controller;
        public GameObject Avatar { private set; get; }
        private string loadingAvatarUrl;
        private int remainingAvatarLoadAttempts;
        public double UpdatedTime { private set; get; }
        private double lastAnimationUpdateTime;
        private double lastReportedTime;
        private PlayerState lastAnimationState;
        private PlayerState lastReportedState;
        public PlayerState State { private set; get; }
        private Vector3 targetPosition;
        private bool isAnotherPlayer = false;

        private int animIDSpeed;
        private int animIDGrounded;
        private int animIDJump;

        private int animIDFreeFall;
        // private int animIDMotionSpeed;

        private IEnumerator teleportCoroutine;

        private const int Precision = 4;
        private static readonly float FloatPrecision = Mathf.Pow(10, -Precision);

        // private Vector3 anotherPlayerVelocity;

        public void Start()
        {
            controller = GetComponent<CharacterController>();
            UpdatedTime = -2 * AnimationUpdateRate;
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            // animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        public void LoadDefaultAvatar()
        {
            ReloadAvatar(DefaultAvatarUrl);
        }

        private void FixedUpdate()
        {
            if (isAnotherPlayer || GameManager.INSTANCE.GetState() != GameManager.State.PLAYING)
            {
                var currentPosition = CurrentPosition();
                var movement = targetPosition - currentPosition;
                var xzMovementMagnitude = new Vector3(movement.x, 0, movement.z).magnitude;
                // var yMovementMagnitude = new Vector3(0, movement.y, 0).magnitude;
                if (xzMovementMagnitude < FloatPrecision && (!isAnotherPlayer
                        // || yMovementMagnitude < FloatPrecision
                    ))
                {
                    // if (isAnotherPlayer)
                    //     controller.Move(targetPosition - currentPosition);
                }
                else if (xzMovementMagnitude > 100)
                    controller.Move(movement);
                else
                {
                    var xzVelocity = State is not {sprinting: true}
                        ? Player.INSTANCE.walkSpeed
                        : Player.INSTANCE.sprintSpeed;

                    var xzStepTarget = Vector3.MoveTowards(currentPosition,
                        new Vector3(targetPosition.x, currentPosition.y, targetPosition.z),
                        xzVelocity * Time.fixedDeltaTime);

                    var yStepTarget = Vector3.MoveTowards(currentPosition,
                        new Vector3(currentPosition.x, targetPosition.y, currentPosition.z),
                        Player.INSTANCE.sprintSpeed * Time.fixedDeltaTime); // TODO: enhance falling speed

                    var m = new Vector3(xzStepTarget.x, yStepTarget.y, xzStepTarget.z) - currentPosition;
                    if (!controller.isGrounded && State is {floating: false})
                        m += FloatPrecision * Vector3.down;
                    m = Vectors.Truncate(m, Precision);

                    // if (isAnotherPlayer)
                    //     anotherPlayerVelocity = m / Time.fixedDeltaTime;

                    controller.Move(m);

                    if (xzMovementMagnitude < FloatPrecision && PlayerState.Equals(State, lastAnimationState)
                                                             && Time.fixedUnscaledTimeAsDouble -
                                                             lastAnimationUpdateTime > AnimationUpdateRate)
                        SetSpeed(0);
                }
            }

            if (isAnotherPlayer
                && controller.isGrounded
                // && anotherPlayerVelocity.y <= 0 && (-anotherPlayerVelocity.y) < FloatPrecision
                // && State is {floating: false}
               )
            {
                SetGrounded(true);
                SetFreeFall(false);
                SetJump(false);
            }
        }

        public void SetAnotherPlayer()
        {
            isAnotherPlayer = true;
        }

        private void UpdateLookDirection(Vector3 movement)
        {
            movement.y = 0;
            if (movement.magnitude > FloatPrecision)
                LookAt(movement.normalized);
        }

        private void LookAt(Vector3 forward)
        {
            forward.y = 0;
            Avatar.transform.rotation = Quaternion.LookRotation(forward);
        }

        public void SetPosition(Vector3 target)
        {
            targetPosition = Vectors.Truncate(target, Precision);
        }

        public Vector3 CurrentPosition()
        {
            return Vectors.Truncate(transform.position, Precision);
        }

        public void SetAvatarBodyDisabled(bool disabled)
        {
            if (Avatar != null)
                Avatar.SetActive(!disabled);
        }

        public void UpdatePlayerState(PlayerState playerState)
        {
            if (playerState == null) return;
            SetPosition(playerState.position.ToVector3());
            if (playerState.avatarUrl != null && !playerState.avatarUrl.Equals(loadingAvatarUrl))
                ReloadAvatar(playerState.avatarUrl);
            SetPlayerState(playerState);

            var pos = State.GetPosition();
            var movement = pos - (lastAnimationState?.GetPosition() ?? pos);
            UpdateLookDirection(movement);

            var floatOrJumpStateChanged =
                playerState.floating != lastAnimationState?.floating || playerState.jump != lastAnimationState?.jump;

            if (!isAnotherPlayer && !floatOrJumpStateChanged &&
                Time.fixedUnscaledTimeAsDouble - lastAnimationUpdateTime < AnimationUpdateRate) return;

            if (Avatar != null)
            {
                // var velocity = movement / (float) (Time.unscaledTimeAsDouble - lastAnimationUpdateTime);
                // UpdateAnimation(velocity);


                var velocity = new Vector3(movement.x, 0, movement.z).normalized *
                               (State.sprinting ? Player.INSTANCE.sprintSpeed : Player.INSTANCE.walkSpeed) +
                               new Vector3(0, movement.y, 0).normalized * State.velocityY; // TODO ?
                UpdateAnimation(velocity);
            }

            if (isAnotherPlayer) return;
            if (floatOrJumpStateChanged ||
                Time.unscaledTimeAsDouble - lastReportedTime >
                (PlayerState.Equals(lastReportedState, State)
                    ? MaxReportDelay
                    : AnimationUpdateRate))
                ReportToServer();
        }
        // }

        private void SetPlayerState(PlayerState state)
        {
            State = state;
            UpdatedTime = Time.unscaledTimeAsDouble;
        }

        private void UpdateAnimation(Vector3 velocity)
        {
            var xzVelocity = new Vector3(velocity.x, 0, velocity.z);
            var grounded = controller.isGrounded;
            if (grounded)
            {
                SetJump(false);
                SetFreeFall(false);
            }

            if (State is {jump: true} && !PlayerState.Equals(State, lastAnimationState))
                SetJump(true);
            SetFreeFall(Mathf.Abs(velocity.y) > 0.1 && !grounded && State is not {floating: true});

            SetGrounded(grounded);
            SetSpeed(xzVelocity.magnitude);

            lastAnimationState = State;
            lastAnimationUpdateTime = Time.unscaledTimeAsDouble;
        }

        private void ReportToServer()
        {
            BrowserConnector.INSTANCE.ReportPlayerState(State);
            lastReportedState = State;
            lastReportedTime = Time.unscaledTimeAsDouble;
            Player.INSTANCE.mainPlayerStateReport.Invoke(State);
        }

        private void ReloadAvatar(string url, Action onDone = null)
        {
            if (url == null || url.Equals(loadingAvatarUrl) || url.Equals(lastAnimationState?.avatarUrl) ||
                remainingAvatarLoadAttempts != 0) return; // TODO?
            remainingAvatarLoadAttempts = 5;
            loadingAvatarUrl = url;

            avatarLoader = new AvatarLoader {UseAvatarCaching = true};
            avatarLoader.OnCompleted += (_, args) =>
            {
                if (isAnotherPlayer)
                    Debug.Log("Avatar loaded for another player");
                if (Avatar != null) DestroyImmediate(Avatar);
                Avatar = args.Avatar;
                Avatar.gameObject.transform.SetParent(transform);
                Avatar.gameObject.transform.localPosition = Vector3.zero;
                animator = Avatar.GetComponent<Animator>();
                onDone?.Invoke();
                remainingAvatarLoadAttempts = 0;
            };
            avatarLoader.OnFailed += (_, args) =>
            {
                remainingAvatarLoadAttempts -= 1;
                switch (args.Type)
                {
                    case FailureType.None or FailureType.ModelDownloadError or FailureType.MetadataDownloadError
                        or FailureType.NoInternetConnection:
                        Debug.Log("Failed to load the avatar: " + args.Type + " | Remaining attempts: " +
                                  remainingAvatarLoadAttempts);
                        if (remainingAvatarLoadAttempts > 0)
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

        private void OnDestroy()
        {
            if (Avatar != null)
                DestroyImmediate(Avatar);
        }

        public class PlayerState
        {
            public string walletId;
            public SerializableVector3 position;
            public bool floating;
            public bool jump;
            public bool sprinting;
            public string avatarUrl;
            // public float velocityY;

            public PlayerState(string walletId, SerializableVector3 position, bool floating, bool jump, bool sprinting,
                float velocityY)
            {
                this.walletId = walletId;
                this.position = position;
                this.floating = floating;
                this.jump = jump;
                this.sprinting = sprinting;
                avatarUrl = DefaultAvatarUrl;
                // this.velocityY = velocityY;
            }

            public Vector3 GetPosition()
            {
                return position.ToVector3();
            }

            public static bool Equals(PlayerState s1, PlayerState s2)
            {
                return !(s1 == null
                         || s2 == null
                         || !Equals(s1.avatarUrl, s2.avatarUrl)
                         || !Equals(s1.position, s2.position)
                         || s1.floating != s2.floating
                         || s1.jump != s2.jump
                         || s1.sprinting != s2.sprinting);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("TeleportPortal")) return;
            var metaBlock = other.GetComponent<MetaFocusable>()?.MetaBlockObject;
            if (metaBlock == null || metaBlock is not TeleportBlockObject teleportBlockObject) return;
            var props = teleportBlockObject.Block.GetProps() as TeleportBlockProperties;
            if (props == null) return;

            teleportCoroutine = CountDownTimer(5, _ => { },
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
    }
}