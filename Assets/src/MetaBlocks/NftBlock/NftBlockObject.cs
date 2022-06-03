using System;
using System.Collections;
using System.Collections.Generic;
using src.Canvas;
using src.MetaBlocks.ImageBlock;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.NftBlock
{
    public class NftBlockObject : ImageBlockObject
    {
        private NftMetadata metadata = new NftMetadata();

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
            if (snackItem != null) snackItem.Remove();

            snackItem = Snack.INSTANCE.ShowLines(GetFaceSnackLines(face), () =>
            {
                if (canEdit)
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                        EditProps(face);
                    if (Input.GetButtonDown("Delete"))
                        GetChunk().DeleteMeta(new MetaPosition(transform.localPosition));
                    if (Input.GetKeyDown(KeyCode.T))
                        GetIconObject().SetActive(!GetIconObject().activeSelf);
                }

                if (Input.GetKeyDown(KeyCode.O))
                    OpenLink();
            });

            lastFocusedFaceIndex = face.index;
        }

        protected override List<string> GetFaceSnackLines(Voxels.Face face = null)
        {
            var lines = new List<string>();
            if (canEdit)
            {
                lines.AddRange(new[]
                {
                    "Press Z for details",
                    "Press T to toggle preview",
                    "Press Del to delete"
                });
            }

            var props = GetBlock().GetProps();
            var url = (props as NftBlockProperties)?.GetOpenseaUrl();
            if (!string.IsNullOrEmpty(url))
                lines.Add("Press O to open Opensea URL");

            if (metadata != null)
            {
                if (!string.IsNullOrWhiteSpace(metadata.name))
                    lines.Add($"\nName: {metadata.name.Trim()}");
                if (!string.IsNullOrWhiteSpace(metadata.description))
                    lines.Add($"\nDescription: {metadata.description.Trim()}");
            }

            if (stateMsg != StateMsg.Ok)
                lines.Add(
                    $"\n{MetaBlockState.ToString(stateMsg, "image")}");

            return lines;
        }

        private new void RenderFaces()
        {
            DestroyImages();
            images.Clear();
            metadata = null;
            var properties = (NftBlockProperties) GetBlock().GetProps();
            if (properties == null) return;

            AddFaceProperties(Voxels.Face.BACK, properties);
        }

        private void AddFaceProperties(Voxels.Face face, NftBlockProperties props)
        {
            if (props == null || string.IsNullOrWhiteSpace(props.collection)) return;
            UpdateStateAndView(StateMsg.LoadingMetadata, face);
            StartCoroutine(GetMetadata(props.collection, props.tokenId, md =>
            {
                metadata = md;
                // var imageUrl = $"{Constants.ApiURL}/nft-metadata/image/{props.collection}/{props.tokenId}";
                var imageUrl = string.IsNullOrWhiteSpace(md.image) ? md.imageUrl : md.image;
                AddFace(face, props.ToImageProp(imageUrl));
            }, () => { UpdateStateAndView(StateMsg.ConnectionError, face); }));
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
            editor.SetValue(props as NftBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new NftBlockProperties(GetBlock().GetProps() as NftBlockProperties);

                props.UpdateProps(value);
                if (props.IsEmpty()) props = null;

                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }

        private void OpenLink()
        {
            var props = GetBlock().GetProps();
            var url = (props as NftBlockProperties)?.GetOpenseaUrl();
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
        }

        private static IEnumerator GetMetadata(string collection, long tokenId, Action<NftMetadata> onSuccess,
            Action onFailure)
        {
            yield return RestClient.Get($"{Constants.ApiURL}/nft-metadata/{collection}/{tokenId}"
                , onSuccess, onFailure);
        }
    }
}