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
        protected readonly List<GameObject> images = new List<GameObject>();
        protected SnackItem snackItem;
        protected int lastFocusedFaceIndex = -1;
        protected Land land;
        protected bool canEdit;
        private bool ready = false;

        protected StateMsg stateMsg = StateMsg.Empty;

        protected void Start()
        {
            canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land);
            if (canEdit)
                CreateIcon();
            ready = true;
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
            RenderFaces();
        }

        public override void Focus(Voxels.Face face)
        {
            if (!canEdit) return;
            if (snackItem != null) snackItem.Remove();

            snackItem = Snack.INSTANCE.ShowLines(GetFaceSnackLines(face), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                    EditProps();
                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new MetaPosition(transform.position));
                if (Input.GetKeyDown(KeyCode.T))
                    GetIconObject().SetActive(!GetIconObject().activeSelf);
            });

            lastFocusedFaceIndex = face.index;
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
            DestroyImages();
            images.Clear();
            MediaBlockProperties properties = (MediaBlockProperties) GetBlock().GetProps();
            if (properties != null)
            {
                AddFace(Voxels.Face.BACK, properties);
            }
        }

        protected void DestroyImages(bool immediate = true)
        {
            foreach (var img in images)
            {
                var selectable = img.GetComponent<MetaFocusable>();
                if (selectable != null)
                    selectable.UnFocus();
                if (immediate)
                    DestroyImmediate(img);
                else
                    Destroy(img);
            }
        }

        protected void AddFace(Voxels.Face face, MediaBlockProperties props)
        {
            if (props == null) return;

            var transform = gameObject.transform;
            var go = new GameObject();
            go.name = "Image game object";
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.eulerAngles = props.rotation.ToVector3();
            
            var imgFace = go.AddComponent<ImageFace>();
            var meshRenderer = imgFace.Initialize(face, props.width, props.height);
            if (!InLand(meshRenderer))
            {
                DestroyImmediate(go);
                UpdateStateAndView(StateMsg.OutOfBound, face);
                return;
            }

            imgFace.Init(meshRenderer, FileService.ResolveUrl(props.url), this, face);
            go.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            images.Add(go);
            var faceSelectable = go.AddComponent<FaceFocusable>();
            faceSelectable.Initialize(this, face);
        }

        public override void UpdateStateAndView(StateMsg msg, Voxels.Face face)
        {
            stateMsg = msg;
            if (snackItem != null && lastFocusedFaceIndex == face.index)
            {
                ((SnackItem.Text) snackItem).UpdateLines(GetFaceSnackLines(face));
            }

            UpdateIcon(msg);
        }

        protected override List<string> GetFaceSnackLines(Voxels.Face face)
        {
            var lines = new List<string>
            {
                "Press Z for details",
                "Press T to toggle preview",
                "Press Del to delete"
            };
            if (stateMsg != StateMsg.Ok)
                lines.Add($"\n{MetaBlockState.ToString(stateMsg, "image")}");
            return lines;
        }

        private void UpdateIcon(StateMsg message) 
        {
            if (message != StateMsg.LoadingMetadata && message != StateMsg.Loading && message != StateMsg.Ok)
            {
                CreateIcon(true);
                return;
            }

            if (canEdit)
                CreateIcon();
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
            DestroyImages(false);
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

        protected override void UpdateState(StateMsg stateMsg)
        {
            throw new System.NotImplementedException();
        }

        public override void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform, Vector3Int localPos, Action<GameObject> onLoad)
        {
        }
    }
}