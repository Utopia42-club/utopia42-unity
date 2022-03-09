using System.Collections.Generic;
using src.Canvas;
using src.MetaBlocks.ImageBlock;
using src.Model;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.NftBlock
{
    public class NftBlockObject : ImageBlockObject
    {
        public override void OnDataUpdate()
        {
            RenderFaces();
        }

        protected override void DoInitialize()
        {
            RenderFaces();
        }

        public override void Focus(Voxels.Face face)
        {
            if (!canEdit) return;
            if (snackItem != null) snackItem.Remove();

            snackItem = Snack.INSTANCE.ShowLines(GetFaceSnackLines(face), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps(face);
                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
                if (Input.GetKeyDown(KeyCode.O))
                    OpenLink(face);
            });

            lastFocusedFaceIndex = face.index;
        }

        protected override List<string> GetFaceSnackLines(Voxels.Face face)
        {
            var lines = new List<string>
            {
                "Press Z for details",
                "Press T to toggle preview",
                "Press Del to delete"
            };

            var props = GetBlock().GetProps();
            var url = (props as NftBlockProperties)?.GetFaceProps(face)?.marketUrl;
            if (!string.IsNullOrEmpty(url))
                lines.Add("Press O to open in web");

            if (stateMsg[face.index] != StateMsg.Ok)
                lines.Add($"\n{MetaBlockState.ToString(stateMsg[face.index], "image")}");
            return lines;
        }

        private new void RenderFaces()
        {
            DestroyImages();
            images.Clear();
            var properties = (NftBlockProperties) GetBlock().GetProps();
            if (properties == null) return;

            AddFace(Voxels.Face.BACK,
                properties.back); // TODO: add face needs to do sth else as well here? or maybe not?
            AddFace(Voxels.Face.FRONT, properties.front);
            AddFace(Voxels.Face.RIGHT, properties.right);
            AddFace(Voxels.Face.LEFT, properties.left);
            AddFace(Voxels.Face.TOP, properties.top);
            AddFace(Voxels.Face.BOTTOM, properties.bottom);
        }

        private void EditProps(Voxels.Face face)
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("NFT Block Properties")
                .WithContent(NftBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<NftBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue((props as NftBlockProperties)?.GetFaceProps(face));
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new NftBlockProperties(GetBlock().GetProps() as NftBlockProperties);

                props.SetFaceProps(face, value);
                if (props.IsEmpty()) props = null;

                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }

        private void OpenLink(Voxels.Face face)
        {
            var props = GetBlock().GetProps();
            var url = (props as NftBlockProperties)?.GetFaceProps(face)?.marketUrl;
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
        }
    }
}