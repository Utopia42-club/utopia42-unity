using System;
using System.Collections;
using System.Collections.Generic;
using Source.Canvas;
using Source.Configuration;
using Source.MetaBlocks.ImageBlock;
using Source.Model;
using Source.Service;
using Source.Utils;
using UnityEngine;

namespace Source.MetaBlocks.NftBlock
{
    public class NftBlockObject : ImageBlockObject
    {
        private NftMetadata metadata = new NftMetadata();
        private string currentCollection = "";
        private long currentTokenId = 0;

        public override void OnDataUpdate()
        {
            RenderFace();
        }

        protected override void SetupDefaultSnack()
        {
            if (snackItem != null) snackItem.Remove();

            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (CanEdit && Input.GetKeyDown(KeyCode.E))
                    TryOpenEditor(EditProps);

                if (Input.GetKeyDown(KeyCode.O))
                    OpenLink();
            });
        }

        protected override List<string> GetSnackLines()
        {
            var lines = new List<string>();
            if (CanEdit)
            {
                lines.Add("Press E for details");
                if (Player.INSTANCE.HammerMode)
                    lines.Add("Press Del to delete");
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

            if (State != State.Ok)
            {
                var msg = MetaBlockState.ToString(State, "image");
                if (msg.Length > 0)
                    lines.Add($"\n{msg}");
            }

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

            if (currentCollection.Equals(props.collection) && currentTokenId == props.tokenId &&
                !MetaBlockState.IsErrorState(State) && State != State.Empty)
            {
                Reload(props.width, props.height, props.rotation.ToVector3(), props.detectCollision);
                return;
            }

            currentCollection = props.collection;
            currentTokenId = props.tokenId;

            UpdateState(State.LoadingMetadata);
            StartCoroutine(GetMetadata(props.collection, props.tokenId, md =>
            {
                metadata = md;
                // var imageUrl = $"{Configuration.Instance.ApiURL}/nft-metadata/image/{props.collection}/{props.tokenId}";
                var imageUrl = string.IsNullOrWhiteSpace(md.image) ? md.imageUrl : md.image;
                AddFace(props.ToImageProp(imageUrl));
            }, () => { UpdateState(State.ConnectionError); }));
        }

        private void EditProps()
        {
            var editor = new NftBlockEditor((value) =>
            {
                var props = new NftBlockProperties(Block.GetProps() as NftBlockProperties);

                props.UpdateProps(value);
                if (props.IsEmpty()) props = null;

                Block.SetProps(props, land);
            }, GetInstanceID());
            editor.SetValue(Block.GetProps() as NftBlockProperties);
            editor.Show();
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
            yield return RestClient.Get($"{Configurations.Instance.apiURL}/nft-metadata/{collection}/{tokenId}"
                , onSuccess, onFailure);
        }

        public override Transform GetRotationTarget(out Action afterRotated)
        {
            if (MetaBlockState.IsErrorState(State) || State == State.Empty)
            {
                afterRotated = null;
                return null;
            }

            afterRotated = () =>
            {
                var props = new NftBlockProperties(Block.GetProps() as NftBlockProperties);
                if (image == null) return;
                props.rotation = new SerializableVector3(imageContainer.transform.eulerAngles);
                Block.SetProps(props, land);
            };
            return imageContainer.transform;
        }
    }
}