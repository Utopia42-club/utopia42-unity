using System;
using System.Collections.Generic;
using src.Canvas;
using src.Model;
using UnityEngine;

namespace src.MetaBlocks.VideoBlock
{
    public class VideoBlockObject : MetaBlockObject
    {
        private VideoFace video;
        private GameObject videoContainer;
        private SnackItem snackItem;
        private ObjectScaleRotationController scaleRotationController;

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
            RenderFace();
        }

        protected override void DoInitialize()
        {
            base.DoInitialize();
            RenderFace();
        }

        public override void Focus()
        {
            if (!canEdit) return;
            if (snackItem != null) snackItem.Remove();

            SetupDefaultSnack();
        }

        private void SetupDefaultSnack()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (canEdit)
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                        EditProps();
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

                if (Input.GetKeyDown(KeyCode.P))
                    TogglePlay();
            });
        }

        private void TogglePlay()
        {
            if (video != null && video.IsPrepared())
                video.TogglePlaying();
            SetupDefaultSnack();
        }

        public override void UnFocus()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }
        }

        protected override void OnStateChanged(State state)
        {
            if (MetaBlockState.IsErrorState(state))
                DestroyVideo();

            // if (snackItem != null) // TODO [detach metablock]: && focused?
            // {
            //     ((SnackItem.Text) snackItem).UpdateLines(GetSnackLines());
            // }

            // TODO [detach metablock]: update view! (show the green placeholder if the state is ok or loading (metadata)
        }

        protected override List<string> GetSnackLines()
        {
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

            if (!canEdit) return lines;
            lines.Add("Press Z for details");
            lines.Add("Press V to edit rotation");
            lines.Add("Press Del to delete");
            return lines;
        }

        private void RenderFace()
        {
            DestroyVideo();
            AddFace((VideoBlockProperties) GetBlock().GetProps());
        }

        private void DestroyVideo(bool immediate = true)
        {
            if (video != null)
            {
                var selectable = video.GetComponent<MetaFocusable>();
                if (selectable != null)
                    selectable.UnFocus();
                if (immediate)
                    DestroyImmediate(video.gameObject);
                else
                    Destroy(video.gameObject);
                video = null;
            }

            if (videoContainer != null)
            {
                if (immediate)
                    DestroyImmediate(videoContainer);
                else
                    Destroy(videoContainer);
                videoContainer = null;
            }
        }

        private void AddFace(VideoBlockProperties props)
        {
            if (props == null)
            {
                UpdateState(State.Empty);
                return;
            }

            var transform = gameObject.transform;

            videoContainer = new GameObject
            {
                name = "video container"
            };
            videoContainer.transform.SetParent(transform, false);
            videoContainer.transform.localPosition = Vector3.zero;
            videoContainer.transform.localScale = new Vector3(props.width, props.height, 1);

            var go = new GameObject();
            go.transform.SetParent(videoContainer.transform, false);
            go.transform.localPosition = new Vector3(-0.5f, -0.5f, 0);

            videoContainer.transform.eulerAngles = props.rotation.ToVector3();


            video = go.AddComponent<VideoFace>();
            var meshRenderer = video.Initialize();
            if (!InLand(meshRenderer))
            {
                UpdateState(State.OutOfBound);
                return;
            }

            video.Init(meshRenderer, props.url, props.previewTime);
            go.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            video.loading.AddListener(l =>
            {
                // if (focused) // TODO [detach metablock] ? 
                // SetupDefaultSnack();
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

        public override void SetToMovingState()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            if (scaleRotationController == null)
            {
                scaleRotationController = gameObject.AddComponent<ObjectScaleRotationController>();
                scaleRotationController.Attach(null, videoContainer.transform);
            }

            snackItem = Snack.INSTANCE.ShowLines(scaleRotationController.EditModeSnackLines, () =>
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    GameManager.INSTANCE.ToggleMovingObjectState(this);
                }
            });
        }

        public override void ExitMovingState()
        {
            var props = new VideoBlockProperties(GetBlock().GetProps() as VideoBlockProperties);
            if (video == null || state != State.Ok) return;
            props.rotation = new SerializableVector3(videoContainer.transform.eulerAngles);
            GetBlock().SetProps(props, land);

            SetupDefaultSnack();
            if (scaleRotationController == null) return;
            scaleRotationController.Detach();
            DestroyImmediate(scaleRotationController);
            scaleRotationController = null;
        }
    }
}