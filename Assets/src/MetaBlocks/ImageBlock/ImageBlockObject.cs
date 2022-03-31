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

        protected readonly StateMsg[] stateMsg =
            {StateMsg.Ok, StateMsg.Ok, StateMsg.Ok, StateMsg.Ok, StateMsg.Ok, StateMsg.Ok};

        protected void Start()
        {
            if (canEdit = Player.INSTANCE.CanEdit(Vectors.FloorToInt(transform.position), out land))
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
                    EditProps(face);
                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
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
                AddFace(Voxels.Face.BACK, properties.back);
                AddFace(Voxels.Face.FRONT, properties.front);
                AddFace(Voxels.Face.RIGHT, properties.right);
                AddFace(Voxels.Face.LEFT, properties.left);
                AddFace(Voxels.Face.TOP, properties.top);
                AddFace(Voxels.Face.BOTTOM, properties.bottom);
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

        protected void AddFace(Voxels.Face face, MediaBlockProperties.FaceProps props)
        {
            if (props == null) return;

            var go = new GameObject();
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero + ((Vector3) face.direction) * 0.1f;
            var imgFace = go.AddComponent<ImageFace>();
            var meshRenderer = imgFace.Initialize(face, props.width, props.height);
            if (!InLand(meshRenderer))
            {
                DestroyImmediate(go);
                UpdateStateAndIcon(StateMsg.OutOfBound, face);
                return;
            }

            imgFace.Init(meshRenderer, FileService.resolveUrl(props.url), this, face);
            go.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            images.Add(go);
            var faceSelectable = go.AddComponent<FaceFocusable>();
            faceSelectable.Initialize(this, face);
        }

        public override void UpdateStateAndIcon(StateMsg msg, Voxels.Face face)
        {
            stateMsg[face.index] = msg;
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
            if (stateMsg[face.index] != StateMsg.Ok)
                lines.Add($"\n{MetaBlockState.ToString(stateMsg[face.index], "image")}");
            return lines;
        }

        private void UpdateIcon(StateMsg message) // TODO
        {
            if (message != StateMsg.LoadingMetadata && message != StateMsg.Loading && message != StateMsg.Ok)
            {
                CreateIcon(true);
                return;
            }

            foreach (var msg in stateMsg)
            {
                if (message != StateMsg.LoadingMetadata && msg != StateMsg.Loading && msg != StateMsg.Ok)
                {
                    CreateIcon(true);
                    return;
                }
            }

            CreateIcon();
        }

        private void EditProps(Voxels.Face face)
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Image Block Properties")
                .WithContent(MediaBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<MediaBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue(props == null ? null : (props as MediaBlockProperties).GetFaceProps(face));
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new MediaBlockProperties(GetBlock().GetProps() as MediaBlockProperties);

                props.SetFaceProps(face, value);
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
    }
}