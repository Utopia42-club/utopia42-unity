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

namespace src
{
    public class Player : MonoBehaviour
    {
        private static readonly string POSITION_KEY = "PLAYER_POSITION";

        public const float CastStep = 0.01f;
        public static readonly Vector3Int ViewDistance = new Vector3Int(5, 5, 5);

        [SerializeField] private Transform cam;
        [SerializeField] private World world;
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float sprintSpeed = 12f;
        [SerializeField] private float jumpHeight = 2;
        [SerializeField] private float sprintJumpHeight = 2.5f;
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private Transform highlightBlock;
        [SerializeField] private Transform placeBlock;
        [SerializeField] public Transform tdObjectHighlightBox;

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
        private CharacterController controller;
        private BlockSelectionController blockSelectionController;
        private bool ctrlDown = false;
        private Vector3Int playerPos;
        public Transform tdObjectHighlightMesh;

        public GameObject MetaBlockPlaceHolder { private set; get; }
        public bool HammerMode { get; private set; } = false;
        public Transform HighlightBlock => highlightBlock;
        public Transform PlaceBlock => placeBlock;

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

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            blockSelectionController = GetComponent<BlockSelectionController>();

            Snack.INSTANCE.ShowObject("Owner", null);

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING)
                    hitCollider = null;
            });

            playerPos = Vectors.TruncateFloor(transform.position);
            StartCoroutine(SavePosition());
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

            this.ownedLands = lands != null ? lands : new List<Land>();
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
            var moveDirection = transform.forward * Vertical + transform.right * Horizontal;
            controller.Move(moveDirection * (sprinting ? sprintSpeed : walkSpeed) * Time.fixedDeltaTime);

            if (controller.isGrounded && velocity.y < 0 || floating)
                velocity.y = 0f;

            if (jumpRequest)
                velocity.y = Mathf.Sqrt((sprinting ? sprintJumpHeight : jumpHeight) * -2f * gravity);

            if (!floating && !controller.isGrounded)
                velocity.y += gravity * Time.fixedDeltaTime;

            controller.Move(velocity * Time.fixedDeltaTime);
            if ((controller.collisionFlags & CollisionFlags.Above) != 0)
                velocity.y = 0;

            playerPos = Vectors.TruncateFloor(transform.position);
        }

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

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            GetInputs();
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
        }

        public void ToolbarSelectedChanged(bool hammerSelected)
        {
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

        public void PlaceCursorBlocks(Vector3 blockHitPoint, Chunk chunk) // TODO [detach metablock]: better name?
        {
            var epsilon = cam.forward * CastStep;
            PossiblePlaceBlockPosInt = Vectors.FloorToInt(blockHitPoint - epsilon);
            PossibleHighlightBlockPosInt = Vectors.FloorToInt(blockHitPoint + epsilon);

            if (BlockSelectionController.INSTANCE.selectionMode == BlockSelectionController.SelectionMode.Dragged &&
                World.INSTANCE.SelectionActive)
            {
                HideCursorBlocksAndPlaceHolder();

                if (CanEdit(PossiblePlaceBlockPosInt, out _))
                    World.INSTANCE.MoveSelection(PossiblePlaceBlockPosInt, false);
            }
            else if (HammerMode)
            {
                highlightBlock.position = PossibleHighlightBlockPosInt;
                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(false);
                if (MetaBlockPlaceHolder != null)
                    MetaBlockPlaceHolder.SetActive(false);
            }
            else if (Blocks.GetBlockType(selectedBlockId) is MetaBlockType metaBlockType)
            {
                placeBlock.gameObject.SetActive(false);
                highlightBlock.gameObject.SetActive(false);

                if (MetaBlockPlaceHolder != null)
                {
                    var mp = metaBlockType.GetPutPosition(blockHitPoint);
                    if (chunk.GetMetaAt(mp) == null && CanEdit(PossibleHighlightBlockPosInt, out placeLand))
                    {
                        MetaBlockPlaceHolder.transform.position = mp.ToWorld();
                        MetaBlockPlaceHolder.gameObject.SetActive(true);
                    }
                    else
                        MetaBlockPlaceHolder.gameObject.SetActive(false);
                }
                else
                    Debug.LogWarning("Null place holder!"); // should not happen
            }
            else
            {
                highlightBlock.position = PossibleHighlightBlockPosInt;
                highlightBlock.gameObject.SetActive(CanEdit(PossibleHighlightBlockPosInt, out highlightLand));
                var currVox = Vectors.FloorToInt(transform.position);
                if (PossiblePlaceBlockPosInt != currVox && PossiblePlaceBlockPosInt != currVox + Vector3Int.up)
                {
                    placeBlock.position = PossiblePlaceBlockPosInt;
                    placeBlock.gameObject.SetActive(CanEdit(PossiblePlaceBlockPosInt, out placeLand));
                }
                else
                    placeBlock.gameObject.SetActive(false);
            }
        }

        public bool CanEdit(Vector3Int blockPos, out Land land, bool isMeta = false)
        {
            if (!isMeta && (playerPos.Equals(blockPos) ||
                            // playerPos.Equals(blockPos + Vector3Int.up) ||
                            playerPos.Equals(blockPos - Vector3Int.up)))
            {
                land = null;
                return false;
            }

            if (Settings.IsGuest())
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
            return controller.center;
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
                        JsonConvert.SerializeObject(new SerializableVector3(transform.position)));
                }

                yield return new WaitForSeconds(5);
            }
        }

        public static Player INSTANCE => GameObject.Find("Player").GetComponent<Player>();

        public bool RemoveHighlightMesh()
        {
            if (tdObjectHighlightMesh == null) return false;
            DestroyImmediate(tdObjectHighlightMesh.gameObject);
            tdObjectHighlightMesh = null;
            return true;
        }
    }
}