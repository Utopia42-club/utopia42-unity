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

        protected override void Start()
        {
            base.Start();
            gameObject.name = "marker block object";
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
            base.DoInitialize();
        }


        public override void Focus()
        {
            if (!canEdit) return;
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new MetaPosition(transform.localPosition));
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

        protected override void OnStateChanged(State state) // TODO [detach metablock]
        {
            throw new System.NotImplementedException();
        }

        protected override List<string> GetSnackLines()
        {
            return new List<string>
            {
                "Press Z for details",
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

        public override void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform, Vector3Int localPos, Action<GameObject> onLoad)
        {
        }

        public override void SetToMovingState()
        {
            throw new NotImplementedException();
        }

        public override void ExitMovingState()
        {
            throw new NotImplementedException();
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