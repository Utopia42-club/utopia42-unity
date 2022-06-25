using System;
using System.Collections.Generic;
using System.Numerics;
using Source.Canvas;
using Source.MetaBlocks;
using Source.Model;
using Source.Utils;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Source
{
    public class BlockSelectionController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI selectedBlocksCountText;
        [SerializeField] private RectTransform selectedBlocksCountTextContainer;

        private MouseLook mouseLook;
        private Player player;
        private SnackItem snackItem;

        private bool movingSelectionAllowed = false;
        private SelectionMode SMode { set; get; } = SelectionMode.Default;
        private Vector3? DraggedPosition { get; set; }
        private bool KeepSourceAfterSelectionMovement => SMode != SelectionMode.Default;

        public bool PlayerMovementAllowed => (!selectionActive || !movingSelectionAllowed) && !scalingOrRotatingSelection;

        private bool selectionActive;
        private bool metaSelectionActive;
        private bool onlyMetaSelectionActive;
        private bool selectionDisplaced;
        private bool scalingOrRotatingSelection;

        public bool Dragging => DraggedPosition.HasValue;

        private int keyboardFrameCounter = 0;
        private bool horizontal;
        private bool vertical;
        private bool jump;
        private bool horizontalDown;
        private bool verticalDown;
        private bool jumpDown;
        private bool shiftHeld;

        public void Start()
        {
            player = Player.INSTANCE;
            mouseLook = MouseLook.INSTANCE;
            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                if (state != GameManager.State.PLAYING && selectionActive)
                    ExitSelectionMode();
            });

            mouseLook.cursorLockedStateChanged.AddListener(locked =>
            {
                if (!locked) StopDragDropProcess();
            });
        }

        public void DoUpdate()
        {
            GetInputs();
            HandleSelectionKeyboardMovement();
            HandleSelectionMouseMovement();
            HandleBlockSelection();
            HandleBlockClipboard();
        }

        private void FixedUpdate()
        {
            if (player.ChangeForbidden) return;
            HandleSelectionKeyboardMovement(true);
            keyboardFrameCounter++;
        }

        private void GetInputs()
        {
            selectionActive = World.INSTANCE.SelectionActive;
            metaSelectionActive = World.INSTANCE.MetaSelectionActive;
            onlyMetaSelectionActive = World.INSTANCE.OnlyMetaSelectionActive;
            selectionDisplaced = World.INSTANCE.SelectionDisplaced;
            scalingOrRotatingSelection = World.INSTANCE.ObjectScaleRotationController.Active;

            horizontal = Input.GetButton("Horizontal");
            vertical = Input.GetButton("Vertical");
            jump = Input.GetButton("Jump");
            horizontalDown = Input.GetButtonDown("Horizontal");
            verticalDown = Input.GetButtonDown("Vertical");
            jumpDown = Input.GetButtonDown("Jump");
            shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private void HandleSelectionKeyboardMovement(bool fixedRate = false)
        {
            if (!selectionActive || !movingSelectionAllowed || Dragging || scalingOrRotatingSelection)
                return;

            var correctFrame = onlyMetaSelectionActive || keyboardFrameCounter % 9 == 0;

            var jmp = !fixedRate ? jumpDown : correctFrame && jump;
            var hz = !fixedRate ? horizontalDown : correctFrame && horizontal;
            var vr = !fixedRate ? verticalDown : correctFrame && vertical;

            var moveDown = jmp && shiftHeld;
            var moveUp = !moveDown && jmp;

            var moveDirection = hz || vr
                ? (player.PlayerForward * player.Vertical + player.CamRight * player.Horizontal).normalized
                : Vector3.zero;
            var movement = moveUp || moveDown || moveDirection.magnitude > Player.CastStep;

            if (!movement) return;

            keyboardFrameCounter = fixedRate ? 0 : 1;

            var delta =
                Vectors.FloorToInt(0.5f * Vector3.one + moveDirection) +
                (moveUp ? Vector3Int.up : moveDown ? Vector3Int.down : Vector3Int.zero);

            if (!onlyMetaSelectionActive)
            {
                World.INSTANCE.MoveSelection(delta, null);
                return;
            }

            World.INSTANCE.MoveMetaSelection(MetaLocalPosition.Step * (Vector3) delta, null);
        }

        private void HandleSelectionMouseMovement()
        {
            if (!selectionActive || player.CtrlHeld || scalingOrRotatingSelection) return;
            if (Input.GetMouseButtonUp(0))
                StopDragDropProcess();

            else if (Input.GetMouseButtonDown(0) && DraggedPosition == null && !selectionDisplaced)
            {
                if (player.FocusedFocusable != null && player.FocusedFocusable.IsSelected())
                {
                    DraggedPosition = player.FocusedFocusable.GetBlockPosition();

                    if (DraggedPosition.HasValue)
                    {
                        DraggedPosition = new Vector3(DraggedPosition.Value.x, World.INSTANCE.GetSelectionMinPoint().y,
                            DraggedPosition.Value.z);
                    }
                }
            }

            else if (Input.GetMouseButton(0) && DraggedPosition != null && selectionActive &&
                     player.FocusedFocusable != null && !player.FocusedFocusable.IsSelected() &&
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

        private void StopDragDropProcess()
        {
            if (!DraggedPosition.HasValue) return;
            DraggedPosition = null;
            // if (selectionDisplaced) 
            //     PrepareForSelectionMovement(SelectionMode.Default); // already happening
        }

        private void HandleBlockSelection()
        {
            if (Dragging || scalingOrRotatingSelection || !mouseLook.cursorLocked) return;

            var selectVoxel = !selectionDisplaced && (player.CtrlHeld || player.CursorEmpty) &&
                              SMode == SelectionMode.Default &&
                              (player.HighlightBlock.gameObject.activeSelf || player.FocusedFocusable != null) &&
                              Input.GetMouseButtonDown(0);

            var multipleSelect = selectVoxel && player.CtrlHeld && selectionActive &&
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
                if (!player.CtrlHeld)
                    ExitSelectionMode();
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

            else if (!selectionActive && !player.CtrlHeld)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (player.HammerMode)
                        DeleteBlock();
                    else if (player.PlaceBlock.gameObject.activeSelf && player.SelectedBlockType != null &&
                             player.SelectedBlockType is not MetaBlockType)
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
                else if (player.HammerMode && Input.GetButtonDown("Delete"))
                    DeleteBlock();
            }
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
            if (Dragging || scalingOrRotatingSelection) return;
            if (Input.GetKeyDown(KeyCode.X) || Input.GetMouseButtonDown(1))
            {
                ExitSelectionMode();
            }
            else if (Input.GetKeyDown(KeyCode.C) && player.CtrlHeld && !selectionDisplaced)
            {
                World.INSTANCE.ResetClipboard();
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                if (selectionDisplaced)
                {
                    ConfirmMove();
                    ReSelectSelection();
                }
                else
                    ExitSelectionMode();
            }
            else if (Input.GetButtonDown("Delete"))
            {
                if (!KeepSourceAfterSelectionMovement)
                    World.INSTANCE.RemoveSelectedBlocks();
                ExitSelectionMode();
            }
            else if (Input.GetKeyDown(KeyCode.V) && player.CtrlHeld &&
                     player.PlaceBlock.gameObject.activeSelf &&
                     !World.INSTANCE.ClipboardEmpty &&
                     !Dragging)
            {
                var minPoint = World.INSTANCE.GetClipboardMinPoint();
                var minIntPoint = Vectors.TruncateFloor(minPoint);
                ClearSelection();

                if (!onlyMetaSelectionActive)
                {
                    var conflictWithPlayer = false;
                    var currVox = Vectors.TruncateFloor(player.GetPosition());
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

                PrepareForSelectionMovement(SelectionMode.Clipboard);
            }
        }

        private void PrepareForSelectionMovement(SelectionMode mode)
        {
            SMode = mode;
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
                if (Input.GetKeyDown(KeyCode.V) && !player.CtrlHeld)
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
                if (metaSelectionActive)
                {
                    lines.AddRange(new[]
                    {
                        "] : scale 3d objects up",
                        "[ : scale 3d objects down",
                        "R + A/D or horizontal mouse movement : rotate objects around y axis",
                        "R + W/S or vertical mouse movement : rotate objects around player right axis",
                    });
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
            SMode = SelectionMode.Default;
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
            if (highlights.Count == 0 || SMode != SelectionMode.Preview && selectionActive)
                return; // do not add preview highlights after existing non-preview highlights
            StartCoroutine(World.INSTANCE.AddHighlights(highlights, result =>
            {
                if (!result.ContainsValue(true)) return;
                AfterAddHighlight(false);
                PrepareForSelectionMovement(SelectionMode.Preview);
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
                    // movingSelectionAllowed = false; // TODO
                    // SetBlockSelectionSnack();
                    PrepareForSelectionMovement(SelectionMode.Default);
                }

                selectedBlocksCountTextContainer.gameObject.SetActive(true);
                PropertyEditor.INSTANCE.Hide();
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