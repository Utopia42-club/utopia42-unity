using System.Collections.Generic;
using src.Canvas;
using src.Model;
using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.VideoBlock
{
    public class VideoBlockObject : MetaBlockObject
    {
        private readonly Dictionary<Voxels.Face, VideoFace> videos = new Dictionary<Voxels.Face, VideoFace>();
        private SnackItem snackItem;
        private Land land;
        private bool canEdit;
        private Voxels.Face focusedFace;
        private bool ready = false;

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
            focusedFace = face;
            UpdateSnacksAndIconObject(face);
        }

        private void TogglePlay(Voxels.Face face)
        {
            VideoFace video;
            if (videos.TryGetValue(face, out video) && video.IsPrepared())
                video.TogglePlaying();
            UpdateSnacksAndIconObject(face);
        }

        private void UpdateSnacksAndIconObject(Voxels.Face face)
        {
            if (snackItem != null) snackItem.Remove();
            var lines = new List<string>();
            VideoFace video;
            if (videos.TryGetValue(face, out video))
            {
                if (!video.IsPrepared())
                    lines.Add("Loading Video...");
                else if (video.IsPlaying())
                    lines.Add("Press P to pause");
                else
                    lines.Add("Press P to play");
            }

            if (GetIconObject() != null)
            {
                GetIconObject().SetActive(true);
                foreach (var vid in videos.Values)
                {
                    if (vid.IsPlaying())
                    {
                        GetIconObject().SetActive(false);
                        break;
                    }
                }
            }

            if (!canEdit)
            {
                if (video != null)
                {
                    snackItem = Snack.INSTANCE.ShowLines(lines, () =>
                    {
                        if (Input.GetKeyDown(KeyCode.P))
                            TogglePlay(face);
                    });
                }
            }
            else
            {
                lines.Add("Press Z for details");
                lines.Add("Press T to toggle preview");
                lines.Add("Press Del to delete");
                snackItem = Snack.INSTANCE.ShowLines(lines, () =>
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                        EditProps(face);
                    if (Input.GetButtonDown("Delete"))
                        GetChunk().DeleteMeta(new VoxelPosition(transform.localPosition));
                    if (Input.GetKeyDown(KeyCode.T))
                        GetIconObject().SetActive(!GetIconObject().activeSelf);
                    if (Input.GetKeyDown(KeyCode.P))
                        TogglePlay(face);
                });
            }
        }

        public override void UnFocus()
        {
            focusedFace = null;
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }
        }

        public override void UpdateStateAndIcon(StateMsg msg, Voxels.Face face) // TODO
        {
            throw new System.NotImplementedException();
        }

        protected override List<string> GetFaceSnackLines(Voxels.Face face) // TODO
        {
            throw new System.NotImplementedException();
        }

        private void RenderFaces()
        {
            DestroyVideos();
            videos.Clear();

            VideoBlockProperties properties = (VideoBlockProperties) GetBlock().GetProps();
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

        private void DestroyVideos(bool immediate = true)
        {
            foreach (var vid in videos.Values)
            {
                var selectable = vid.GetComponent<MetaFocusable>();
                if (selectable != null)
                    selectable.UnFocus();
                if(immediate)
                    DestroyImmediate(vid.gameObject);
                else
                    Destroy(vid.gameObject);
            }
        }

        private void AddFace(Voxels.Face face, VideoBlockProperties.FaceProps props)
        {
            if (props == null) return;

            var go = new GameObject();
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero + ((Vector3) face.direction) * 0.1f;
            var vidFace = go.AddComponent<VideoFace>();
            var meshRenderer = vidFace.Initialize(face, props.width, props.height);
            if (!InLand(meshRenderer))
            {
                DestroyImmediate(go);
                CreateIcon(true);
                return;
            }

            vidFace.Init(meshRenderer, props.url, props.previewTime);
            go.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            videos[face] = vidFace;
            vidFace.loading.AddListener(l =>
            {
                if (focusedFace == face) UpdateSnacksAndIconObject(face);
            });

            var faceSelectable = go.AddComponent<FaceFocusable>();
            faceSelectable.Initialize(this, face);
        }

        private void EditProps(Voxels.Face face)
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Video Block Properties")
                .WithContent(VideoBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<VideoBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue(props == null ? null : (props as VideoBlockProperties).GetFaceProps(face));
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new VideoBlockProperties(GetBlock().GetProps() as VideoBlockProperties);

                props.SetFaceProps(face, value);
                if (props.IsEmpty()) props = null;

                GetBlock().SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }
        
        private void OnDestroy()
        {
            DestroyVideos(false);
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
    }
}