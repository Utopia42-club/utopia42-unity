using System;
using System.Collections.Generic;
using src.Canvas;
using src.MetaBlocks;
using src.MetaBlocks.TdObjectBlock;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;
using MetaBlock = src.MetaBlocks.MetaBlock;

namespace src
{
    public class Player : MonoBehaviour
    {
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
        [SerializeField] private Transform tdObjectHighlightBox;

        [NonSerialized] public byte selectedBlockId = 1;

        private bool sprinting;
        private Vector3 velocity = Vector3.zero;
        private Land highlightLand;
        public Land placeLand;
        private bool jumpRequest;
        private bool floating = false;
        private Vector3Int lastChunk;
        private List<Land> ownedLands = new List<Land>();
        private MetaBlock focusedMetaBlock;
        private Voxels.Face focusedMetaFace;
        private RaycastHit raycastHit;
        private Collider hitCollider;
        private CharacterController controller;
        private BlockSelectionController blockSelectionController;

        public Transform HighlightBlock => highlightBlock;
        public Transform PlaceBlock => placeBlock;
        public Transform TdObjectHighlightBox => tdObjectHighlightBox;

        public Player(Transform tdObjectHighlightBox, Transform placeBlock, Transform highlightBlock)
        {
            this.tdObjectHighlightBox = tdObjectHighlightBox;
            this.placeBlock = placeBlock;
            this.highlightBlock = highlightBlock;
        }

        public float Horizontal { get; private set; }
        public float Vertical { get; private set; }
        public MetaFocusable FocusedMeta { get; private set; }
        public Vector3Int PlaceBlockPosInt { get; private set; }

        public Land HighlightLand => highlightLand;
        public Land PlaceLand => placeLand;


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
        }

        public List<Land> GetOwnedLands()
        {
            return ownedLands;
        }

        public void ResetLands()
        {
            List<Land> lands = null;
            string wallet = Settings.WalletId();
            if (wallet != null)
            {
                var service = WorldService.INSTANCE;
                lands = service.GetLandsFor(wallet);
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
        }

        private void DetectFocus()
        {
            if (Physics.Raycast(cam.position, cam.forward, out raycastHit, 20))
            {
                PlaceCursorBlocks(raycastHit.point);
                if (hitCollider == raycastHit.collider) return;
                hitCollider = raycastHit.collider;
                var metaFocusable = hitCollider.GetComponent<MetaFocusable>();
                if (metaFocusable != null)
                {
                    focusedMetaFace = null;
                    if (FocusedMeta != null)
                    {
                        FocusedMeta.UnFocus();
                    }

                    if (!blockSelectionController.SelectionActive)
                        metaFocusable.Focus();
                    else if (metaFocusable.metaBlockObject is TdObjectBlockObject)
                    {
                        ShowTdObjectHighlightBox(((TdObjectBlockObject) metaFocusable.metaBlockObject)
                            .TdObjectBoxCollider);
                    }

                    FocusedMeta = metaFocusable;
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

            if (FocusedMeta != null)
                FocusedMeta.UnFocus();
            focusedMetaFace = null;
            FocusedMeta = null;
            hitCollider = null;
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;
            GetMovementInputs();
            // GetTestInputs();
            blockSelectionController.DoUpdate();

            if (lastChunk == null)
            {
                lastChunk = ComputePosition().chunk;
                world.OnPlayerChunkChanged(lastChunk);
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

        private void GetTestInputs()
        {
            var onGround = Input.GetKey(KeyCode.LeftShift);
            if (!Input.GetKeyDown(KeyCode.T) ||
                !onGround && !Input.GetKey(KeyCode.RightShift)) return;
            Debug.Log("Putting 2000 blocks " + (onGround ? "on ground" : "out of reach"));
            var type = WorldService.INSTANCE.GetBlockType("stone");
            var from = Vectors.TruncateFloor(transform.position) + (onGround ? 3 : 250) * Vector3.up;
            var to = @from + new Vector3(10, 20, 10);

            var toBePut = new Dictionary<Vector3, BlockType>();
            for (var x = @from.x; x <= to.x; x++)
            for (var y = @from.y; y <= to.y; y++)
            for (var z = @from.z; z <= to.z; z++)
                toBePut.Add(new Vector3(x, y, z), type);
            ApiPutBlocks(toBePut);
        }

        private void GetMovementInputs()
        {
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

        private void PlaceCursorBlocks(Vector3 blockHitPoint)
        {
            var epsilon = cam.forward * CastStep;
            PlaceBlockPosInt = Vectors.FloorToInt(blockHitPoint - epsilon);

            var posInt = Vectors.FloorToInt(blockHitPoint + epsilon);
            var vp = new VoxelPosition(posInt);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            var metaToFocus = chunk.GetMetaAt(vp);
            var foundSolid = chunk.GetBlock(vp.local).isSolid;

            if (foundSolid)
            {
                highlightBlock.position = posInt;
                highlightBlock.gameObject.SetActive(CanEdit(posInt, out highlightLand));

                if (WorldService.INSTANCE.GetBlockType(selectedBlockId) is MetaBlockType)
                {
                    if (chunk.GetMetaAt(vp) == null)
                    {
                        placeBlock.position = posInt;
                        placeBlock.gameObject.SetActive(CanEdit(posInt, out placeLand));
                    }
                    else
                        placeBlock.gameObject.SetActive(false);
                }
                else
                {
                    var currVox = Vectors.FloorToInt(transform.position);
                    if (PlaceBlockPosInt != currVox && PlaceBlockPosInt != currVox + Vector3Int.up)
                    {
                        placeBlock.position = PlaceBlockPosInt;
                        placeBlock.gameObject.SetActive(CanEdit(PlaceBlockPosInt, out placeLand));
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

                if (focusedMetaBlock != null && !blockSelectionController.SelectionActive)
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

        public bool CanEdit(Vector3Int position, out Land land)
        {
            if (Settings.IsGuest())
            {
                land = null;
                return true;
            }

            land = FindOwnedLand(position);
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

        public void PutBlock(Vector3 pos, BlockType type)
        {
            var vp = new VoxelPosition(pos);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            if (type is MetaBlockType)
                chunk.PutMeta(vp, type, placeLand);
            else
                chunk.PutVoxel(vp, type, placeLand);
        }

        public bool ApiPutBlock(Vector3 vector3, BlockType getBlockType)
        {
            return ApiPutBlocks(new Dictionary<Vector3, BlockType> {{vector3, getBlockType}})[vector3];
        }

        public Dictionary<Vector3, bool> ApiPutBlocks(Dictionary<Vector3, BlockType> blocks)
        {
            var result = new Dictionary<Vector3, bool>();
            var toBePut = new Dictionary<VoxelPosition, Tuple<BlockType, Land>>();
            foreach (var pos in blocks.Keys)
            {
                var vp = new VoxelPosition(pos);
                var type = blocks[pos];
                var playerPos = Vectors.TruncateFloor(transform.position);
                var blockPos = vp.ToWorld();
                if (type is MetaBlockType || playerPos.Equals(blockPos) || playerPos.Equals(blockPos + Vector3Int.up) ||
                    playerPos.Equals(blockPos - Vector3Int.up))
                {
                    result.Add(pos, false);
                    continue;
                }

                if (CanEdit(Vectors.FloorToInt(pos), out var ownerLand))
                {
                    toBePut.Add(vp, new Tuple<BlockType, Land>(type, ownerLand));
                    result.Add(pos, true);
                    continue;
                }

                result.Add(pos, false);
            }

            world.PutBlocks(toBePut);

            return result;
        }

        private VoxelPosition ComputePosition()
        {
            return new VoxelPosition(transform.position);
        }

        public void ShowTdObjectHighlightBox(BoxCollider boxCollider)
        {
            var colliderTransform = boxCollider.transform;
            tdObjectHighlightBox.transform.rotation = colliderTransform.rotation;

            var size = boxCollider.size;
            var minPos = boxCollider.center - size / 2;

            var gameObjectTransform = boxCollider.gameObject.transform;
            size.Scale(gameObjectTransform.localScale);
            size.Scale(gameObjectTransform.parent.localScale);

            tdObjectHighlightBox.localScale = size;
            tdObjectHighlightBox.position = colliderTransform.TransformPoint(minPos);
            tdObjectHighlightBox.gameObject.SetActive(true);
        }

        public void HideTdObjectHighlightBox()
        {
            tdObjectHighlightBox.gameObject.SetActive(false);
        }

        public Vector3 GetCurrentPosition()
        {
            return controller.center;
        }

        public static Player INSTANCE => GameObject.Find("Player").GetComponent<Player>();
    }
}