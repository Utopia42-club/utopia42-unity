using System.Collections.Generic;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.LightBlock
{
    public class LightBlockObject : MetaBlockObject
    {
        private SnackItem snackItem;
        private Land land;
        private bool canEdit;
        private bool ready = false;
        private Light light;

        private void Start()
        {
            if (canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land))
                CreateIcon();
            ready = true;
            gameObject.name = "light metablock";
        }

        public override bool IsReady()
        {
            return ready;
        }

        public override void OnDataUpdate()
        {
            ResetLight();
        }

        protected override void DoInitialize()
        {
            ResetLight();
        }

        private void ResetLight()
        {
            if (light == null)
            {
                light = gameObject.AddComponent<Light>();
                light.type = LightType.Point;
            }

            var props = GetProps();
            if (props == null) return;
            light.color = ColorUtility.TryParseHtmlString(props.hexColor, out var color)
                ? color
                : LightBlockEditor.DefaultColor;
            light.range = props.range;
            light.intensity = props.intensity;
        }


        public override void Focus(Voxels.Face face)
        {
            if (!canEdit) return;
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            var lines = new List<string>();
            lines.Add("Press Z for details");
            lines.Add("Press T to toggle preview");
            lines.Add("Press DEL to delete object");

            snackItem = Snack.INSTANCE.ShowLines(lines, () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
            });
        }

        private LightBlockProperties GetProps()
        {
            return (LightBlockProperties) GetBlock().GetProps();
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
                .WithTitle("Light Block Properties")
                .WithContent(LightBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<LightBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue(props == null ? null : props as LightBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new LightBlockProperties(GetBlock().GetProps() as LightBlockProperties);
                if (value != null)
                {
                    props.intensity = value.intensity;
                    props.range = value.range;
                    props.hexColor = value.hexColor;
                }

                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }
    }
}