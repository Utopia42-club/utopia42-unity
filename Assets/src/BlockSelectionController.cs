using System;
using System.Collections.Generic;
using src.Canvas;
using src.MetaBlocks;
using src.MetaBlocks.TdObjectBlock;
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
        private bool movingSelectionAllowed = false;
        private bool rotationMode = false;
        public SelectionMode selectionMode { private set; get; } = SelectionMode.Default;
        private bool KeepSourceAfterSelectionMovement => selectionMode != SelectionMode.Default;

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
                HandleBlockMovement();
            HandleBlockRotation();
            HandleBlockSelection();
            HandleBlockClipboard();
        }

        private void HandleBlockRotation()
        {
            if (!World.INSTANCE.SelectionActive) return;
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
                Vectors.FloorToInt(0.5f * Vector3.one + moveDirection) +
                (moveUp ? Vector3Int.up : moveDown ? Vector3Int.down : Vector3Int.zero);

            World.INSTANCE.MoveSelection(delta);
        }

        private void HandleBlockSelection()
        {
            if (rotationMode || !mouseLook.cursorLocked) return;

            var ctrlHeld = Input.GetKey(KeyCode.LeftControl) ||
                           Input.GetKey(KeyCode.RightControl) ||
                           Input.GetKey(KeyCode.LeftCommand) ||
                           Input.GetKey(KeyCode.RightCommand);

            var selectVoxel = !World.INSTANCE.SelectionDisplaced && selectionMode == SelectionMode.Default &&
                              (player.HighlightBlock.gameObject.activeSelf || player.focused != null) &&
                              Input.GetMouseButtonDown(0) && ctrlHeld;

            var multipleSelect = selectVoxel && World.INSTANCE.SelectionActive &&
                                 (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

            if (multipleSelect)
            {
                var lastSelectedPosition = World.INSTANCE.lastSelectedPosition.ToWorld();
                var possibleCurrentSelectedPosition = player.focused == null
                    ? player.HighlightBlock.position
                    : player.focused.GetBlockPosition();
                if (!possibleCurrentSelectedPosition.HasValue) return;
                var currentSelectedPosition =
                    Vectors.FloorToInt(possibleCurrentSelectedPosition.Value); // TODO: extract method

                var from = new Vector3Int(Mathf.Min(lastSelectedPosition.x, currentSelectedPosition.x),
                    Mathf.Min(lastSelectedPosition.y, currentSelectedPosition.y),
                    Mathf.Min(lastSelectedPosition.z, currentSelectedPosition.z));
                var to = new Vector3Int(Mathf.Max(lastSelectedPosition.x, currentSelectedPosition.x),
                    Mathf.Max(lastSelectedPosition.y, currentSelectedPosition.y),
                    Mathf.Max(lastSelectedPosition.z, currentSelectedPosition.z));

                var vps = new List<VoxelPosition>();
                for (var x = from.x; x <= to.x; x++)
                for (var y = from.y; y <= to.y; y++)
                for (var z = from.z; z <= to.z; z++)
                {
                    var position = new Vector3Int(x, y, z);
                    if (position.Equals(lastSelectedPosition) || position.Equals(currentSelectedPosition)) continue;
                    vps.Add(new VoxelPosition(position));
                }

                vps.Add(new VoxelPosition(currentSelectedPosition));
                AddHighlights(vps);
            }
            else if (selectVoxel)
            {
                var possibleCurrentSelectedPosition = player.focused == null
                    ? player.HighlightBlock.position
                    : player.focused.GetBlockPosition();
                if (!possibleCurrentSelectedPosition.HasValue) return;
                var currentSelectedPosition =
                    Vectors.FloorToInt(possibleCurrentSelectedPosition.Value);
                AddHighlight(new VoxelPosition(currentSelectedPosition));
            }

            if (ctrlHeld) return;

            if (Input.GetMouseButtonDown(0))
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
            var possiblePosition = player.focused == null
                ? player.HighlightBlock.position
                : player.focused.GetBlockPosition();
            if (!possiblePosition.HasValue) return;
            var position =
                Vectors.FloorToInt(possiblePosition.Value);
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
                if (!KeepSourceAfterSelectionMovement)
                    World.INSTANCE.RemoveSelectedBlocks();
                ExitSelectionMode();
            }
            else if (Input.GetKeyDown(KeyCode.V) &&
                     (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                      Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) &&
                     player.PlaceBlock.gameObject.activeSelf && !World.INSTANCE.ClipboardEmpty &&
                     !World.INSTANCE.SelectionActive)
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
                    PrepareForClipboardMovement(SelectionMode.Clipboard);
                }
            }
        }

        private void PrepareForClipboardMovement(SelectionMode mode)
        {
            selectionMode = mode;
            movingSelectionAllowed = true;
            SetBlockSelectionSnack();
        }

        private void ConfirmMove()
        {
            World.INSTANCE.DuplicateSelectedBlocks(!KeepSourceAfterSelectionMovement);
            if (!KeepSourceAfterSelectionMovement)
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

        public void ExitSelectionMode()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            ClearSelection();
            movingSelectionAllowed = false;
            selectionMode = SelectionMode.Default;
        }

        public void AddHighlights(List<VoxelPosition> vps)
        {
            if (vps.Count == 0) return;
            World.INSTANCE.AddHighlights(vps, () => AfterAddHighlight());
        }

        public void AddPreviewHighlights(Dictionary<VoxelPosition, Tuple<uint, MetaBlock>> highlights)
        {
            if (highlights.Count == 0 || selectionMode != SelectionMode.Preview && World.INSTANCE.SelectionActive)
                return; // do not add preview highlights after existing non-preview highlights
            StartCoroutine(World.INSTANCE.AddHighlights(highlights, () =>
            {
                AfterAddHighlight(false);
                PrepareForClipboardMovement(SelectionMode.Preview);
            }));
        }

        public void AddDraggedGlbHighlight(string url)
        {
            var vp = new VoxelPosition(player.transform.position + 3 * Vector3.up);
            World.INSTANCE.AddHighlight(vp, () =>
                {
                    selectionMode = SelectionMode.Dragged;
                },
                new Tuple<uint, MetaBlock>(Blocks.GetBlockType("#000000").id,
                    Blocks.TdObjectBlockType.Instantiate(null,
                        "{\"url\":\"" + url + "\", \"type\":\"GLB\"}")));
        }


        public void AddHighlight(VoxelPosition vp)
        {
            World.INSTANCE.AddHighlight(vp, () => AfterAddHighlight());
        }

        private void AfterAddHighlight(bool setSnack = true)
        {
            var total = World.INSTANCE.TotalBlocksSelected;
            switch (total)
            {
                case 0:
                    ExitSelectionMode();
                    break;
                case 1:
                    if (setSnack)
                    {
                        movingSelectionAllowed = false;
                        SetBlockSelectionSnack();
                    }

                    selectedBlocksCountTextContainer.gameObject.SetActive(true);
                    break;
            }

            UpdateCountMsg();
        }

        public static BlockSelectionController INSTANCE =>
            GameObject.Find("Player").GetComponent<BlockSelectionController>();

        public enum SelectionMode
        {
            Default,
            Clipboard,
            Preview,
            Dragged
        }
    }
}