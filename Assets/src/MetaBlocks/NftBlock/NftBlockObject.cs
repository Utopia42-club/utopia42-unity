using System;
using System.Collections;
using System.Collections.Generic;
using src.Canvas;
using src.MetaBlocks.ImageBlock;
using src.Model;
using src.Service;
using src.Service.Ethereum;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.NftBlock
{
    public class NftBlockObject : ImageBlockObject
    {
        public Dictionary<Voxels.Face, NftMetadata> metadata = new Dictionary<Voxels.Face, NftMetadata>();

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
            var url = (props as NftBlockProperties)?.GetFaceProps(face)?.OpenseaUrl;
            if (!string.IsNullOrEmpty(url))
                lines.Add("Press O to open Opensea URL");

            if (metadata.TryGetValue(face, out var faceMetadata))
            {
                if (!string.IsNullOrWhiteSpace(faceMetadata.name))
                    lines.Add($"\nName: {faceMetadata.name.Trim()}");
                if (!string.IsNullOrWhiteSpace(faceMetadata.description))
                    lines.Add($"\nDescription: {faceMetadata.description.Trim()}");
            }

            if (stateMsg[face.index] != StateMsg.Ok)
                lines.Add($"\n{MetaBlockState.ToString(stateMsg[face.index], "image")}");
            return lines;
        }

        private new void RenderFaces()
        {
            DestroyImages();
            images.Clear();
            metadata.Clear();
            var properties = (NftBlockProperties) GetBlock().GetProps();
            if (properties == null) return;

            AddFaceProperties(Voxels.Face.BACK, properties.back);
            AddFaceProperties(Voxels.Face.FRONT, properties.front);
            AddFaceProperties(Voxels.Face.RIGHT, properties.right);
            AddFaceProperties(Voxels.Face.LEFT, properties.left);
            AddFaceProperties(Voxels.Face.TOP, properties.top);
            AddFaceProperties(Voxels.Face.BOTTOM, properties.bottom);
        }

        private void AddFaceProperties(Voxels.Face face, NftBlockProperties.FaceProps props)
        {
            if (props == null || string.IsNullOrWhiteSpace(props.collection) || string.IsNullOrWhiteSpace(props.tokenId)) return;
            StartCoroutine(GetMetadata(props.collection, props.tokenId, md =>
            {
                metadata.Add(face, md);
                var imageUrl = string.IsNullOrWhiteSpace(md.image) ? md.imageUrl : md.image;
                AddFace(face, props.ToImageFaceProp(imageUrl));
            }, () => { }));
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
            var url = (props as NftBlockProperties)?.GetFaceProps(face)?.OpenseaUrl;
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
        }

        private IEnumerator GetMetadata(string collection, string tokenId, Action<NftMetadata> onSuccess,
            Action onFailure)
        {
            string metadataUri = null;
            yield return EthereumClientService.INSTANCE.GetTokenUri(collection, tokenId, uri => { metadataUri = uri; },
                onFailure);

            if (metadataUri == null) yield break;
            yield return RestClient.Get(metadataUri, onSuccess, onFailure);
        }
    }
}