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
        private const string DefaultAvatarUrl =
            "https://d1a370nemizbjq.cloudfront.net/8b6189f0-c999-4a6a-bffc-7f68d66b39e6.glb"; //FIXME Configuration?
        // "https://d1a370nemizbjq.cloudfront.net/d7a562b0-2378-4284-b641-95e5262e28e5.glb";

        private const string RendererWarningMessage = "Your avatar is too complex. Loading the default...";
        private const string AvatarLoadedMessage = "Avatar loaded";
        private const string AvatarLoadRetryMessage = "Failed to load the avatar. Retrying...";
        private const string AvatarLoadFailedMessage = "Failed to load the avatar. Loading the default...";

        private const int MaxReportDelay = 1; // in seconds 
        private const float AnimationUpdateRate = 0.1f; // in seconds

        private Animator animator;
        private CharacterController controller;
        public GameObject Avatar { private set; get; }
        private string loadingAvatarUrl;
        private int remainingAvatarLoadAttempts;
        public double UpdatedTime;
        private double lastPerformedStateTime;
        private double lastReportedTime;
        private PlayerState lastPerformedState;
        private PlayerState lastReportedState;
        private PlayerState state;
        private Vector3 targetPosition;
        private bool isAnotherPlayer = false;
        public bool Initialized { get; private set; }

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
        public bool AvatarAllowed { get; private set; }

        private bool ControllerEnabled => controller != null && controller.enabled && !controllerDisabled; // TODO!

        public void Start()
        {
            Initialized = false;
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

        private void LoadDefaultAvatar(bool resetAvatarMsg = true)
        {
            ReloadAvatar(DefaultAvatarUrl, false, resetAvatarMsg);
        }

        private IEnumerator LoadAvatarFromWallet(string walletId)
        {
            yield return null;
            ProfileLoader.INSTANCE.load(walletId, profile =>
            {
                if (profile != null && profile.avatarUrl != null && profile.avatarUrl.Length > 0)
                    ReloadAvatar(profile.avatarUrl);
                // ReloadAvatar("https://d1a370nemizbjq.cloudfront.net/d7a562b0-2378-4284-b641-95e5262e28e5.glb"); // complex default
                // ReloadAvatar("https://d1a370nemizbjq.cloudfront.net/3343c701-0f84-4a57-8c0e-eb25724a2133.glb"); // simple with transparent
                else
                    LoadDefaultAvatar();
            }, () => LoadDefaultAvatar());
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

        public void SetAnotherPlayer(string walletId, Vector3 position, bool makeVisible)
        {
            Initialized = true;
            isAnotherPlayer = true;
            AvatarAllowed = false;
            ProfileLoader.INSTANCE.load(walletId,
                profile => nameLabel.text = profile?.name ?? MakeWalletShorter(walletId),
                () => nameLabel.text = MakeWalletShorter(walletId));
            if (makeVisible)
                LoadAnotherPlayerAvatar(walletId);
            var target = Vectors.Truncate(position, Precision);
            SetTargetPosition(target);
        }

        public void LoadAnotherPlayerAvatar(string walletId)
        {
            AvatarAllowed = true;
            StartCoroutine(LoadAvatarFromWallet(walletId));
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
            Initialized = true;
            isAnotherPlayer = false;
            AvatarAllowed = true;
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
            Avatar.transform.rotation = Quaternion.LookRotation(forward);
        }

        private void SetTargetPosition(Vector3 target)
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
            if (Avatar != null)
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

        public void ReloadAvatar(string url, bool ignorePreviousUrl = false,
            bool resetAvatarMsg = true)
        {
            if (url == null || !ignorePreviousUrl && url.Equals(loadingAvatarUrl) ||
                remainingAvatarLoadAttempts != 0) return;
            if (resetAvatarMsg)
                GameManager.INSTANCE.ResetAvatarMsg();
            remainingAvatarLoadAttempts = 3;
            loadingAvatarUrl = url;

            AvatarLoader.INSTANCE.AddJob(gameObject, url, OnAvatarLoad, OnAvatarLoadFailure);
        }

        private void OnAvatarLoadFailure(FailureType failureType)
        {
            remainingAvatarLoadAttempts -= 1;
            if (remainingAvatarLoadAttempts > 0 && failureType != FailureType.UrlProcessError)
            {
                Debug.LogWarning(
                    $"{state?.walletId} | {failureType} : {AvatarLoadRetryMessage} | Remaining attempts: " +
                    remainingAvatarLoadAttempts);
                if (!isAnotherPlayer)
                    GameManager.INSTANCE.ShowAvatarStateMessage(AvatarLoadRetryMessage, false);
                AvatarLoader.INSTANCE.AddJob(gameObject, loadingAvatarUrl, OnAvatarLoad, OnAvatarLoadFailure);
            }
            else
            {
                Debug.LogWarning(
                    $"{state?.walletId} | {failureType} : {AvatarLoadFailedMessage}");
                if (!isAnotherPlayer)
                    GameManager.INSTANCE.ShowAvatarStateMessage(AvatarLoadFailedMessage, true);
                remainingAvatarLoadAttempts = 0;
                LoadDefaultAvatar(false);
            }
        }

        private void OnAvatarLoad(GameObject avatar)
        {
            if (avatar.GetComponentsInChildren<Renderer>().Length > 2)
            {
                Debug.LogWarning($"{state?.walletId} | {RendererWarningMessage}");
                if (!isAnotherPlayer)
                    GameManager.INSTANCE.ShowAvatarStateMessage(RendererWarningMessage, true);
                MetaBlockObject.DeepDestroy3DObject(avatar);
                remainingAvatarLoadAttempts = 0;
                LoadDefaultAvatar(false);
                return;
            }

            if (isAnotherPlayer)
                Debug.Log($"{state?.walletId} | {AvatarLoadedMessage}");
            PrepareAvatar(avatar);
            animator = Avatar.GetComponentInChildren<Animator>();
            remainingAvatarLoadAttempts = 0;
        }

        private void PrepareAvatar(GameObject go)
        {
            if (Avatar != null)
                MetaBlockObject.DeepDestroy3DObject(Avatar);

            if (isAnotherPlayer)
            {
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                Avatar = go;
                return;
            }

            var container = new GameObject {name = "AvatarContainer"};
            container.transform.SetParent(transform);
            container.transform.localPosition = Vector3.zero;
            go.transform.SetParent(container.transform);
            go.transform.localPosition = new Vector3(.06f, 0, -.02f);
            Avatar = container;
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