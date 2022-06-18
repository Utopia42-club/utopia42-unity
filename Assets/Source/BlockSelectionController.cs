using System.Collections.Generic;
using System.Numerics;
using Source.Canvas;
using Source.Model;
using Source.Utils;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Source
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
        public Vector3? DraggedPosition { get; private set; }
        private bool KeepSourceAfterSelectionMovement => selectionMode != SelectionMode.Default;

        public bool PlayerMovementAllowed => !selectionActive || !movingSelectionAllowed;

        private Transform transform => Player.INSTANCE.transform; // TODO

        private bool selectionActive;
        private bool metaSelectionActive;
        private bool onlyMetaSelectionActive;
        private bool dragging;

        private int keyboardFrameCounter = 0;

        public void Start()
        {
            player = Player.INSTANCE;
            mouseLook = MouseLook.INSTANCE;
            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state != GameManager.State.PLAYING && selectionActive)
                    ExitSelectionMode();
            });
        }

        public void DoUpdate()
        {
            selectionActive = World.INSTANCE.SelectionActive;
            metaSelectionActive = World.INSTANCE.MetaSelectionActive;
            onlyMetaSelectionActive = World.INSTANCE.OnlyMetaSelectionActive;
            dragging = DraggedPosition.HasValue;

            HandleSelectionKeyboardMovement();
            HandleSelectionMouseMovement();
            HandleBlockRotation();
            HandleBlockSelection();
            HandleBlockClipboard();

            keyboardFrameCounter++;
        }

        private void HandleBlockRotation()
        {
            if (dragging || !selectionActive || metaSelectionActive || rotationMode == Input.GetKey(KeyCode.R)) return;
            rotationMode = !rotationMode;
            if (!rotationMode)
                mouseLook.RemoveRotationTarget();
            else if (selectionActive &&
                     !World.INSTANCE
                         .OnlyMetaSelectionActive) // multiple selection rotation is not support for meta selection only
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

        private void HandleSelectionKeyboardMovement()
        {
            if (!selectionActive || !movingSelectionAllowed || dragging) return;

            var frameCondition = onlyMetaSelectionActive ? keyboardFrameCounter % 5 == 0 : keyboardFrameCounter % 50 == 0;

            var jump = Input.GetButtonDown("Jump") || frameCondition && Input.GetButton("Jump");
            var horizontal = Input.GetButtonDown("Horizontal") || frameCondition && Input.GetButton("Horizontal");
            var vertical = Input.GetButtonDown("Vertical") || frameCondition && Input.GetButton("Vertical");

            if (jump || horizontal || vertical) keyboardFrameCounter = 0;

            var moveDown = jump &&
                           (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            var moveUp = !moveDown && jump;

            var moveDirection = horizontal || vertical
                ? (transform.forward * player.Vertical + transform.right * player.Horizontal).normalized
                : Vector3.zero;
            var movement = moveUp || moveDown || moveDirection.magnitude > Player.CastStep;

            if (!movement) return;

            var delta =
                Vectors.FloorToInt(0.5f * Vector3.one + moveDirection) +
                (moveUp ? Vector3Int.up : moveDown ? Vector3Int.down : Vector3Int.zero);

            if (!onlyMetaSelectionActive)
            {
                World.INSTANCE.MoveSelection(delta, null);
                return;
            }

            World.INSTANCE.MoveMetaSelection(0.1f * (Vector3) delta, null);
        }

        private void HandleSelectionMouseMovement()
        {
            if (!selectionActive || player.CtrlDown) return;
            if (Input.GetMouseButtonUp(0) && DraggedPosition != null)
            {
                ConfirmMove();
                ReSelectSelection();
                DraggedPosition = null;
            }

            else if (Input.GetMouseButtonDown(0) && DraggedPosition == null)
            {
                if (player.FocusedFocusable is not MetaFocusable metaFocusable) return; // TODO ?

                DraggedPosition = metaFocusable.GetBlockPosition();

                if (DraggedPosition.HasValue)
                    DraggedPosition = new Vector3(DraggedPosition.Value.x, World.INSTANCE.GetSelectionMinPoint().y,
                        DraggedPosition.Value.z);
            }

            else if (Input.GetMouseButton(0) && DraggedPosition != null && selectionActive &&
                     player.FocusedFocusable is ChunkFocusable &&
                     player.CanEdit(player.PossiblePlaceBlockPosInt, out _, true))
            {
                var minY = World.INSTANCE.GetSelectionMinPoint().y;
                if (onlyMetaSelectionActive)
                {
                    var pos = player.PossiblePlaceMetaBlockPos;
                    if (pos.y > minY)
                        pos.y += pos.y - minY;
                    World.INSTANCE.MoveMetaSelection(pos, DraggedPosition.Value);
                }
                else
                {
                    var pos = player.PossiblePlaceBlockPosInt;
                    if (pos.y > minY)
                        pos.y += pos.y - Mathf.FloorToInt(minY);
                    World.INSTANCE.MoveSelection(pos,
                        Vectors.TruncateFloor(DraggedPosition.Value));
                }
            }
        }

        private void HandleBlockSelection()
        {
            if (dragging || rotationMode || !mouseLook.cursorLocked) return;

            var selectVoxel = !World.INSTANCE.SelectionDisplaced && selectionMode == SelectionMode.Default &&
                              (player.HighlightBlock.gameObject.activeSelf || player.FocusedFocusable != null) &&
                              Input.GetMouseButtonDown(0) && player.CtrlDown;

            var multipleSelect = selectVoxel && selectionActive &&
                                 (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                                 World.INSTANCE.LastSelectedPosition != null;

            if (multipleSelect) // TODO: add support for metablock multiselection?
            {
                var lastSelectedPosition = World.INSTANCE.LastSelectedPosition.ToWorld();
                if (!player.HighlightBlock.gameObject.activeSelf) return;
                var currentSelectedPosition = player.PossibleHighlightBlockPosInt;

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
                if (player.FocusedFocusable == null) // should never happen
                {
                    if (player.HighlightBlock.gameObject.activeSelf)
                        AddHighlight(new VoxelPosition(player.PossibleHighlightBlockPosInt));
                }
                else if (player.FocusedFocusable is ChunkFocusable chunkFocusable)
                {
                    var pos = chunkFocusable.GetBlockPosition();
                    if (pos.HasValue)
                        AddHighlight(new VoxelPosition(pos.Value));
                }
                else
                {
                    var pos = player.FocusedFocusable.GetBlockPosition();
                    if (pos.HasValue)
                        AddHighlight(new MetaPosition(pos.Value));
                }
            }

            if (player.CtrlDown || selectionActive) return;

            if (Input.GetMouseButtonDown(0))
            {
                if (player.HammerMode)
                    DeleteBlock();
                else if (player.PlaceBlock.gameObject.activeSelf)
                {
                    World.INSTANCE.TryPutVoxel(new VoxelPosition(player.PossiblePlaceBlockPosInt),
                        player.SelectedBlockType);
                }
                else if (player.MetaBlockPlaceHolder != null && player.MetaBlockPlaceHolder.activeSelf)
                {
                    World.INSTANCE.TryPutMeta(new MetaPosition(player.MetaBlockPlaceHolder.transform.position),
                        player.SelectedBlockType);
                }
                else if (player.PreparedMetaBlock != null && player.PreparedMetaBlock.IsActive)
                {
                    World.INSTANCE.PutMetaWithProps(
                        new MetaPosition(player.PreparedMetaBlock.blockObject.transform.position),
                        player.PreparedMetaBlock.type, player.PreparedMetaBlock.GetProps(), player.placeLand);
                }
            }
            else if (Input.GetMouseButtonDown(1))
                DeleteBlock();
        }

        private void DeleteBlock()
        {
            if (player.HighlightBlock.gameObject.activeSelf)
            {
                var position = Vectors.FloorToInt(player.PossibleHighlightBlockPosInt);
                var vp = new VoxelPosition(position);
                World.INSTANCE.TryDeleteVoxel(vp);
                return;
            }

            if (player.FocusedFocusable != null && player.FocusedFocusable is MetaFocusable metaFocusable)
            {
                var position = metaFocusable.GetBlockPosition();
                if (position != null)
                    World.INSTANCE.TryDeleteMeta(new MetaPosition(position.Value));
            }
        }

        private void HandleBlockClipboard()
        {
            if (dragging) return;
            if (Input.GetKeyDown(KeyCode.X))
            {
                ExitSelectionMode();
            }
            else if (Input.GetKeyDown(KeyCode.C) && player.CtrlDown)
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
            else if (Input.GetKeyDown(KeyCode.V) && player.CtrlDown &&
                     player.PlaceBlock.gameObject.activeSelf &&
                     !World.INSTANCE.ClipboardEmpty &&
                     !selectionActive)
            {
                var minPoint = World.INSTANCE.GetClipboardMinPoint();
                var minIntPoint = Vectors.TruncateFloor(minPoint);
                ClearSelection();

                if (!onlyMetaSelectionActive)
                {
                    var conflictWithPlayer = false;
                    var currVox = Vectors.TruncateFloor(transform.position);
                    foreach (var pos in World.INSTANCE.ClipboardVoxelPositions)
                    {
                        var newPosition = pos - minIntPoint + player.PossiblePlaceBlockPosInt;
                        if (currVox.Equals(newPosition) || currVox.Equals(newPosition + Vector3Int.up) ||
                            currVox.Equals(newPosition - Vector3Int.up))
                        {
                            conflictWithPlayer = true;
                            break;
                        }
                    }

                    if (!conflictWithPlayer)
                        World.INSTANCE.PasteClipboard(player.PossiblePlaceBlockPosInt - minIntPoint);
                }
                else
                    World.INSTANCE.PasteClipboard(player.PossiblePlaceMetaBlockPos - minPoint);

                PrepareForClipboardMovement(SelectionMode.Clipboard);
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

        private void ReSelectSelection()
        {
            var selectedPositions = World.INSTANCE.GetSelectedBlocksPositions();
            var selectedMetaPositions = World.INSTANCE.GetSelectedMetaBlocksPositions();
            ExitSelectionMode();
            AddHighlights(selectedPositions);
            foreach (var metaPosition in selectedMetaPositions)
            {
                AddHighlight(metaPosition);
            }
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
                if (Input.GetKeyDown(KeyCode.V) && !player.CtrlDown)
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
                if (!metaSelectionActive)
                {
                    lines.Add("R + horizontal mouse movement : rotate around y axis");
                    lines.Add("R + vertical mouse movement : rotate around player right axis");
                }
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
            DraggedPosition = null;
        }

        public void AddHighlights(List<VoxelPosition> vps)
        {
            if (vps.Count == 0) return;
            World.INSTANCE.AddHighlights(vps, result =>
            {
                if (!result.ContainsValue(true)) return;
                AfterAddHighlight();
            });
        }

        public void AddPreviewHighlights(Dictionary<VoxelPosition, uint> highlights)
        {
            if (highlights.Count == 0 || selectionMode != SelectionMode.Preview && selectionActive)
                return; // do not add preview highlights after existing non-preview highlights
            StartCoroutine(World.INSTANCE.AddHighlights(highlights, result =>
            {
                if (!result.ContainsValue(true)) return;
                AfterAddHighlight(false);
                PrepareForClipboardMovement(SelectionMode.Preview);
            }));
        }

        public void AddHighlight(VoxelPosition vp)
        {
            World.INSTANCE.AddHighlight(vp, _ => AfterAddHighlight());
        }

        public void AddHighlight(MetaPosition mp)
        {
            var metaSelectionActive = World.INSTANCE.MetaSelectionActive;
            World.INSTANCE.AddHighlight(mp, () =>
            {
                AfterAddHighlight();
                if (metaSelectionActive != World.INSTANCE.MetaSelectionActive)
                    SetBlockSelectionSnack();
            });
        }

        private void AfterAddHighlight(bool setSnack = true)
        {
            var total = World.INSTANCE.TotalBlocksSelected;

            if (total == 0)
                ExitSelectionMode();
            else
            {
                if (setSnack)
                {
                    movingSelectionAllowed = false;
                    SetBlockSelectionSnack();
                }

                selectedBlocksCountTextContainer.gameObject.SetActive(true);
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
        }
    }
}