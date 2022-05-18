using System;
using System.Collections.Generic;
using System.Linq;
using src.Canvas;
using src.Model;
using src.Utils;
using TMPro;
using UnityEngine;

namespace src
{
    public class BlockSelectionController : MonoBehaviour
    {
        [SerializeField] private float selectionRotationSensitivity = 0.3f;
        [SerializeField] private TextMeshProUGUI selectedBlocksCountText;
        [SerializeField] private RectTransform selectedBlocksCountTextContainer;

        private MouseLook mouseLook;
        private Player player;
        private SnackItem snackItem;

        private Vector3 rotationSum = Vector3.zero;

        private bool
            movingSelectionAllowed =
                false; // whether we can move the selections using arrow keys and space (only mean sth when selectionActive = true) 

        private bool rotationMode = false;
        private bool clipboardMovement = false;

        public bool PlayerMovementAllowed => !World.INSTANCE.SelectionActive || !movingSelectionAllowed;

        public void Start()
        {
            player = Player.INSTANCE;
            mouseLook = MouseLook.INSTANCE;
            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state != GameManager.State.PLAYING && World.INSTANCE.SelectionActive)
                    ExitSelectionMode();
            });
        }

        public void DoUpdate()
        {
            if (World.INSTANCE.SelectionActive && movingSelectionAllowed)
            {
                HandleBlockRotation();
                HandleBlockMovement();
            }

            HandleBlockSelection();
            HandleBlockClipboard();
        }

        private void HandleBlockRotation()
        {
            if (!World.INSTANCE.SelectionActive || !movingSelectionAllowed) return;
            if (rotationMode == Input.GetKey(KeyCode.R)) return;
            rotationMode = !rotationMode;
            if (!rotationMode)
                mouseLook.RemoveRotationTarget();
            else if (World.INSTANCE.SelectionActive)
                mouseLook.SetRotationTarget(RotateSelection);
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

            World.INSTANCE.RotateSelection(rotationAxis);
            rotationSum = Vector3.zero;
        }

        private void HandleBlockMovement()
        {
            var moveDown = Input.GetButtonDown("Jump") &&
                           (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            var moveUp = !moveDown && Input.GetButtonDown("Jump");

            var moveDirection = Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical")
                ? (transform.forward * player.Vertical + transform.right * player.Horizontal).normalized
                : Vector3.zero;
            var movement = moveUp || moveDown || moveDirection.magnitude > Player.CastStep;

            if (!movement) return;

            var delta =
                World.INSTANCE.CalculateSelectionDisplacement(moveDirection) +
                (moveUp ? Vector3Int.up : moveDown ? Vector3Int.down : Vector3Int.zero);

            World.INSTANCE.MoveSelection(delta);
        }

        private void HandleBlockSelection()
        {
            if (rotationMode || !mouseLook.cursorLocked) return;
            var selectVoxel = !movingSelectionAllowed &&
                              (player.HighlightBlock.gameObject.activeSelf || player.focused != null) &&
                              Input.GetMouseButtonDown(0) && (Input.GetKey(KeyCode.LeftControl) ||
                                                              Input.GetKey(KeyCode.RightControl) ||
                                                              Input.GetKey(KeyCode.LeftCommand) ||
                                                              Input.GetKey(KeyCode.RightCommand));

            var multipleSelect = selectVoxel && World.INSTANCE.SelectionActive &&
                                 (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

            if (multipleSelect)
            {
                var lastSelectedPosition = World.INSTANCE.lastSelectedPosition.ToWorld();
                var currentSelectedPosition =
                    Vectors.FloorToInt(player.focused == null
                        ? player.HighlightBlock.position
                        : player.focused.GetBlockPosition());

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
                    World.INSTANCE.AddHighlight(new VoxelPosition(position), true);
                }
                AddHighlight(currentSelectedPosition); // TODO: refactor?
            }
            else if (selectVoxel)
            {
                var selectedBlockPosition =
                    Vectors.FloorToInt(player.focused == null
                        ? player.HighlightBlock.position
                        : player.focused.GetBlockPosition());
                AddHighlight(selectedBlockPosition);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (player.HammerMode && (player.HighlightBlock.gameObject.activeSelf || player.focused != null))
                    DeleteBlock();
                else if (player.PlaceBlock.gameObject.activeSelf)
                {
                    World.INSTANCE.PutBlock(new VoxelPosition(player.PlaceBlock.position),
                        Blocks.GetBlockType(player.selectedBlockId));
                }
            }
            else if (Input.GetMouseButtonDown(1) &&
                     (player.HighlightBlock.gameObject.activeSelf || player.focused != null))
                DeleteBlock();
        }

        private void DeleteBlock()
        {
            var position = Vectors.FloorToInt(player.focused == null
                ? player.HighlightBlock.position
                : player.focused.GetBlockPosition());
            var vp = new VoxelPosition(position);
            var chunk = World.INSTANCE.GetChunkIfInited(vp.chunk);
            if (chunk != null)
            {
                if (player.focused == null || player.focused is ChunkFocusable)
                    chunk.DeleteVoxel(vp, player.HighlightLand);
                if (chunk.GetMetaAt(vp) != null)
                    chunk.DeleteMeta(vp);
            }
        }

        private void HandleBlockClipboard()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                ExitSelectionMode();
            }
            else if (Input.GetKeyDown(KeyCode.C) &&
                     (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                      Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)))
            {
                World.INSTANCE.ResetClipboard();
                ExitSelectionMode();
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                ConfirmMove();
                ExitSelectionMode();
            }
            else if (Input.GetButtonDown("Delete"))
            {
                World.INSTANCE.RemoveSelectedBlocks();
                ExitSelectionMode();
            }
            else if (player.PlaceBlock.gameObject.activeSelf && !World.INSTANCE.ClipboardEmpty &&
                     Input.GetKeyDown(KeyCode.V) &&
                     (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                      Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)))
            {
                var minPoint = World.INSTANCE.GetClipboardMinPoint();
                var conflictWithPlayer = false;
                var currVox = Vectors.TruncateFloor(transform.position);
                ClearSelection();
                foreach (var pos in World.INSTANCE.ClipboardWorldPositions)
                {
                    var newPosition = pos - minPoint + player.PlaceBlockPosInt;
                    if (currVox.Equals(newPosition) || currVox.Equals(newPosition + Vector3Int.up) ||
                        currVox.Equals(newPosition - Vector3Int.up))
                    {
                        conflictWithPlayer = true;
                        break;
                    }
                }

                if (!conflictWithPlayer)
                {
                    World.INSTANCE.PasteClipboard(player.PlaceBlockPosInt - minPoint);
                    clipboardMovement = true;
                    movingSelectionAllowed = true;
                    SetBlockSelectionSnack();
                }
            }
        }

        private bool AddHighlight(Vector3Int position)
        {
            if (!player.CanEdit(position, out var land)) return false;
            var added = World.INSTANCE.AddHighlight(new VoxelPosition(position));
            if (!added) // removed
            {
                if (!World.INSTANCE.SelectionActive)
                    ExitSelectionMode();
                return false;
            }

            if (World.INSTANCE.TotalBlocksSelected == 1)
            {
                movingSelectionAllowed = false;
                SetBlockSelectionSnack();
                selectedBlocksCountTextContainer.gameObject.SetActive(true);
            }

            UpdateCountMsg();
            return true;
        }

        private void ConfirmMove()
        {
            World.INSTANCE.DuplicateSelectedBlocks();
            if (!clipboardMovement)
                World.INSTANCE.RemoveSelectedBlocks(true);
        }

        private void ClearSelection()
        {
            World.INSTANCE.ClearHighlights();
            selectedBlocksCountTextContainer.gameObject.SetActive(false);
        }

        private void UpdateCountMsg()
        {
            var count = World.INSTANCE.TotalBlocksSelected;
            selectedBlocksCountText.text = count == 1 ? "1 Block Selected" : count + " Blocks Selected";
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
                    clipboardMovement = false;
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
                    lines.Add("R + horizontal mouse movement : rotate around y axis");
                    lines.Add("R + vertical mouse movement : rotate around player right axis");
                }

                lines.Add("CTRL+C/V : copy/paste selection");
                lines.Add("CTRL+CLICK : select/unselect block");
                lines.Add(
                    "CTRL+SHIFT+CLICK : select/unselect all blocks between the last selected block and current block");
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

        public void ExitSelectionMode()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            ClearSelection();
            movingSelectionAllowed = false;
        }

        public static BlockSelectionController INSTANCE =>
            GameObject.Find("Player").GetComponent<BlockSelectionController>();
    }
}