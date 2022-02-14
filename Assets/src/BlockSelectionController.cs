using System;
using System.Collections.Generic;
using System.Linq;
using src.Canvas;
using src.MetaBlocks;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;

namespace src
{
    public class BlockSelectionController : MonoBehaviour
    {
        [SerializeField] private float selectionRotationSensitivity = 0.3f;

        private MouseLook mouseLook;
        private Player player;
        private World world;
        private SnackItem snackItem;

        private readonly List<SelectableBlock> selectedBlocks = new List<SelectableBlock>();
        private readonly List<SelectableBlock> copiedBlocks = new List<SelectableBlock>();

        private Vector3 rotationSum = Vector3.zero;

        public bool SelectionActive { get; private set; } = false; // whether we have any selected any blocks or not

        private bool
            movingSelectionAllowed =
                false; // whether we can move the selections using arrow keys and space (only mean sth when selectionActive = true) 

        private bool rotationMode = false;

        public bool PlayerMovementAllowed => !SelectionActive || !movingSelectionAllowed;

        public void Start()
        {
            player = Player.INSTANCE;
            mouseLook = MouseLook.INSTANCE;
            world = World.INSTANCE;
        }

        public void DoUpdate()
        {
            HandleBlockRotation();
            HandleBlockMovement();
            HandleBlockSelection();
            HandleBlockClipboard();
        }

        private void HandleBlockRotation()
        {
            if (rotationMode == Input.GetKey(KeyCode.R)) return;
            rotationMode = !rotationMode;
            if (!rotationMode)
                mouseLook.RemoveRotationTarget();
            else if (selectedBlocks.Count > 0 && SelectionActive)
            {
                mouseLook.SetRotationTarget(RotateSelection);
            }
        }

        private void RotateSelection(Vector3 rotation)
        {
            rotationSum += rotation;
            var absY = Mathf.Abs(rotationSum.y);
            var absX = Mathf.Abs(rotationSum.x);

            if (Mathf.Max(absX, absY) < selectionRotationSensitivity * 90)
                return;

            Vector3 rotationAxis = default;
            if (absY > absX)
            {
                rotationAxis = rotationSum.y > 0 ? Vector3.up : Vector3.down;
            }
            else
            {
                var right = transform.right;
                Vector3 axis = default;
                if (Mathf.Abs(right.x) > Mathf.Abs(right.z))
                    axis = right.x < 0 ? Vector3.left : Vector3.right;
                else
                    axis = right.z < 0 ? Vector3.back : Vector3.forward;

                rotationAxis = rotationSum.x > 0 ? axis : -axis;
            }

            var center = selectedBlocks[0].HighlightPosition + 0.5f * Vector3.one;

            foreach (var block in selectedBlocks)
                block.RotateAround(center, rotationAxis);

            rotationSum = Vector3.zero;
        }

        private void HandleBlockMovement()
        {
            if (selectedBlocks.Count == 0 || !SelectionActive || !movingSelectionAllowed) return;
            var moveDown = Input.GetButtonDown("Jump") &&
                           (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            var moveUp = !moveDown && Input.GetButtonDown("Jump");

            var moveDirection = Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical")
                ? (transform.forward * player.Vertical + transform.right * player.Horizontal).normalized
                : Vector3.zero;
            var movement = moveUp || moveDown || moveDirection.magnitude > Player.CastStep;

            if (!movement) return;

            var delta = Vectors.FloorToInt(
                Vectors.FloorToInt(selectedBlocks[0].HighlightPosition + 0.5f * Vector3.one + moveDirection) -
                selectedBlocks[0].HighlightPosition +
                (moveUp ? Vector3.up : moveDown ? Vector3.down : Vector3.zero));
            foreach (var block in selectedBlocks)
            {
                block.Move(delta);
            }
        }

        private void HandleBlockSelection()
        {
            if (rotationMode) return;
            var selectVoxel = (player.HighlightBlock.gameObject.activeSelf || player.FocusedMeta != null) &&
                              Input.GetMouseButtonDown(0) && (Input.GetKey(KeyCode.LeftControl) ||
                                                              Input.GetKey(KeyCode.RightControl) ||
                                                              Input.GetKey(KeyCode.LeftCommand) ||
                                                              Input.GetKey(KeyCode.RightCommand));

            var multipleSelect = selectVoxel && selectedBlocks.Count > 0 &&
                                 (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

            if (multipleSelect) // TODO: add limit
            {
                var lastSelectedPosition = selectedBlocks.Last().Position;
                var currentSelectedPosition =
                    Vectors.FloorToInt(player.FocusedMeta == null
                        ? player.HighlightBlock.position
                        : player.FocusedMeta.GetBlockPosition());

                var from = new Vector3Int(Mathf.Min(lastSelectedPosition.x, currentSelectedPosition.x),
                    Mathf.Min(lastSelectedPosition.y, currentSelectedPosition.y),
                    Mathf.Min(lastSelectedPosition.z, currentSelectedPosition.z));
                var to = new Vector3Int(Mathf.Max(lastSelectedPosition.x, currentSelectedPosition.x),
                    Mathf.Max(lastSelectedPosition.y, currentSelectedPosition.y),
                    Mathf.Max(lastSelectedPosition.z, currentSelectedPosition.z));

                for (var x = from.x; x <= to.x; x++)
                for (var y = from.y; y <= to.y; y++)
                for (var z = from.z; z <= to.z; z++)
                {
                    var position = new Vector3Int(x, y, z);
                    if (position.Equals(lastSelectedPosition) || position.Equals(currentSelectedPosition)) continue;
                    SelectBlockAtPosition(position);
                }

                SelectBlockAtPosition(currentSelectedPosition);
            }
            else if (selectVoxel)
            {
                var selectedBlockPosition =
                    Vectors.FloorToInt(player.FocusedMeta == null
                        ? player.HighlightBlock.position
                        : player.FocusedMeta.GetBlockPosition());
                SelectBlockAtPosition(selectedBlockPosition);
            }
            else if (player.HighlightBlock.gameObject.activeSelf && Input.GetMouseButtonDown(0))
            {
                var vp = new VoxelPosition(player.HighlightBlock.position);
                var chunk = world.GetChunkIfInited(vp.chunk);
                if (chunk != null)
                {
                    chunk.DeleteVoxel(vp, player.HighlightLand);
                    if (chunk.GetMetaAt(vp) != null)
                        chunk.DeleteMeta(vp);
                }
            }

            if (player.PlaceBlock.gameObject.activeSelf && Input.GetMouseButtonDown(1))
            {
                player.PutBlock(player.PlaceBlock.position, UtopiaService.INSTANCE.GetBlockType(player.selectedBlockId));
            }
        }

        private void SelectBlockAtPosition(Vector3Int position)
        {
            // Remove any existing selections
            var indicesToRemove = new List<int>();
            for (int i = 0; i < selectedBlocks.Count; i++)
            {
                if (selectedBlocks[i].Position.Equals(position))
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
            else AddNewSelectedBlock(position);
        }

        private void HandleBlockClipboard()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                ExitBlockSelectionMovement();
            }
            else if (Input.GetKeyDown(KeyCode.C) &&
                     (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                      Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)))
            {
                ClearClipboard();
                foreach (var block in selectedBlocks)
                    AddNewCopiedBlock(block.HighlightPosition);
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
            else if (player.PlaceBlock.gameObject.activeSelf && copiedBlocks.Count > 0 &&
                     Input.GetKeyDown(KeyCode.V) &&
                     (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                      Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)))
            {
                var minX = int.MaxValue;
                var minY = int.MaxValue;
                var minZ = int.MaxValue;
                foreach (var srcBlock in copiedBlocks)
                {
                    if (srcBlock.Position.x < minX)
                        minX = srcBlock.Position.x;
                    if (srcBlock.Position.y < minY)
                        minY = srcBlock.Position.y;
                    if (srcBlock.Position.z < minZ)
                        minZ = srcBlock.Position.z;
                }

                var minPoint = new Vector3Int(minX, minY, minZ);
                var conflictWithPlayer = false;
                var currVox = Vectors.TruncateFloor(transform.position);
                ClearSelection();
                foreach (var srcBlock in copiedBlocks)
                {
                    var newPosition = srcBlock.Position - minPoint + player.PlaceBlockPosInt;
                    if (currVox.Equals(newPosition) || currVox.Equals(newPosition + Vector3Int.up) ||
                        currVox.Equals(newPosition - Vector3Int.up))
                    {
                        conflictWithPlayer = true;
                        break;
                    }
                }

                if (!conflictWithPlayer)
                    foreach (var srcBlock in copiedBlocks)
                    {
                        var newPosition = srcBlock.Position - minPoint + player.PlaceBlockPosInt;
                        if (player.CanEdit(newPosition, out var land))
                        {
                            srcBlock.PutInPosition(world, newPosition, land);
                            AddNewSelectedBlock(newPosition);
                        }
                    }
            }
        }

        private void AddNewSelectedBlock(Vector3Int position)
        {
            if (player.CanEdit(position, out var land))
            {
                var selectedBlock = SelectableBlock.Create(position, world, player.HighlightBlock,
                    player.TdObjectHighlightBox, land);
                if (selectedBlock == null) return;
                selectedBlocks.Add(selectedBlock);
                if (selectedBlocks.Count == 1 && !SelectionActive)
                {
                    SelectionActive = true;
                    movingSelectionAllowed = false;
                    SetBlockSelectionSnack();
                }
            }
        }

        private void AddNewCopiedBlock(Vector3Int position)
        {
            if (player.CanEdit(position, out var land))
            {
                var copiedBlock =
                    SelectableBlock.Create(position, world, player.HighlightBlock, player.TdObjectHighlightBox, land,
                        false);
                if (copiedBlock != null)
                    copiedBlocks.Add(copiedBlock);
            }
        }

        private void ConfirmMove()
        {
            var movedBlocks = selectedBlocks.Where(block => block.IsMoved()).ToList();
            var currVox = Vectors.TruncateFloor(transform.position);
            foreach (var block in movedBlocks)
            {
                var position = block.HighlightPosition;
                if (currVox.Equals(position) || currVox.Equals(position + Vector3Int.up) ||
                    currVox.Equals(position - Vector3Int.up))
                    return;
            }

            foreach (var block in movedBlocks)
                block.Remove(world);
            foreach (var block in movedBlocks)
            {
                var pos = block.HighlightPosition;
                if (player.CanEdit(pos, out var land))
                    block.PutInPosition(world, pos, land);
            }
        }

        private void DeleteSelection()
        {
            foreach (var block in selectedBlocks)
                block.Remove(world);
        }

        private void ClearSelection()
        {
            foreach (var block in selectedBlocks)
                block.DestroyHighlights();
            selectedBlocks.Clear();
        }

        private void ClearClipboard()
        {
            foreach (var block in copiedBlocks)
                block.DestroyHighlights();
            copiedBlocks.Clear();
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
                    !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                      Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)))
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
                lines.Add("H : exit help");
                if (movingSelectionAllowed)
                {
                    lines.Add("W : forward");
                    lines.Add("S : backward");
                    lines.Add("SPACE : up");
                    lines.Add("SHIFT+SPACE : down");
                    lines.Add("A : left");
                    lines.Add("D : right");
                }

                lines.Add("CTRL+C/V : copy/paste selection");
                lines.Add("CTRL+CLICK : select/unselect block");
                lines.Add(
                    "CTRL+SHIFT+CLICK : select/unselect all blocks between the last selected block and current block");
                lines.Add("R + horizontal mouse movement : rotate around y axis");
                lines.Add("R + vertical mouse movement : rotate around player right axis");
            }
            else
            {
                lines.Add("H : help");
            }

            lines.Add("X : cancel");
            lines.Add("ENTER : confirm movement");
            lines.Add("Del : delete selected blocks");
            if (movingSelectionAllowed)
                lines.Add("V : set player as movement target");
            else
                lines.Add("V : set selection as movement target");
            return lines;
        }

        public void ExitBlockSelectionMovement()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            ClearSelection();
            SelectionActive = false;
            movingSelectionAllowed = false;
        }

        public void ReCreateTdObjectHighlightIfSelected(Vector3Int position)
        {
            foreach (var s in selectedBlocks.Where(s => s.Position.Equals(position)))
            {
                s.ReCreateTdObjectHighlight(world, player.TdObjectHighlightBox);
                return;
            }
        }

        public static BlockSelectionController INSTANCE =>
            GameObject.Find("Player").GetComponent<BlockSelectionController>();
    }
}