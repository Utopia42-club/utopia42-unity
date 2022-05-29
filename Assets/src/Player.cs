using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using src.Canvas;
using src.MetaBlocks;
using src.Model;
using src.Service;
using src.Utils;
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
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float sprintSpeed = 12f;
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
        private MetaBlock focusedMetaBlock;
        private Voxels.Face focusedMetaFace;
        private RaycastHit raycastHit;
        private Collider hitCollider;
        private CharacterController characterController;
        private BlockSelectionController blockSelectionController;
        private bool ctrlDown = false;
        private Vector3Int playerPos;
        [NonSerialized] public GameObject avatar;
        private AvatarController avatarController;

        public Transform tdObjectHighlightMesh;

        public bool HammerMode { get; private set; } = false;
        public Transform HighlightBlock => highlightBlock;
        public Transform PlaceBlock => placeBlock;

        public Vector3 firstPersonCameraPosition;
        public Vector3 thirdPersonCameraPosition;
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
        public Focusable focused { get; private set; }
        public Vector3Int PlaceBlockPosInt { get; private set; }

        public Land HighlightLand => highlightLand;

        private void Start()
        {
            blockSelectionController = GetComponent<BlockSelectionController>();

            Snack.INSTANCE.ShowObject("Owner", null);

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING)
                    hitCollider = null;
            });

            avatar = Instantiate(avatarPrefab, transform);
            avatarController = avatar.GetComponent<AvatarController>();
            characterController = avatar.GetComponent<CharacterController>();
            cam.SetParent(avatar.transform);

            playerPos = Vectors.TruncateFloor(GetPosition());
            StartCoroutine(SavePosition());

            viewModeChanged = new UnityEvent<ViewMode>();
            ToggleViewMode();
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


        private void DetectFocus()
        {
            if (viewMode == ViewMode.FIRST_PERSON
                && Physics.Raycast(cam.position, cam.forward, out raycastHit, 20))
            {
                if (hitCollider == raycastHit.collider &&
                    hitCollider.TryGetComponent(typeof(MetaFocusable), out _)) return;
                hitCollider = raycastHit.collider;
                var focusable = hitCollider.GetComponent<Focusable>();
                if (focusable != null)
                {
                    focusedMetaFace = null;
                    if (focused != null)
                        focused.UnFocus();
                    focusable.Focus(raycastHit.point);
                    focused = focusable;
                    return;
                }
            }
            else
            {
                highlightBlock.gameObject.SetActive(false);
                placeBlock.gameObject.SetActive(false);
                if (focusedMetaBlock != null)
                    focusedMetaBlock.UnFocus();
            }

            if (focused != null)
                focused.UnFocus();
            focusedMetaFace = null;
            focused = null;
            hitCollider = null;
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            GetInputs();
            if (viewMode == ViewMode.FIRST_PERSON)
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

        private void ToggleViewMode()
        {
            viewMode = viewMode == ViewMode.FIRST_PERSON ? ViewMode.THIRD_PERSON : ViewMode.FIRST_PERSON;
            cam.localPosition = viewMode == ViewMode.FIRST_PERSON
                ? firstPersonCameraPosition
                : thirdPersonCameraPosition;

            viewModeChanged.Invoke(viewMode);
        }

        public void SetHammerActive(bool active)
        {
            if (HammerMode == active) return;
            HammerMode = active;
            placeBlock.gameObject.SetActive(!active);
        }

        public void PlaceCursorBlocks(Vector3 blockHitPoint, Chunk chunk)
        {
            var epsilon = cam.forward * CastStep;
            PlaceBlockPosInt = Vectors.FloorToInt(blockHitPoint - epsilon);
            var posInt = Vectors.FloorToInt(blockHitPoint + epsilon);
            var vp = new VoxelPosition(posInt);
            // var chunk = world.GetChunkIfInited(vp.chunk);
            // if (chunk == null) return;
            var metaToFocus = chunk.GetMetaAt(vp);
            var foundSolid = chunk.GetBlock(vp.local).isSolid;

            if (foundSolid)
            {
                highlightBlock.position = posInt;
                highlightBlock.gameObject.SetActive(CanEdit(posInt, out highlightLand));

                if (Blocks.GetBlockType(selectedBlockId) is MetaBlockType && !HammerMode)
                {
                    if (chunk.GetMetaAt(vp) == null)
                    {
                        placeBlock.position = posInt;
                        placeBlock.gameObject.SetActive((!HammerMode || ctrlDown) && CanEdit(posInt, out placeLand));
                    }
                    else
                        placeBlock.gameObject.SetActive(false);
                }
                else
                {
                    var currVox = Vectors.FloorToInt(GetPosition());
                    if (PlaceBlockPosInt != currVox && PlaceBlockPosInt != currVox + Vector3Int.up)
                    {
                        placeBlock.position = PlaceBlockPosInt;
                        placeBlock.gameObject.SetActive((!HammerMode || ctrlDown) &&
                                                        CanEdit(PlaceBlockPosInt, out placeLand));
                    }
                    else
                        placeBlock.gameObject.SetActive(false);
                }
            }
            else
            {
                highlightBlock.gameObject.SetActive(false);
                placeBlock.gameObject.SetActive(false);
            }

            Voxels.Face faceToFocus = null;
            if (metaToFocus != null)
            {
                if (!metaToFocus.IsPositioned()) metaToFocus = null;
                else
                {
                    faceToFocus = FindFocusedFace(blockHitPoint - posInt);
                    if (faceToFocus == null) metaToFocus = null;
                }
            }

            if (focusedMetaBlock != metaToFocus || faceToFocus != focusedMetaFace)
            {
                if (focusedMetaBlock != null)
                    focusedMetaBlock.UnFocus();
                focusedMetaBlock = metaToFocus;
                focusedMetaFace = faceToFocus;

                if (focusedMetaBlock != null && !World.INSTANCE.SelectionActive)
                {
                    if (!focusedMetaBlock.Focus(focusedMetaFace))
                    {
                        focusedMetaBlock = null;
                        focusedMetaFace = null;
                    }
                }
            }
        }


        private Voxels.Face FindFocusedFace(Vector3 blockLocalHitPoint)
        {
            if (blockLocalHitPoint.x < CastStep) return Voxels.Face.LEFT;
            if (Math.Abs(blockLocalHitPoint.x - 1) < CastStep) return Voxels.Face.RIGHT;

            if (blockLocalHitPoint.z < CastStep) return Voxels.Face.BACK;
            if (Math.Abs(blockLocalHitPoint.z - 1) < CastStep) return Voxels.Face.FRONT;

            if (blockLocalHitPoint.y < CastStep) return Voxels.Face.BOTTOM;
            if (Math.Abs(blockLocalHitPoint.y - 1) < CastStep) return Voxels.Face.TOP;

            return null;
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