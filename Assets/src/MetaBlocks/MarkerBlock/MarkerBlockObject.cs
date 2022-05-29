using System;
using System.Collections.Generic;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.MarkerBlock
{
    public class MarkerBlockObject : MetaBlockObject
    {
        private SnackItem snackItem;
        private Land land;
        private bool canEdit;
        private bool ready = false;

        private void Start()
        {
            if (canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land))
                CreateIcon();
            ready = true;
        }

        public override bool IsReady()
        {
            return ready;
        }

        public override void OnDataUpdate()
        {
        }

        protected override void DoInitialize()
        {
        }


        public override void Focus(Voxels.Face face)
        {
            if (!canEdit) return;
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            snackItem = Snack.INSTANCE.ShowLines(GetFaceSnackLines(), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
            });
        }

        private MarkerBlockProperties GetProps()
        {
            return (MarkerBlockProperties) GetBlock().GetProps();
        }

        public override void UnFocus()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }
        }

        public override void UpdateStateAndIcon(StateMsg msg, Voxels.Face face) // TODO
        {
            throw new System.NotImplementedException();
        }

        protected override List<string> GetFaceSnackLines(Voxels.Face face = null)
        {
            return new List<string>
            {
                "Press Z for details",
                "Press T to toggle preview",
                "Press DEL to delete object"
            };
        }

        public override void ShowFocusHighlight()
        {
        }

        public override void RemoveFocusHighlight()
        {
        }

        public override GameObject CreateSelectHighlight(Transform parent, bool show = true)
        {
            return null;
        }

        protected override void UpdateState(StateMsg stateMsg)
        {
            throw new System.NotImplementedException();
        }

        public override void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform, Vector3Int localPos, Action<GameObject> onLoad)
        {
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Marker Block Properties")
                .WithContent(MarkerBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<MarkerBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue(props == null ? null : props as MarkerBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new MarkerBlockProperties(GetBlock().GetProps() as MarkerBlockProperties);
                if (value != null)
                    props.name = value.name;
                if (props.IsEmpty()) props = null;
                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }
    }
}