using System;
using System.Collections.Generic;
using src.Canvas;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.ImageBlock
{
    public class ImageBlockObject : MetaBlockObject
    {
        private GameObject image;
        protected SnackItem snackItem;

        protected override void Start()
        {
            base.Start();
            gameObject.name = "image block object";
        }

        public override bool IsReady()
        {
            return ready;
        }

        public override void OnDataUpdate()
        {
            RenderFaces();
        }

        protected override void DoInitialize()
        {
            base.DoInitialize();
            RenderFaces();
        }

        public override void Focus()
        {
            if (!canEdit) return;
            if (snackItem != null) snackItem.Remove();

            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new MetaPosition(transform.position));
                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
            });
        }

        public override void UnFocus()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }
        }

        private void RenderFaces()
        {
            DestroyImage();
            MediaBlockProperties properties = (MediaBlockProperties) GetBlock().GetProps();
            if (properties != null)
            {
                AddFace(Voxels.Face.BACK, properties);
            }
        }

        protected void DestroyImage(bool immediate = true)
        {
            if (image == null) return;
            var selectable = image.GetComponent<MetaFocusable>();
            if (selectable != null)
                selectable.UnFocus();
            if (immediate)
                DestroyImmediate(image);
            else
                Destroy(image);
            image = null;
        }

        protected void AddFace(Voxels.Face face, MediaBlockProperties props)
        {
            if (props == null) return;

            var transform = gameObject.transform;
            var go = new GameObject();
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.eulerAngles = props.rotation.ToVector3();

            var imgFace = go.AddComponent<ImageFace>();
            var meshRenderer = imgFace.Initialize(face, props.width, props.height);
            if (!InLand(meshRenderer))
            {
                DestroyImmediate(go);
                UpdateState(State.OutOfBound);
                return;
            }

            imgFace.Init(meshRenderer, FileService.ResolveUrl(props.url), this, face);
            go.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            image = go;
            var faceSelectable = go.AddComponent<MetaFocusable>();
            faceSelectable.Initialize(this);
        }

        protected override void OnStateChanged(State state)
        {
            if (snackItem != null) // TODO [detach metablock]: && focused?
            {
                ((SnackItem.Text) snackItem).UpdateLines(GetSnackLines());
            }

            // TODO [detach metablock]: update view! (show the green placeholder if the state is ok or loading (metadata)
        }

        protected override List<string> GetSnackLines()
        {
            var lines = new List<string>
            {
                "Press Z for details",
                "Press T to toggle preview",
                "Press Del to delete"
            };
            if (state != State.Ok)
                lines.Add($"\n{MetaBlockState.ToString(state, "image")}");
            return lines;
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Image Block Properties")
                .WithContent(MediaBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<MediaBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue(props as MediaBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new MediaBlockProperties(GetBlock().GetProps() as MediaBlockProperties);

                props.UpdateProps(value);
                if (props.IsEmpty()) props = null;

                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }

        private void OnDestroy()
        {
            DestroyImage(false);
            base.OnDestroy();
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

        public override void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform,
            Vector3Int localPos, Action<GameObject> onLoad)
        {
        }
    }
}