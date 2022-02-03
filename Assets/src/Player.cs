using System;
using System.Collections.Generic;
using System.Linq;
using src.Canvas;
using src.MetaBlocks;
using src.MetaBlocks.TdObjectBlock;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src
{
    public class Player : MonoBehaviour
    {
        public static readonly Vector3Int ViewDistance = new Vector3Int(5, 5, 5);

        private bool sprinting;

        public Transform cam;
        public World world;

        public float walkSpeed = 6f;
        public float sprintSpeed = 12f;
        public float jumpHeight = 1;
        public float gravity = -9.8f;

        private float horizontal;
        private float vertical;
        private Vector3 velocity = Vector3.zero;
        private Land highlightLand;
        private Land placeLand;
        private bool jumpRequest;
        private bool floating = false;

        private Vector3Int lastChunk;

        private List<Land> ownedLands = new List<Land>();
        private List<SelectableBlock> selectedBlocks = new List<SelectableBlock>();
        private List<SelectableBlock> copiedBlocks = new List<SelectableBlock>();

        public Transform highlightBlock;
        public Transform placeBlock;
        public Transform tdObjectHighlightBox;
        private MetaBlock focusedMetaBlock;
        private Voxels.Face focusedMetaFace;
        private RaycastHit raycastHit;
        private MetaFocusable focusedMeta;
        private Collider hitCollider;
        private CharacterController controller;

        public float castStep = 0.01f;
        public byte selectedBlockId = 1;

        private bool selectionActive = false; // whether we have any selected any blocks or not

        private bool
            movingSelectionAllowed =
                false; // whether we can move the selections using arrow keys and space (only mean sth when selectionActive = true) 

        private SnackItem snackItem;
        private Vector3Int placeBlockPosInt;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            Snack.INSTANCE.ShowObject("Owner", null);

            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state == GameManager.State.PLAYING)
                {
                    hitCollider = null;
                    ExitBlockSelectionMovement();
                }
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
                var service = VoxelService.INSTANCE;
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
            if (selectionActive && movingSelectionAllowed) return;
            var moveDirection = transform.forward * vertical + transform.right * horizontal;
            controller.Move(moveDirection * (sprinting ? sprintSpeed : walkSpeed) * Time.fixedDeltaTime);

            if (controller.isGrounded && velocity.y < 0 || floating)
                velocity.y = 0f;

            if (jumpRequest)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

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
                    if (focusedMeta != null)
                    {
                        focusedMeta.UnFocus();
                    }

                    if (!selectionActive)
                        metaFocusable.Focus();
                    else if(metaFocusable.metaBlockObject is TdObjectBlockObject)
                    {
                        ShowTdObjectHighlightBox(((TdObjectBlockObject) metaFocusable.metaBlockObject).TdObjectBoxCollider);
                    }
                    focusedMeta = metaFocusable;
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

            if (focusedMeta != null)
                focusedMeta.UnFocus();
            focusedMetaFace = null;
            focusedMeta = null;
            hitCollider = null;
        }

        private void Update()
        {
            if (GameManager.INSTANCE.GetState() != GameManager.State.PLAYING) return;

            GetMovementInputs();
            HandleBlockMovement();
            HandleBlockSelection();
            HandleBlockClipboard();

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

        private void GetMovementInputs()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

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

        private void HandleBlockMovement()
        {
            if (!(selectedBlocks.Count > 0 && selectionActive)) return;

            var rotateAroundX = Input.GetKeyDown(KeyCode.R) &&
                                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                                (Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl));
            var rotateAroundZ = Input.GetKeyDown(KeyCode.R) &&
                                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            var rotateAroundY = Input.GetKeyDown(KeyCode.R);
            var rotation = rotateAroundY || rotateAroundZ || rotateAroundX;
            var center = selectedBlocks[0].highlight.position + 0.5f * Vector3.one;

            Vector3 delta = default;
            if (movingSelectionAllowed)
            {
                var moveDown = Input.GetButtonDown("Jump") &&
                               (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
                var moveUp = !moveDown && Input.GetButtonDown("Jump");

                var moveDirection = Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical")
                    ? (transform.forward * vertical + transform.right * horizontal).normalized
                    : Vector3.zero;
                var movement = moveUp || moveDown || moveDirection.magnitude > castStep;

                if (!rotation && !movement) return;

                delta = Vectors.FloorToInt(center + moveDirection) - selectedBlocks[0].highlight.position +
                        (moveUp ? Vector3.up : moveDown ? Vector3.down : Vector3.zero);
            }
            else if (!rotation) return;

            foreach (var block in selectedBlocks)
            {
                if (rotateAroundX)
                    block.RotateAroundX(center);
                else if (rotateAroundZ)
                    block.RotateAroundZ(center);
                else if (rotateAroundY)
                    block.RotateAroundY(center);
                if (movingSelectionAllowed)
                    block.Move(Vectors.FloorToInt(delta));
            }
        }

        private void HandleBlockSelection()
        {
            var selectVoxel = (highlightBlock.gameObject.activeSelf || focusedMeta != null) &&
                              Input.GetMouseButtonDown(0) && (Input.GetKey(KeyCode.LeftControl) ||
                                                              Input.GetKey(KeyCode.RightControl));
            var deleteVoxel = !selectVoxel && highlightBlock.gameObject.activeSelf && Input.GetMouseButtonDown(0);

            if (selectVoxel)
            {
                var selectedBlockPosition =
                    focusedMeta == null ? highlightBlock.position : focusedMeta.GetBlockPosition();

                // Remove any existing selections
                var indicesToRemove = new List<int>();
                for (int i = 0; i < selectedBlocks.Count; i++)
                {
                    if (selectedBlocks[i].position.Equals(selectedBlockPosition))
                        indicesToRemove.Add(i);
                }

                if (indicesToRemove.Count > 0)
                {
                    foreach (var index in indicesToRemove.OrderByDescending(i => i))
                    {
                        selectedBlocks[index].DestroyHighlights();
                        selectedBlocks.RemoveAt(index);
                        if (selectedBlocks.Count == 0)
                        {
                            ExitBlockSelectionMovement();
                            break;
                        }
                    }
                }
                else AddNewSelectedBlock(selectedBlockPosition);
            }

            if (deleteVoxel)
            {
                var vp = new VoxelPosition(highlightBlock.position);
                var chunk = world.GetChunkIfInited(vp.chunk);
                if (chunk != null)
                {
                    chunk.DeleteVoxel(vp, highlightLand);
                    if (chunk.GetMetaAt(vp) != null)
                        chunk.DeleteMeta(vp);
                }
            }

            if (placeBlock.gameObject.activeSelf && Input.GetMouseButtonDown(1))
            {
                PutBlock(placeBlock.position, VoxelService.INSTANCE.GetBlockType(selectedBlockId));
            }
        }

        public void PutBlock(Vector3 pos, BlockType type)
        {
            var vp = new VoxelPosition(pos);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk != null)
            {
                if (type is MetaBlockType)
                    chunk.PutMeta(vp, type, placeLand);
                else
                    chunk.PutVoxel(vp, type, placeLand);
            }
        }

        private void AddNewSelectedBlock(Vector3 position)
        {
            if (CanEdit(Vectors.FloorToInt(position), out var land))
            {
                var selectedBlock = SelectableBlock.Create(position, world, highlightBlock, tdObjectHighlightBox, land);
                if (selectedBlock != null)
                {
                    selectedBlocks.Add(selectedBlock);
                    if (selectedBlocks.Count == 1 && !selectionActive)
                    {
                        selectionActive = true;
                        movingSelectionAllowed = false;
                        SetBlockSelectionSnack();
                    }
                }
            }
        }

        private void AddNewCopiedBlock(Vector3 position)
        {
            if (CanEdit(Vectors.FloorToInt(position), out var land))
            {
                var copiedBlock =
                    SelectableBlock.Create(position, world, highlightBlock, tdObjectHighlightBox, land, false);
                if (copiedBlock != null)
                    copiedBlocks.Add(copiedBlock);
            }
        }

        private void SetBlockSelectionSnack(bool help = false)
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(help), () =>
            {
                if (Input.GetKeyDown(KeyCode.H))
                    SetBlockSelectionSnack(!help);
                if (Input.GetKeyDown(KeyCode.V) &&
                    !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
                {
                    movingSelectionAllowed = !movingSelectionAllowed;
                    SetBlockSelectionSnack(help);
                }
            });
        }

        private List<string> GetSnackLines(bool helpMode)
        {
            var lines = new List<string>();
            if (helpMode)
            {
                if (movingSelectionAllowed)
                {
                    lines.Add("W : forward");
                    lines.Add("S : backward");
                    lines.Add("SPACE : up");
                    lines.Add("SHIFT+SPACE : down");
                    lines.Add("A : left");
                    lines.Add("D : right");
                }

                lines.Add("R : rotate around y");
                lines.Add("SHIFT+R : rotate around z");
                lines.Add("CTRL+SHIFT+R : rotate around x");
                lines.Add("H : exit help");
            }
            else
            {
                lines.Add("H : help");
            }

            lines.Add("X : cancel");
            lines.Add("ENTER : confirm movement");
            lines.Add("Del : delete selected blocks");
            lines.Add("CTRL+C/V : copy/paste selection");
            lines.Add("CTRL+CLICK : select/unselect block");
            if (movingSelectionAllowed)
                lines.Add("V : set player as movement target");
            else
                lines.Add("V : set selection as movement target");
            return lines;
        }

        private void HandleBlockClipboard()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                ExitBlockSelectionMovement();
            }
            else if (Input.GetKeyDown(KeyCode.C) &&
                     (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                copiedBlocks.Clear();
                foreach (var block in selectedBlocks)
                    AddNewCopiedBlock(block.highlight.position);
                ExitBlockSelectionMovement();
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                ConfirmMove();
                ExitBlockSelectionMovement();
            }
            else if (Input.GetButtonDown("Delete"))
            {
                DeleteSelection();
                ExitBlockSelectionMovement();
            }
            else if (placeBlock.gameObject.activeSelf && copiedBlocks.Count > 0 &&
                     Input.GetKeyDown(KeyCode.V) &&
                     (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                var minX = float.PositiveInfinity;
                var minY = float.PositiveInfinity;
                var minZ = float.PositiveInfinity;
                foreach (var srcBlock in copiedBlocks)
                {
                    if (srcBlock.position.x < minX)
                        minX = srcBlock.position.x;
                    if (srcBlock.position.y < minY)
                        minY = srcBlock.position.y;
                    if (srcBlock.position.z < minZ)
                        minZ = srcBlock.position.z;
                }

                var minPoint = new Vector3(minX, minY, minZ);
                var conflictWithPlayer = false;
                var currVox = Vectors.FloorToInt(transform.position);
                ClearSelection();
                foreach (var srcBlock in copiedBlocks)
                {
                    var newPosition = TruncateFloor(srcBlock.position - minPoint + placeBlockPosInt);
                    if (newPosition.Equals(currVox) || newPosition.Equals(currVox + Vector3Int.up)
                                                    || newPosition.Equals(currVox + 2 * Vector3Int.up))
                    {
                        conflictWithPlayer = true;
                        break;
                    }
                }

                if (!conflictWithPlayer)
                    foreach (var srcBlock in copiedBlocks)
                    {
                        var newPosition = TruncateFloor(srcBlock.position - minPoint + placeBlockPosInt);
                        if (CanEdit(newPosition, out var land))
                        {
                            srcBlock.PutInPosition(world, newPosition, land);
                            AddNewSelectedBlock(newPosition);
                        }
                    }
            }
        }

        private void ConfirmMove()
        {
            var movedBlocks = selectedBlocks.Where(block => block.IsMoved()).ToList();
            var currVox = Vectors.FloorToInt(transform.position);
            foreach (var block in movedBlocks)
            {
                var position = block.highlight.position;
                if (position.Equals(currVox) || position.Equals(currVox + Vector3Int.up)
                                             || position.Equals(currVox + 2 * Vector3Int.up))
                    return;
            }

            foreach (var block in movedBlocks)
                block.Remove(world);
            foreach (var block in movedBlocks)
            {
                var pos = TruncateFloor(block.highlight.position);
                if (Player.INSTANCE.CanEdit(pos, out var land))
                    block.PutInPosition(world, pos, land);
            }
        }

        private void DeleteSelection()
        {
            foreach (var block in selectedBlocks)
                block.Remove(world);
        }

        private void ExitBlockSelectionMovement()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            ClearSelection();
            selectionActive = false;
            movingSelectionAllowed = false;
        }

        private void ClearSelection()
        {
            foreach (var block in selectedBlocks)
                block.DestroyHighlights();
            selectedBlocks.Clear();
        }

        private void PlaceCursorBlocks(Vector3 blockHitPoint)
        {
            var epsilon = cam.forward * castStep;
            placeBlockPosInt = Vectors.FloorToInt(blockHitPoint - epsilon);

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

                if (VoxelService.INSTANCE.GetBlockType(selectedBlockId) is MetaBlockType)
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
                    if (placeBlockPosInt != currVox && placeBlockPosInt != currVox + Vector3Int.up)
                    {
                        placeBlock.position = placeBlockPosInt;
                        placeBlock.gameObject.SetActive(CanEdit(placeBlockPosInt, out placeLand));
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

                if (focusedMetaBlock != null && !selectionActive)
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
            if (blockLocalHitPoint.x < castStep) return Voxels.Face.LEFT;
            if (Math.Abs(blockLocalHitPoint.x - 1) < castStep) return Voxels.Face.RIGHT;

            if (blockLocalHitPoint.z < castStep) return Voxels.Face.BACK;
            if (Math.Abs(blockLocalHitPoint.z - 1) < castStep) return Voxels.Face.FRONT;

            if (blockLocalHitPoint.y < castStep) return Voxels.Face.BOTTOM;
            if (Math.Abs(blockLocalHitPoint.y - 1) < castStep) return Voxels.Face.TOP;

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
            if (highlightLand != null && highlightLand.Contains(ref position))
                return highlightLand;
            if (placeLand != null && placeLand.Contains(ref position))
                return placeLand;
            foreach (var land in ownedLands)
                if (land.Contains(ref position))
                    return land;
            return null;
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

        public static Player INSTANCE
        {
            get { return GameObject.Find("Player").GetComponent<Player>(); }
        }

        private static Vector3Int TruncateFloor(Vector3 vector)
        {
            vector.x = Mathf.Round(vector.x * 1000) / 1000;
            vector.y = Mathf.Round(vector.y * 1000) / 1000;
            vector.z = Mathf.Round(vector.z * 1000) / 1000;
            return Vectors.FloorToInt(vector);
        }
    }
}