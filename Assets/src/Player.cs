using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using src.Canvas;
using src.MetaBlocks;
using src.Model;
using src.Service;
using src.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace src
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

        [NonSerialized] public uint selectedBlockId = 1;

        private bool sprinting;
        private Vector3 velocity = Vector3.zero;
        private Land highlightLand;
        public Land placeLand;
        private bool jumpRequest;
        private bool floating = false;
        private Vector3Int? lastChunk;
        private List<Land> ownedLands = new List<Land>();
        private RaycastHit raycastHit;
        private Collider hitCollider;
        private CharacterController characterController;
        private BlockSelectionController blockSelectionController;
        private bool ctrlDown = false;
        private Vector3Int playerPos;
        [NonSerialized] public GameObject avatar;
        private AvatarController avatarController;

        public Transform tdObjectHighlightMesh;

        public GameObject MetaBlockPlaceHolder { private set; get; }
        public bool HammerMode { get; private set; } = false;
        public Transform HighlightBlock => highlightBlock;
        public Transform PlaceBlock => placeBlock;

        public bool ChangeForbidden => Settings.IsGuest() || viewMode != ViewMode.FIRST_PERSON;

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
        public Vector3Int PossibleHighlightBlockPosInt { get; set; }

        public Land HighlightLand => highlightLand;

        public Transform transform => avatar?.transform; // TODO

        private void Start()
        {
            blockSelectionController = GetComponent<BlockSelectionController>();

            Snack.INSTANCE.ShowObject("Owner", null);

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING)
                    hitCollider = null;
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
                    HideCursorBlocksAndPlaceHolder();
                    if (FocusedFocusable != null)
                    {
                        FocusedFocusable.UnFocus();
                        FocusedFocusable = null;
                    }
                }

                hitCollider = null;
            });
        }


        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            GetInputs();
            if (!ChangeForbidden)
                blockSelectionController.DoUpdate();

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
            ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                       Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
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
            if (!ChangeForbidden)
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
            if (Physics.Raycast(cam.position, cam.forward, out raycastHit, 20))
            {
                var focusable = raycastHit.collider.GetComponent<Focusable>();
                if (hitCollider == raycastHit.collider &&
                    focusable != null && focusable is MetaFocusable) return;
                hitCollider = raycastHit.collider;
                if (focusable != null)
                {
                    if (FocusedFocusable != null)
                        FocusedFocusable.UnFocus();
                    focusable.Focus(raycastHit.point);
                    FocusedFocusable = focusable;
                    if (focusable is MetaFocusable metaFocusable)
                        HideCursorBlocksAndPlaceHolder();
                    return;
                }
            }
            else
                HideCursorBlocksAndPlaceHolder();

            if (FocusedFocusable != null)
                FocusedFocusable.UnFocus();
            FocusedFocusable = null;
            hitCollider = null;
        }

        private void HideCursorBlocksAndPlaceHolder()
        {
            highlightBlock.gameObject.SetActive(false);
            placeBlock.gameObject.SetActive(false);
            if (MetaBlockPlaceHolder != null)
                MetaBlockPlaceHolder.gameObject.SetActive(false);
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

        public void ToolbarSelectedChanged(bool hammerSelected)
        {
            if (ChangeForbidden) return;
            if (HammerMode == false && hammerSelected)
            {
                HammerMode = true;
                placeBlock.gameObject.SetActive(false);
                if (MetaBlockPlaceHolder != null)
                    MetaBlockPlaceHolder.gameObject.SetActive(false);
                return;
            }

            HammerMode = hammerSelected;

            if (Blocks.GetBlockType(selectedBlockId) is MetaBlockType type)
            {
                HideCursorBlocksAndPlaceHolder();
                MetaBlockPlaceHolder = type.GetPlaceHolder();
                if (MetaBlockPlaceHolder != null)
                    MetaBlockPlaceHolder.SetActive(true);
                return;
            }

            if (MetaBlockPlaceHolder != null)
                MetaBlockPlaceHolder.SetActive(false);
            placeBlock.gameObject.SetActive(true);
        }

        public void PlaceCursorBlocks(Vector3 blockHitPoint, Chunk chunk)
        {
            var epsilon = cam.forward * CastStep;
            PossiblePlaceBlockPosInt = Vectors.FloorToInt(blockHitPoint - epsilon);
            PossibleHighlightBlockPosInt = Vectors.FloorToInt(blockHitPoint + epsilon);

            if (BlockSelectionController.INSTANCE.selectionMode == BlockSelectionController.SelectionMode.Dragged &&
                World.INSTANCE.SelectionActive && CanEdit(PossiblePlaceBlockPosInt, out _))
            {
                HideCursorBlocksAndPlaceHolder();
                World.INSTANCE.MoveSelection(PossiblePlaceBlockPosInt, false);
            }
            else if (HammerMode && CanEdit(PossibleHighlightBlockPosInt, out _))
            {
                highlightBlock.position = PossibleHighlightBlockPosInt;
                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(false);
                if (MetaBlockPlaceHolder != null)
                    MetaBlockPlaceHolder.SetActive(false);
            }
            else if (Blocks.GetBlockType(selectedBlockId) is MetaBlockType metaBlockType &&
                     CanEdit(PossibleHighlightBlockPosInt, out placeLand))
            {
                placeBlock.gameObject.SetActive(false);
                highlightBlock.gameObject.SetActive(false);

                if (MetaBlockPlaceHolder != null)
                {
                    var mp = metaBlockType.GetPutPosition(blockHitPoint);
                    if (chunk.GetMetaAt(mp) == null)
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
                if (CanEdit(PossibleHighlightBlockPosInt, out highlightLand))
                {
                    highlightBlock.position = PossibleHighlightBlockPosInt;
                    highlightBlock.gameObject.SetActive(true);
                }
                else
                    highlightBlock.gameObject.SetActive(false);

                var currVox = Vectors.FloorToInt(GetPosition());
                if (PossiblePlaceBlockPosInt != currVox && PossiblePlaceBlockPosInt != currVox + Vector3Int.up &&
                    CanEdit(PossiblePlaceBlockPosInt, out placeLand))
                {
                    placeBlock.position = PossiblePlaceBlockPosInt;
                    placeBlock.gameObject.SetActive(true);
                }
                else
                    placeBlock.gameObject.SetActive(false);

                if(MetaBlockPlaceHolder != null)
                    MetaBlockPlaceHolder.gameObject.SetActive(false);
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
            if (tdObjectHighlightMesh == null) return false;
            DestroyImmediate(tdObjectHighlightMesh.gameObject);
            tdObjectHighlightMesh = null;
            return true;
        }

        public void SetPosition(Vector3 pos)
        {
            avatar.transform.position = pos;
        }

        public Vector3 GetPosition()
        {
            return avatar.transform.position;
        }

        public enum ViewMode
        {
            FIRST_PERSON,
            THIRD_PERSON,
        }

        public static Player INSTANCE => GameObject.Find("Player").GetComponent<Player>();
    }
}