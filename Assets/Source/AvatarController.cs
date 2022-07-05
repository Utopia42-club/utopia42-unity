using System;
using System.Collections;
using ReadyPlayerMe;
using Source.Canvas;
using Source.MetaBlocks;
using Source.MetaBlocks.TeleportBlock;
using Source.Model;
using Source.Ui.Profile;
using Source.Utils;
using TMPro;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Source
{
    public class AvatarController : MonoBehaviour
    {
        public static readonly string DefaultAvatarUrl =
            "https://d1a370nemizbjq.cloudfront.net/8b6189f0-c999-4a6a-bffc-7f68d66b39e6.glb"; //FIXME Configuration?

        private const int MaxReportDelay = 1; // in seconds 
        private const float AnimationUpdateRate = 0.1f; // in seconds

        private AvatarLoader avatarLoader;

        private Animator animator;
        private CharacterController controller;
        public GameObject Avatar { private set; get; }
        private string loadingAvatarUrl;
        private int remainingAvatarLoadAttempts;
        public double UpdatedTime { private set; get; }
        private double lastPerformedStateTime;
        private double lastReportedTime;
        private PlayerState lastPerformedState;
        private PlayerState lastReportedState;
        private PlayerState state;
        private Vector3 targetPosition;
        private bool isAnotherPlayer = false;

        private int animIDSpeed;
        private int animIDGrounded;
        private int animIDJump;
        private int animIDFreeFall;

        private IEnumerator teleportCoroutine;

        private const int Precision = 5;
        private static readonly float FloatPrecision = Mathf.Pow(10, -Precision);
        private Vector3 movement;
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private GameObject namePanel;
        private bool controllerDisabled;
        private GameObject avatarContainer;

        private bool ControllerEnabled => controller != null && controller.enabled && !controllerDisabled; // TODO!

        public void Start()
        {
            avatarContainer = new GameObject {name = "AvatarContainer"};
            avatarContainer.transform.SetParent(transform);
            avatarContainer.transform.localPosition = Vector3.zero;
            controller = GetComponent<CharacterController>();
            UpdatedTime = -2 * AnimationUpdateRate;
            animIDSpeed = Animator.StringToHash("Speed");
            animIDGrounded = Animator.StringToHash("Grounded");
            animIDJump = Animator.StringToHash("Jump");
            animIDFreeFall = Animator.StringToHash("FreeFall");
            StartCoroutine(UpdateAnimationCoroutine());
        }

        private void Update()
        {
            namePanel.transform.rotation = Camera.main.transform.rotation;
        }

        private void LoadDefaultAvatar()
        {
            ReloadAvatar(DefaultAvatarUrl);
        }

        private IEnumerator LoadAvatarFromWallet(string walletId)
        {
            yield return null;
            ProfileLoader.INSTANCE.load(walletId, profile =>
            {
                if (profile != null && profile.avatarUrl != null && profile.avatarUrl.Length > 0)
                    ReloadAvatar(profile.avatarUrl);
                else
                    LoadDefaultAvatar();
            }, LoadDefaultAvatar);
        }

        private void FixedUpdate()
        {
            if (!isAnotherPlayer || !ControllerEnabled || state is {teleport: true}) return;

            var currentPosition = CurrentPosition();
            var xzVelocity = state is not {sprinting: true}
                ? Player.INSTANCE.walkSpeed
                : Player.INSTANCE.sprintSpeed;

            var xzStepTarget = Vector3.MoveTowards(currentPosition,
                new Vector3(targetPosition.x, currentPosition.y, targetPosition.z),
                xzVelocity * Time.fixedDeltaTime);

            var yStepTarget = Vector3.MoveTowards(currentPosition,
                new Vector3(currentPosition.x, targetPosition.y, currentPosition.z),
                Player.INSTANCE.sprintSpeed * Time.fixedDeltaTime); // TODO: enhance falling speed

            var m = new Vector3(xzStepTarget.x, yStepTarget.y, xzStepTarget.z) - currentPosition;
            if (!controller.isGrounded && state is {floating: false})
                m += 2 * FloatPrecision * Vector3.down;
            m = Vectors.Truncate(m, Precision);

            var xzMovementMagnitude =
                new Vector3(targetPosition.x - currentPosition.x, 0, targetPosition.z - currentPosition.z)
                    .magnitude;
            controller.Move(m);

            if (Avatar != null && xzMovementMagnitude < FloatPrecision &&
                PlayerState.Equals(state, lastPerformedState)
                && Time.fixedUnscaledTimeAsDouble -
                lastPerformedStateTime > AnimationUpdateRate)
                SetSpeed(0);

            if (Avatar != null && controller.isGrounded)
            {
                SetGrounded(true);
                SetFreeFall(false);
                SetJump(false);
            }
        }

        public void SetAnotherPlayer(string walletId, Vector3 position)
        {
            isAnotherPlayer = true;
            ProfileLoader.INSTANCE.load(walletId,
                profile => nameLabel.text = profile?.name ?? MakeWalletShorter(walletId),
                () => nameLabel.text = MakeWalletShorter(walletId));
            StartCoroutine(LoadAvatarFromWallet(walletId));
            var target = Vectors.Truncate(position, Precision);
            SetTargetPosition(target);
        }

        private IEnumerator SetTransformPosition(Vector3 position)
        {
            controllerDisabled = true;
            yield return null;
            var active = Avatar != null && Avatar.activeSelf;
            SetAvatarBodyActive(false);
            controller.enabled = false;
            yield return null;
            transform.position = position;
            controller.enabled = true;
            if (active) SetAvatarBodyActive(true);
            controllerDisabled = false;
        }

        public void SetMainPlayer(string walletId)
        {
            isAnotherPlayer = false;
            namePanel.gameObject.SetActive(false);
            StartCoroutine(LoadAvatarFromWallet(walletId));
            SetTargetPosition(transform.position);
        }

        private string MakeWalletShorter(string walletId)
        {
            return walletId[..6] + "..." + walletId[^5..];
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
            avatarContainer.transform.rotation = Quaternion.LookRotation(forward);
        }

        public void SetTargetPosition(Vector3 target)
        {
            targetPosition = Vectors.Truncate(target, Precision);
        }

        public Vector3 CurrentPosition()
        {
            return Vectors.Truncate(transform.position, Precision);
        }

        public void SetAvatarBodyActive(bool active)
        {
            if (Avatar != null)
                Avatar.SetActive(active);
        }

        public void UpdatePlayerState(PlayerState playerState)
        {
            if (playerState == null || isAnotherPlayer && !ControllerEnabled) return;
            SetTargetPosition(playerState.position.ToVector3());
            SetPlayerState(playerState);
            if (state.teleport)
            {
                if (!PlayerState.Equals(state, lastPerformedState))
                {
                    StartCoroutine(SetTransformPosition(targetPosition));
                    if (!isAnotherPlayer)
                        ReportToServer();
                }

                SetLastPerformedState(state);
                return;
            }

            var pos = state.GetPosition();
            movement = pos - (lastPerformedState?.GetPosition() ?? pos);
            UpdateLookDirection(movement);

            var floatOrJumpStateChanged =
                playerState.floating != lastPerformedState?.floating || playerState.jump != lastPerformedState?.jump;

            if ((isAnotherPlayer || floatOrJumpStateChanged) && Avatar != null && ControllerEnabled)
            {
                UpdateAnimation();
            }

            if (!isAnotherPlayer && floatOrJumpStateChanged)
                ReportToServer();
        }

        private void SetLastPerformedState(PlayerState playerState)
        {
            lastPerformedState = playerState;
            lastPerformedStateTime = Time.unscaledTimeAsDouble;
        }

        private IEnumerator UpdateAnimationCoroutine()
        {
            while (true)
            {
                yield return null;
                if (isAnotherPlayer)
                    yield break;

                if (Avatar != null && Time.unscaledTimeAsDouble - lastPerformedStateTime > AnimationUpdateRate
                                   && !PlayerState.Equals(state, lastPerformedState)
                                   && state != null && ControllerEnabled)
                {
                    UpdateAnimation();
                }

                if (state != null && Time.unscaledTimeAsDouble - lastReportedTime >
                    (PlayerState.Equals(lastReportedState, state)
                        ? MaxReportDelay
                        : AnimationUpdateRate))
                    ReportToServer();
            }
        }

        private void SetPlayerState(PlayerState playerState)
        {
            state = playerState;
            UpdatedTime = Time.unscaledTimeAsDouble;
        }

        private void UpdateAnimation()
        {
            var xzVelocity = new Vector3(movement.x, 0, movement.z).normalized *
                             (state.sprinting ? Player.INSTANCE.sprintSpeed : Player.INSTANCE.walkSpeed);
            var grounded = controller.isGrounded;
            if (grounded)
            {
                SetJump(false);
                SetFreeFall(false);
            }

            if (state is {jump: true} && !PlayerState.Equals(state, lastPerformedState))
                SetJump(true);
            SetFreeFall(Mathf.Abs(state.velocityY) > Player.INSTANCE.MinFreeFallSpeed
                        && !grounded && state is not {floating: true});

            SetGrounded(grounded);
            SetSpeed(xzVelocity.magnitude);

            SetLastPerformedState(state);
        }

        private void ReportToServer()
        {
            BrowserConnector.INSTANCE.ReportPlayerState(state);
            lastReportedState = state;
            lastReportedTime = Time.unscaledTimeAsDouble;
            Player.INSTANCE.mainPlayerStateReport.Invoke(state);
        }

        public void ReloadAvatar(string url, Action onDone = null)
        {
            if (url == null || url.Equals(loadingAvatarUrl) || remainingAvatarLoadAttempts != 0) return;
            remainingAvatarLoadAttempts = 3;
            loadingAvatarUrl = url;

            avatarLoader = new AvatarLoader {UseAvatarCaching = true};
            avatarLoader.OnCompleted += (_, args) =>
            {
                if (args.Avatar.GetComponentsInChildren<Renderer>().Length > 1)
                {
                    Debug.LogWarning(
                        $"{state?.walletId} | Loaded avatar has more than one renderer component | Loading the default avatar...");
                    MetaBlockObject.DeepDestroy3DObject(args.Avatar);
                    avatarLoader.LoadAvatar(DefaultAvatarUrl);
                    return;
                }

                if (isAnotherPlayer)
                    Debug.Log($"{state?.walletId} | Avatar loaded");
                if (Avatar != null)
                    MetaBlockObject.DeepDestroy3DObject(Avatar);
                Avatar = args.Avatar;
                Avatar.gameObject.transform.SetParent(avatarContainer.transform);
                Avatar.gameObject.transform.localPosition = new Vector3(.06f, 0, -.02f);
                animator = Avatar.GetComponent<Animator>();
                onDone?.Invoke();
                remainingAvatarLoadAttempts = 0;
            };
            avatarLoader.OnFailed += (_, args) =>
            {
                remainingAvatarLoadAttempts -= 1;
                if (remainingAvatarLoadAttempts > 0)
                {
                    Debug.LogWarning(
                        $"{state?.walletId} | Failed to load the avatar: {args.Type} | Remaining attempts: " +
                        remainingAvatarLoadAttempts);
                    avatarLoader.LoadAvatar(url);
                }
                else
                {
                    Debug.LogWarning(
                        $"{state?.walletId} | Failed to load the avatar: {args.Type} | Loading the default avatar...");
                    LoadDefaultAvatar();
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
                MetaBlockObject.DeepDestroy3DObject(Avatar);
            if (nameLabel != null)
                DestroyImmediate(nameLabel);
            if (namePanel != null)
                DestroyImmediate(namePanel);
        }

        public class PlayerState
        {
            public string rid;
            public string walletId;
            public SerializableVector3 position;
            public bool floating;
            public bool jump;
            public bool sprinting;
            public float velocityY;
            public bool teleport;

            public PlayerState(string walletId, SerializableVector3 position, bool floating, bool jump,
                bool sprinting, float velocityY, bool teleport)
            {
                // rid = random.Next(0, int.MaxValue);
                rid = Guid.NewGuid().ToString();
                this.walletId = walletId;
                this.position = position;
                this.floating = floating;
                this.jump = jump;
                this.sprinting = sprinting;
                this.velocityY = velocityY;
                this.teleport = teleport;
            }

            private PlayerState(string walletId, SerializableVector3 position)
            {
                this.walletId = walletId;
                this.position = position;
                teleport = true;
            }

            public static PlayerState CreateTeleportState(string walletId, Vector3 position)
            {
                return new PlayerState(walletId, new SerializableVector3(position));
            }

            public Vector3 GetPosition()
            {
                return position.ToVector3();
            }

            public static bool Equals(PlayerState s1, PlayerState s2)
            {
                return s1 != null
                       && s2 != null
                       && s1.walletId == s2.walletId
                       && s1.rid == s2.rid
                       && Equals(s1.position, s2.position)
                       && s1.floating == s2.floating
                       && s1.jump == s2.jump
                       && s1.sprinting == s2.sprinting
                       && s1.teleport == s2.teleport
                       && Math.Abs(s1.velocityY - s2.velocityY) < FloatPrecision;
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