using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Source.Canvas;
using Source.MetaBlocks;
using Source.MetaBlocks.TdObjectBlock;
using Source.Model;
using Source.Service;
using Source.Ui.AssetsInventory.Models;
using Source.Ui.Menu;
using Source.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Source
{
    public class Player : MonoBehaviour
    {
        private static readonly string POSITION_KEY = "PLAYER_POSITION";

        public const float CastStep = 0.01f;
        public static readonly Vector3Int ViewDistance = new Vector3Int(5, 5, 5);

        [SerializeField] private Transform cam;
        [SerializeField] private World world;
        [SerializeField] public float walkSpeed = 6f;
        [SerializeField] public float sprintSpeed = 12f;
        [SerializeField] private float jumpHeight = 2;
        [SerializeField] private float sprintJumpHeight = 2.5f;
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private Transform highlightBlock;
        [SerializeField] private Transform placeBlock;
        [SerializeField] public Transform tdObjectHighlightBox;
        [SerializeField] public GameObject avatarPrefab;

        public BlockType SelectedBlockType { private set; get; }

        private bool sprinting;
        private Vector3 velocity = Vector3.zero;
        private Land highlightLand;
        public Land placeLand;
        private bool jumpRequest;
        private bool floating = false;
        private Vector3Int? lastChunk;
        private List<Land> ownedLands = new List<Land>();
        private Collider hitCollider;
        private RaycastHit raycastHit;
        private CharacterController characterController;
        private BlockSelectionController blockSelectionController;
        private Renderer placeBlockRenderer;
        public bool CtrlDown { private set; get; }
        private Vector3Int playerPos;
        private AvatarController avatarController;
        [NonSerialized] public GameObject avatar;
        [NonSerialized] public Transform focusHighlight;

        public MetaBlock PreparedMetaBlock { private set; get; }
        public GameObject MetaBlockPlaceHolder { private set; get; }

        public bool HammerMode { get; private set; } = true;
        public bool SelectionActiveBeforeAtFrameBeginning { get; private set; } = true;
        public Transform HighlightBlock => highlightBlock;
        public Transform PlaceBlock => placeBlock;

        public bool ChangeForbidden => Settings.IsGuest() || viewMode != ViewMode.FIRST_PERSON;

        private bool DisableRaycast => World.INSTANCE.ObjectScaleRotationController.Active;

        [SerializeField] private Vector3 firstPersonCameraPosition;
        [SerializeField] private Vector3 thirdPersonCameraPosition;
        private ViewMode viewMode = ViewMode.FIRST_PERSON;
        public UnityEvent<ViewMode> viewModeChanged;

        public Player(Transform tdObjectHighlightBox, Transform placeBlock, Transform highlightBlock)
        {
            this.tdObjectHighlightBox = tdObjectHighlightBox;
            this.placeBlock = placeBlock;
            this.highlightBlock = highlightBlock;
        }

        public float Horizontal { get; private set; }
        public float Vertical { get; private set; }
        public Focusable FocusedFocusable { get; private set; }
        public Vector3Int PossiblePlaceBlockPosInt { get; private set; }
        public Vector3 PossiblePlaceMetaBlockPos { get; private set; }
        public Vector3Int PossibleHighlightBlockPosInt { get; set; }
        public Land HighlightLand => highlightLand;
        public Transform transform => avatar?.transform; // TODO
        public RaycastHit RaycastHit => raycastHit;

        private void Start()
        {
            blockSelectionController = GetComponent<BlockSelectionController>();
            placeBlockRenderer = placeBlock.GetComponentInChildren<Renderer>();

            Snack.INSTANCE.ShowObject("Owner", null);

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING)
                    ResetRaycastMemory();
                else
                    HideCursors();
            });

            avatar = Instantiate(avatarPrefab, gameObject.transform);
            avatarController = avatar.GetComponent<AvatarController>();
            characterController = avatar.GetComponent<CharacterController>();
            cam.SetParent(avatar.transform);

            playerPos = Vectors.TruncateFloor(GetPosition());
            StartCoroutine(SavePosition());

            viewModeChanged = new UnityEvent<ViewMode>();
            ToggleViewMode();

            viewModeChanged.AddListener(vm =>
            {
                if (vm == ViewMode.THIRD_PERSON)
                {
                    BlockSelectionController.INSTANCE.ExitSelectionMode();
                    HideCursors();
                    if (FocusedFocusable != null)
                    {
                        FocusedFocusable.UnFocus();
                        FocusedFocusable = null;
                    }
                }

                ResetRaycastMemory();
            });

            HammerMode = false;
            world.UpdateHighlightBlockColor(false);
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            SelectionActiveBeforeAtFrameBeginning = World.INSTANCE.SelectionActive;
            GetInputs();

            if (!ChangeForbidden)
            {
                blockSelectionController.DoUpdate();
                if (!HammerMode && Input.GetButtonDown("Delete") && !SelectionActiveBeforeAtFrameBeginning)
                    Ui.AssetsInventory.AssetsInventory.INSTANCE.SelectSlotInfo(new SlotInfo());
            }

            if (lastChunk == null)
            {
                lastChunk = ComputePosition().chunk;
                world.OnPlayerChunkChanged(lastChunk.Value);
            }
            else
            {
                var currChunk = ComputePosition().chunk;
                if (!lastChunk.Equals(currChunk))
                {
                    lastChunk = currChunk;
                    world.OnPlayerChunkChanged(currChunk);
                }
            }
        }

        private void GetInputs()
        {
            if (GameManager.INSTANCE.IsUiEngaged())
                return;
            CtrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            Horizontal = Input.GetAxis("Horizontal");
            Vertical = Input.GetAxis("Vertical");

            if (Input.GetButtonDown("Sprint"))
                sprinting = true;

            if (Input.GetButtonUp("Sprint"))
                sprinting = false;

            if (Input.GetButtonDown("Jump"))
                jumpRequest = true;

            if (Input.GetButtonUp("Jump"))
                jumpRequest = false;

            if (Input.GetButtonDown("Toggle Floating"))
                floating = !floating;

            if (Input.GetButtonDown("Toggle View"))
                ToggleViewMode();
        }

        private void FixedUpdate()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            UpdatePlayerPosition();
            DetectFocus();
        }

        private void UpdatePlayerPosition()
        {
            if (!blockSelectionController.PlayerMovementAllowed) return;

            var moveDirection = avatar.transform.forward * Vertical + avatar.transform.right * Horizontal;

            var isGrounded = characterController.isGrounded && velocity.y < 0 || floating;

            avatarController.Move(moveDirection * (sprinting ? sprintSpeed : walkSpeed) * Time.fixedDeltaTime);

            if (isGrounded)
                velocity.y = 0f;

            if (jumpRequest)
            {
                if (!floating && isGrounded)
                    avatarController.JumpAnimation();
                velocity.y = Mathf.Sqrt((sprinting ? sprintJumpHeight : jumpHeight) * -2f * gravity);
            }

            if (!floating && !characterController.isGrounded)
                velocity.y += gravity * Time.fixedDeltaTime;

            avatarController.Move(velocity * Time.fixedDeltaTime);

            if ((characterController.collisionFlags & CollisionFlags.Above) != 0)
                velocity.y = 0;

            var pos = GetPosition();

            playerPos = Vectors.TruncateFloor(pos);

            avatarController.UpdatePlayerState(new AvatarController.PlayerState(Settings.WalletId(),
                new SerializableVector3(pos), new SerializableVector3(cam.forward), floating, sprinting));
        }


        // ReSharper disable Unity.PerformanceAnalysis
        private void DetectFocus()
        {
            if (DisableRaycast)
            {
                ResetRaycastMemory();
                return;
            }

            if (Physics.Raycast(cam.position, cam.forward, out raycastHit, 40))
            {
                var focusable = raycastHit.collider.GetComponent<Focusable>();
                hitCollider = raycastHit.collider;
                if (focusable != null)
                {
                    if (focusable != FocusedFocusable && FocusedFocusable != null)
                        FocusedFocusable.UnFocus();
                    focusable.Focus(raycastHit.point);
                    FocusedFocusable = focusable;
                    return;
                }
            }
            else
                HideCursors();

            ResetRaycastMemory();
        }

        private void HideBlockCursors()
        {
            highlightBlock.gameObject.SetActive(false);
            placeBlockRenderer.enabled = false;
            placeBlock.gameObject.SetActive(false);
        }

        private void HideCursors()
        {
            HideBlockCursors();
            if (MetaBlockPlaceHolder != null)
                MetaBlockPlaceHolder.gameObject.SetActive(false);
            PreparedMetaBlock?.SetActive(false);
        }

        public List<Land> GetOwnedLands()
        {
            return ownedLands;
        }

        public void ResetLands()
        {
            List<Land> lands = null;
            if (Settings.WalletId() != null)
            {
                var service = WorldService.INSTANCE;
                lands = service.GetPlayerLands();
                service.RefreshChangedLands(lands);
            }

            ownedLands = lands != null ? lands : new List<Land>();
        }

        private void ToggleViewMode()
        {
            var isNowFirstPerson = viewMode == ViewMode.FIRST_PERSON;
            viewMode = isNowFirstPerson ? ViewMode.THIRD_PERSON : ViewMode.FIRST_PERSON;
            cam.localPosition = isNowFirstPerson ? thirdPersonCameraPosition : firstPersonCameraPosition;
            avatarController.SetAvatarBodyDisabled(!isNowFirstPerson);
            viewModeChanged.Invoke(viewMode);
        }

        public ViewMode GetViewMode()
        {
            return viewMode;
        }

        public void PlaceCursors(Vector3 blockHitPoint) 
        {
            if (ChangeForbidden) return;
            var epsilon = cam.forward * CastStep;
            PossiblePlaceBlockPosInt = Vectors.FloorToInt(blockHitPoint - epsilon);
            PossibleHighlightBlockPosInt = Vectors.FloorToInt(blockHitPoint + epsilon);
            PossiblePlaceMetaBlockPos = new MetaPosition(blockHitPoint).ToWorld();

            var selectionActive = World.INSTANCE.SelectionActive;
            var selectionDisplaced = World.INSTANCE.SelectionDisplaced;

            HideCursors();
            if (BlockSelectionController.INSTANCE.Dragging) return;
            if (!CtrlDown && HammerMode && CanEdit(PossibleHighlightBlockPosInt, out _) && !selectionActive && FocusedFocusable is ChunkFocusable)
            {
                highlightBlock.position = PossibleHighlightBlockPosInt;
                highlightBlock.gameObject.SetActive(true);
            }
            else if (!CtrlDown && !selectionActive && PreparedMetaBlock != null &&
                     CanEdit(PossibleHighlightBlockPosInt, out placeLand))
            {
                PreparedMetaBlock.SetActive(true);
                if (PreparedMetaBlock.blockObject == null)
                {
                    var pos = PreparedMetaBlock.type.GetPlaceHolderPutPosition(blockHitPoint).ToWorld();
                    PreparedMetaBlock.RenderAt(null, pos, null);
                }
                else
                    PreparedMetaBlock.UpdateWorldPosition(new MetaPosition(blockHitPoint).ToWorld());
            }
            else if (!CtrlDown && !selectionActive && SelectedBlockType is MetaBlockType metaBlockType &&
                     CanEdit(PossibleHighlightBlockPosInt, out placeLand))
            {
                if (MetaBlockPlaceHolder != null)
                {
                    var mp = metaBlockType.GetPlaceHolderPutPosition(blockHitPoint);
                    var c = World.INSTANCE.GetChunkIfInited(mp.chunk);
                    if (c == null)
                        Debug.LogWarning("Null chunk!"); // should not happen
                    else if (c.GetMetaAt(mp) == null)
                    {
                        MetaBlockPlaceHolder.transform.position = mp.ToWorld();
                        MetaBlockPlaceHolder.gameObject.SetActive(true);
                    }
                }
                else
                    Debug.LogWarning("Null place holder!"); // should not happen
            }
            else
            {
                if (FocusedFocusable is ChunkFocusable && CanEdit(PossibleHighlightBlockPosInt, out highlightLand) && !selectionDisplaced)
                {
                    highlightBlock.position = PossibleHighlightBlockPosInt;
                    highlightBlock.gameObject.SetActive(true);
                }

                var currVox = Vectors.FloorToInt(GetPosition());
                if (PossiblePlaceBlockPosInt != currVox &&
                    PossiblePlaceBlockPosInt != currVox + Vector3Int.up &&
                    CanEdit(PossiblePlaceBlockPosInt, out placeLand))
                {
                    placeBlock.position = PossiblePlaceBlockPosInt;
                    placeBlock.gameObject.SetActive(true);

                    if (!CtrlDown && !selectionActive && SelectedBlockType != null && SelectedBlockType is not MetaBlockType)
                        placeBlockRenderer.enabled = true;
                }
            }
        }

        public bool CanEdit(Vector3Int blockPos, out Land land, bool isMeta = false)
        {
            if (Settings.IsGuest())
            {
                land = null;
                return false;
            }

            if (!isMeta && (playerPos.Equals(blockPos) ||
                            // playerPos.Equals(blockPos + Vector3Int.up) ||
                            playerPos.Equals(blockPos - Vector3Int.up)))
            {
                land = null;
                return false;
            }


            land = FindOwnedLand(blockPos);
            return land != null && !land.isNft;
        }

        private Land FindOwnedLand(Vector3Int position)
        {
            if (highlightLand != null && highlightLand.Contains(position))
                return highlightLand;
            if (placeLand != null && placeLand.Contains(position))
                return placeLand;
            foreach (var land in ownedLands)
                if (land.Contains(position))
                    return land;
            return null;
        }

        private VoxelPosition ComputePosition()
        {
            return new VoxelPosition(playerPos);
        }

        public Vector3 GetCurrentPosition()
        {
            return characterController.center;
        }

        public static Vector3? GetSavedPosition()
        {
            var str = PlayerPrefs.GetString(POSITION_KEY);
            return string.IsNullOrWhiteSpace(str)
                ? (Vector3?) null
                : JsonConvert.DeserializeObject<SerializableVector3>(str).ToVector3();
        }

        private IEnumerator SavePosition()
        {
            while (true)
            {
                if (GameManager.INSTANCE.GetState() == GameManager.State.PLAYING)
                {
                    PlayerPrefs.SetString(POSITION_KEY,
                        JsonConvert.SerializeObject(new SerializableVector3(GetPosition())));
                }

                yield return new WaitForSeconds(5);
            }
        }


        public bool RemoveHighlightMesh()
        {
            if (focusHighlight == null) return false;
            DestroyImmediate(focusHighlight.gameObject);
            focusHighlight = null;
            return true;
        }

        public void SetPosition(Vector3 pos)
        {
            avatarController.SetPosition(pos);
        }

        public Vector3 GetPosition()
        {
            return avatarController.GetPosition();
        }

        public bool PluginWriteAllowed(out string warnMsg)
        {
            if (viewMode != ViewMode.FIRST_PERSON)
            {
                warnMsg = "executing this plugin is only permitted in first person view mode";
                return false;
            }

            warnMsg = null;
            return true;
        }

        public enum ViewMode
        {
            FIRST_PERSON,
            THIRD_PERSON,
        }

        public static Player INSTANCE => GameObject.Find("Player").GetComponent<Player>();

        private void OnSelectedAssetChanged(SlotInfo slotInfo)
        {
            ResetRaycastMemory();
            HideCursors();
            SelectedBlockType = slotInfo?.block;
            HammerMode = false;
            world.UpdateHighlightBlockColor(false);
            PreparedMetaBlock?.DestroyView();
            PreparedMetaBlock = null;
            if (ChangeForbidden) return; // TODO ?

            var glbUrl = slotInfo?.asset?.glbUrl;
            if (glbUrl != null)
            {
                PreparedMetaBlock = new MetaBlock(Blocks.TdObject, null, new TdObjectBlockProperties
                {
                    url = glbUrl,
                    type = TdObjectBlockProperties.TdObjectType.GLB
                }, true);
                return;
            }

            if (slotInfo is {asset: null, block: null})
            {
                HammerMode = true;
                world.UpdateHighlightBlockColor(true);
                return;
            }

            if (SelectedBlockType is MetaBlockType metaBlockType)
                MetaBlockPlaceHolder = metaBlockType.GetPlaceHolder();
        }

        private void ResetRaycastMemory()
        {
            if (FocusedFocusable != null)
                FocusedFocusable.UnFocus();
            FocusedFocusable = null;
            hitCollider = null;
        }

        public void InitOnSelectedAssetChanged()
        {
            Ui.AssetsInventory.AssetsInventory.INSTANCE.selectedSlotChanged.AddListener(OnSelectedAssetChanged);
        }
    }
}