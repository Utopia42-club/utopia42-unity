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
            RenderFace();
        }

        protected override void SetupDefaultSnack()
        {
            if (snackItem != null) snackItem.Remove();

            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (canEdit)
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        RemoveFocusHighlight();
                        EditProps();
                    }
                    if (Input.GetKeyDown(KeyCode.V))
                    {
                        RemoveFocusHighlight();
                        GameManager.INSTANCE.ToggleMovingObjectState(this);
                    }
                    if (Input.GetButtonDown("Delete"))
                    {
                        World.INSTANCE.TryDeleteMeta(new MetaPosition(transform.position));
                    }
                }

                if (Input.GetKeyDown(KeyCode.O))
                    OpenLink();
            });
        }

        protected override List<string> GetSnackLines()
        {
            var lines = new List<string>();
            if (canEdit)
            {
                lines.AddRange(new[]
                {
                    "Press Z for details",
                    "Press V to edit rotation",
                    "Press Del to delete"
                });
            }

            var props = Block.GetProps();
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

            if (state != State.Ok)
                lines.Add(
                    $"\n{MetaBlockState.ToString(state, "image")}");

            return lines;
        }

        protected override void RenderFace()
        {
            DestroyImage();
            metadata = null;
            var props = (NftBlockProperties) Block.GetProps();
            if (props == null || string.IsNullOrWhiteSpace(props.collection))
            {
                UpdateState(State.Empty);
                return;
            }
            UpdateState(State.LoadingMetadata);
            StartCoroutine(GetMetadata(props.collection, props.tokenId, md =>
            {
                metadata = md;
                // var imageUrl = $"{Constants.ApiURL}/nft-metadata/image/{props.collection}/{props.tokenId}";
                var imageUrl = string.IsNullOrWhiteSpace(md.image) ? md.imageUrl : md.image;
                AddFace(props.ToImageProp(imageUrl));
            }, () => { UpdateState(State.ConnectionError); }));
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("NFT Block Properties")
                .WithContent(NftBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<NftBlockEditor>();

            var props = Block.GetProps();
            editor.SetValue(props as NftBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new NftBlockProperties(Block.GetProps() as NftBlockProperties);

                props.UpdateProps(value);
                if (props.IsEmpty()) props = null;

                Block.SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }

        private void OpenLink()
        {
            var props = Block.GetProps();
            var url = (props as NftBlockProperties)?.GetOpenseaUrl();
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
        }

        private static IEnumerator GetMetadata(string collection, long tokenId, Action<NftMetadata> onSuccess,
            Action onFailure)
        {
            yield return RestClient.Get($"{Constants.ApiURL}/nft-metadata/{collection}/{tokenId}"
                , onSuccess, onFailure);
        }
        
        public override void ExitMovingState()
        {
            var props = new NftBlockProperties(Block.GetProps() as NftBlockProperties);
            if (image == null || state != State.Ok) return;
            props.rotation = new SerializableVector3(imageContainer.transform.eulerAngles);
            Block.SetProps(props, land);
            
            if (snackItem != null) SetupDefaultSnack();
            if (scaleRotationController == null) return;
            scaleRotationController.Detach();
            DestroyImmediate(scaleRotationController);
            scaleRotationController = null;
        }
    }
}