using System.Collections.Generic;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.ImageBlock
{
    public class ImageBlockObject : MetaBlockObject
    {
        private readonly List<GameObject> images = new List<GameObject>();
        private SnackItem snackItem;
        private int lastFocusedFaceIndex = -1;
        private Land land;
        private bool canEdit;
        private bool ready = false;

        private readonly StateMsg[] stateMsg =
            {StateMsg.Ok, StateMsg.Ok, StateMsg.Ok, StateMsg.Ok, StateMsg.Ok, StateMsg.Ok};

        private void Start()
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

            snackItem = Snack.INSTANCE.ShowLines(GetFaceSnackLines(face.index), () =>
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

        private List<string> GetFaceSnackLines(int faceIndex)
        {
            var lines = new List<string>();
            lines.Add("Press Z for details");
            lines.Add("Press T to toggle preview");
            lines.Add("Press Del to delete");
            if (stateMsg[faceIndex] != StateMsg.Ok)
                lines.Add($"\n{MetaBlockState.ToString(stateMsg[faceIndex], "image")}");
            return lines;
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
            foreach (var img in images)
                DestroyImmediate(img);
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

        private void AddFace(Voxels.Face face, MediaBlockProperties.FaceProps props)
        {
            if (props == null) return;

            var go = new GameObject();
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero + ((Vector3) face.direction) * 0.1f;
            var imgFace = go.AddComponent<ImageFace>();
            var meshRenderer = imgFace.Initialize(face, props.width, props.height);
            if (!InLand(meshRenderer))
            {
                DestroyImmediate(meshRenderer);
                DestroyImmediate(imgFace);
                UpdateStateAndIcon(face.index, StateMsg.OutOfBound);
                return;
            }

            imgFace.Init(meshRenderer, props.url, this, face.index);
            images.Add(go);
            var faceSelectable = imgFace.gameObject.AddComponent<FaceSelectable>();
            faceSelectable.Initialize(this, face);
        }

        public void UpdateStateAndIcon(int faceIndex, StateMsg msg)
        {
            stateMsg[faceIndex] = msg;
            if (snackItem != null && lastFocusedFaceIndex == faceIndex)
            {
                ((SnackItem.Text) snackItem).UpdateLines(GetFaceSnackLines(faceIndex));
            }

            UpdateIcon(msg);
        }

        private void UpdateIcon(StateMsg message) // TODO
        {
            if (message != StateMsg.Loading && message != StateMsg.Ok)
            {
                CreateIcon(true);
                return;
            }

            foreach (var msg in stateMsg)
            {
                if (msg != StateMsg.Loading && msg != StateMsg.Ok)
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
    }
}