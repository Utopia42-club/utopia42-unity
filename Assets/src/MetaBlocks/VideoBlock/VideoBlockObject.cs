using System;
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
                        EditProps();
                    if (Input.GetButtonDown("Delete"))
                        GetChunk().DeleteMeta(new MetaPosition(transform.localPosition));
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

        public override void UpdateStateAndView(StateMsg msg, Voxels.Face face) // TODO
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
                AddFace(Voxels.Face.BACK, properties);
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

        private void AddFace(Voxels.Face face, VideoBlockProperties props)
        {
            if (props == null) return;

            var transform = gameObject.transform;
            var go = new GameObject();
            go.name = "Video game object";
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.eulerAngles = props.rotation.ToVector3();
            
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

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Video Block Properties")
                .WithContent(VideoBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<VideoBlockEditor>();

            var props = GetBlock().GetProps();
            editor.SetValue(props as VideoBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new VideoBlockProperties(GetBlock().GetProps() as VideoBlockProperties);

                props.UpdateProps(value);
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

        protected override void UpdateState(StateMsg stateMsg)
        {
            throw new System.NotImplementedException();
        }

        public override void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform, Vector3Int localPos, Action<GameObject> onLoad)
        {
        }
    }
}