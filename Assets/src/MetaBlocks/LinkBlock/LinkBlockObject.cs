using System.Collections.Generic;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.LinkBlock
{
    public class LinkBlockObject : MetaBlockObject
    {
        private SnackItem snackItem;
        private Land land;
        private bool canEdit;
        private bool ready = false;

        private void Start()
        {
            canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land);
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
            UpdateSnacks();
        }

        private void UpdateSnacks()
        {
            if (snackItem != null) snackItem.Remove();
            var lines = new List<string>();
            if (canEdit)
            {
                lines.Add("Press Z for details");
                lines.Add("Press Del to delete");
            }

            LinkBlockProperties props = GetProps();
            if (props != null && !props.IsEmpty())
            {
                if (props.pos == null)
                    lines.Add("Press O to open in web");
                else
                    lines.Add("Press O to transport");
            }
            snackItem = Snack.INSTANCE.ShowLines(lines, () =>
            {
                if (canEdit)
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                        EditProps();
                    if (Input.GetButtonDown("Delete"))
                        GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
                }
                if (props != null && !props.IsEmpty() && Input.GetKeyDown(KeyCode.O))
                    OpenLink();
            });
        }

        private void OpenLink()
        {
            LinkBlockProperties faceProps = GetProps();
            if (faceProps.pos == null)
                Application.OpenURL(faceProps.url);
            else
                GameManager.INSTANCE.MovePlayerTo(new Vector3(faceProps.pos[0], faceProps.pos[1], faceProps.pos[2]));
        }

        private LinkBlockProperties GetProps()
        {
            return (LinkBlockProperties)GetBlock().GetProps();
        }

        public override void UnFocus()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Link Block Properties")
                .WithContent(LinkBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<LinkBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue(props as LinkBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                Debug.Log(value.url);
                Debug.Log(value.pos);
                if (value.pos != null) value.url = null;
                if (value.IsEmpty()) value = null;
                Debug.Log(value);
                GetBlock().SetProps(value, land);
                manager.CloseDialog(dialog);
                UpdateSnacks();
            });
        }
    }
}
