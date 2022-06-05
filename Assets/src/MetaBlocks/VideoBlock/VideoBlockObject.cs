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
        private VideoFace video;
        private SnackItem snackItem;

        protected override void Start()
        {
            base.Start();
            gameObject.name = "video block object";
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
            UpdateSnacksAndIconObject();
        }

        private void TogglePlay()
        {
            if (video != null && video.IsPrepared())
                video.TogglePlaying();
            UpdateSnacksAndIconObject();
        }

        private void UpdateSnacksAndIconObject() // TODO [detach metablock] ?
        {
            if (snackItem != null) snackItem.Remove();
            var lines = new List<string>();
            if (video != null)
            {
                if (!video.IsPrepared())
                    lines.Add("Loading Video...");
                else if (video.IsPlaying())
                    lines.Add("Press P to pause");
                else
                    lines.Add("Press P to play");
            }

            if (GetIconObject() != null) // TODO [detach metablock] ?
            {
                GetIconObject().SetActive(true);
                if (video.IsPlaying())
                {
                    GetIconObject().SetActive(false);
                }
            }

            if (!canEdit)
            {
                if (video != null)
                {
                    snackItem = Snack.INSTANCE.ShowLines(lines, () =>
                    {
                        if (Input.GetKeyDown(KeyCode.P))
                            TogglePlay();
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
                        TogglePlay();
                });
            }
        }

        public override void UnFocus()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }
        }

        protected override void OnStateChanged(State state) // TODO [detach metablock] ?
        {
        }

        protected override List<string> GetSnackLines() // TODO [detach metablock] ?
        {
            throw new System.NotImplementedException();
        }

        private void RenderFaces()
        {
            DestroyVideo();
            VideoBlockProperties properties = (VideoBlockProperties) GetBlock().GetProps();
            if (properties != null)
            {
                AddFace(Voxels.Face.BACK, properties);
            }
        }

        private void DestroyVideo(bool immediate = true)
        {
            if (video == null) return;
            var selectable = video.GetComponent<MetaFocusable>();
            if (selectable != null)
                selectable.UnFocus();
            if (immediate)
                DestroyImmediate(video.gameObject);
            else
                Destroy(video.gameObject);
            video = null;
        }

        private void AddFace(Voxels.Face face, VideoBlockProperties props)
        {
            if (props == null) return;

            var transform = gameObject.transform;
            var go = new GameObject();
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.eulerAngles = props.rotation.ToVector3();

            var vidFace = go.AddComponent<VideoFace>();
            var meshRenderer = vidFace.Initialize(face, props.width, props.height);
            if (!InLand(meshRenderer))
            {
                DestroyImmediate(go);
                return;
            }

            vidFace.Init(meshRenderer, props.url, props.previewTime);
            go.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            video = vidFace;
            vidFace.loading.AddListener(l =>
            {
                // if (focused) // TODO [detach metablock] ? 
                    UpdateSnacksAndIconObject();
            });

            var faceSelectable = go.AddComponent<MetaFocusable>();
            faceSelectable.Initialize(this);
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
            DestroyVideo(false);
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