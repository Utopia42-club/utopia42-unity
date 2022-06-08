using System;
using System.Collections.Generic;
using src.Canvas;
using src.MetaBlocks.ImageBlock;
using src.Model;
using UnityEngine;

namespace src.MetaBlocks.VideoBlock
{
    public class VideoBlockObject : MetaBlockObject
    {
        private VideoFace video;
        private GameObject videoContainer;
        private ObjectScaleRotationController scaleRotationController;

        public override void OnDataUpdate()
        {
            RenderFace();
        }

        protected override void DoInitialize()
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

                if (Input.GetKeyDown(KeyCode.P))
                    TogglePlay();
            });
        }

        private void TogglePlay()
        {
            if (video != null && video.IsPrepared())
                video.TogglePlaying();
            if (snackItem != null) SetupDefaultSnack();
        }

        protected override void OnStateChanged(State state)
        {
            if (snackItem != null) SetupDefaultSnack();
            var error = MetaBlockState.IsErrorState(state);
            if (!error && state != State.Empty) return;

            DestroyVideo();
            CreateVideoFace(gameObject.transform, MediaBlockEditor.DEFAULT_DIMENSION,
                MediaBlockEditor.DEFAULT_DIMENSION, Vector3.zero,
                out videoContainer, out var go, out var renderer, true).PlaceHolderInit(renderer, error);
            go.AddComponent<MetaFocusable>().Initialize(this);
        }

        protected virtual List<string> GetSnackLines()
        {
            var lines = new List<string>();
            if (video != null && video.IsPrepared())
            {
                if (video.IsPlaying())
                    lines.Add("Press P to pause");
                else
                    lines.Add("Press P to play");
            }

            if (canEdit)
            {
                lines.Add("Press Z for details");
                lines.Add("Press V to edit rotation");
                lines.Add("Press Del to delete");
            }

            var line = MetaBlockState.ToString(state, "video");
            if (line.Length > 0 && state != State.Empty && state != State.Ok)
                lines.Add((lines.Count > 0 ? "\n" : "") + line);
            return lines;
        }

        private void RenderFace()
        {
            DestroyVideo();
            AddFace((VideoBlockProperties) Block.GetProps());
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

            video = CreateVideoFace(gameObject.transform, props.width, props.height, props.rotation.ToVector3(),
                out videoContainer, out var go, out var meshRenderer, true);

            if (!InLand(meshRenderer))
            {
                UpdateState(State.OutOfBound);
                return;
            }

            video.Init(meshRenderer, props.url, props.previewTime, this);
            go.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            go.AddComponent<MetaFocusable>().Initialize(this);
        }

        public static VideoFace CreateVideoFace(Transform transform, int width, int height, Vector3 rotation,
            out GameObject container, out GameObject videoGo, out MeshRenderer renderer, bool withCollider)
        {
            container = new GameObject
            {
                name = "video container"
            };
            container.transform.SetParent(transform, false);
            container.transform.localPosition = Vector3.zero;
            container.transform.localScale = new Vector3(width, height, 1);

            videoGo = new GameObject();
            videoGo.transform.SetParent(container.transform, false);
            videoGo.transform.localPosition = new Vector3(-0.5f, -0.5f, 0);

            container.transform.eulerAngles = rotation;


            var face = videoGo.AddComponent<VideoFace>();
            renderer = face.Initialize(withCollider);
            return face;
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Video Block Properties")
                .WithContent(VideoBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<VideoBlockEditor>();

            var props = Block.GetProps();
            editor.SetValue(props as VideoBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new VideoBlockProperties(Block.GetProps() as VideoBlockProperties);

                props.UpdateProps(value);
                if (props.IsEmpty()) props = null;

                Block.SetProps(props, land);
                manager.CloseDialog(dialog);
            });
        }

        protected override void OnDestroy()
        {
            DestroyVideo(false);
            base.OnDestroy();
        }

        public override void ShowFocusHighlight()
        {
            if (video == null) return;
            Player.INSTANCE.RemoveHighlightMesh();
            Player.INSTANCE.tdObjectHighlightMesh = CreateMeshHighlight(World.INSTANCE.HighlightBlock);
        }
        
        private Transform CreateMeshHighlight(Material material, bool active = true)
        {
            var transform = video.transform;
            var clone = Instantiate(transform, transform.parent);
            DestroyImmediate(clone.GetComponent<MeshCollider>());
            var renderer = clone.GetComponent<MeshRenderer>();
            renderer.enabled = active;
            renderer.material = material;
            return clone;
        }
        
        public override void RemoveFocusHighlight()
        {
            Player.INSTANCE.RemoveHighlightMesh();
        }

        public override GameObject CreateSelectHighlight(Transform parent, bool show = true)
        {
            return null;
        }

        public override void LoadSelectHighlight(MetaBlock block, Transform highlightChunkTransform,
            MetaLocalPosition localPos, Action<GameObject> onLoad)
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
            var props = new VideoBlockProperties(Block.GetProps() as VideoBlockProperties);
            if (video == null || state != State.Ok) return;
            props.rotation = new SerializableVector3(videoContainer.transform.eulerAngles);
            Block.SetProps(props, land);

            if (snackItem != null) SetupDefaultSnack();
            if (scaleRotationController == null) return;
            scaleRotationController.Detach();
            DestroyImmediate(scaleRotationController);
            scaleRotationController = null;
        }
    }
}