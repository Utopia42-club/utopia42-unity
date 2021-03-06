using System;
using System.Collections.Generic;
using Source.Canvas;
using Source.MetaBlocks.ImageBlock;
using Source.Model;
using UnityEngine;

namespace Source.MetaBlocks.VideoBlock
{
    public class VideoBlockObject : MetaBlockObject
    {
        private VideoFace video;
        private GameObject videoContainer;
        private string currentUrl = "";
        private float currentPreviewTime = 0;

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
                if (CanEdit && Input.GetKeyDown(KeyCode.E))
                    TryOpenEditor(EditProps);

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
            var props = (VideoBlockProperties) Block.GetProps();

            var rotation = props == null ? Vector3.zero : props.rotation.ToVector3();
            var width = props == null
                ? MediaBlockEditor.DEFAULT_DIMENSION
                : (error ? Math.Min(props.width, MediaBlockEditor.DEFAULT_DIMENSION) : props.width);
            var height = props == null
                ? MediaBlockEditor.DEFAULT_DIMENSION
                : (error ? Math.Min(props.height, MediaBlockEditor.DEFAULT_DIMENSION) : props.height);

            video = CreateVideoFace(gameObject.transform, width, height, rotation,
                out videoContainer, out var go, out var r, true);
            video.PlaceHolderInit(r, error);
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

            if (CanEdit)
            {
                lines.Add("Press E for details");
                if (Player.INSTANCE.HammerMode)
                    lines.Add("Press Del to delete");
            }

            var line = MetaBlockState.ToString(State, "video");
            if (line.Length > 0 && State != State.Empty && State != State.Ok)
                lines.Add((lines.Count > 0 ? "\n" : "") + line);
            return lines;
        }

        private void RenderFace()
        {
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

            if (currentUrl.Equals(props.url) && Math.Abs(currentPreviewTime - props.previewTime) < 0.001 &&
                !MetaBlockState.IsErrorState(State) && State != State.Empty)
            {
                Reload(props.width, props.height, props.rotation.ToVector3(), props.detectCollision);
                return;
            }

            currentUrl = props.url;
            currentPreviewTime = props.previewTime;
            DestroyVideo();
            video = CreateVideoFace(gameObject.transform, props.width, props.height, props.rotation.ToVector3(),
                out videoContainer, out var go, out var meshRenderer, true);

            if (ExceedsBoundaries)
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

        private void Reload(int width, int height, Vector3 rotation, bool detectCollision)
        {
            videoContainer.transform.localScale = new Vector3(width, height, 1);
            videoContainer.transform.eulerAngles = rotation;
            video.gameObject.layer = detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            UpdateState(State);
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
            var editor = new VideoBlockEditor((value) =>
            {
                var props = new VideoBlockProperties(Block.GetProps() as VideoBlockProperties);

                props.UpdateProps(value);
                if (props.IsEmpty()) props = null;

                Block.SetProps(props, land);
            }, GetInstanceID());
            editor.SetValue(Block.GetProps() as VideoBlockProperties);
            editor.Show();
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
            Player.INSTANCE.focusHighlight = CreateMeshHighlight(World.INSTANCE.HighlightBlock);
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
            if (video == null) return null;
            var highlight = CreateMeshHighlight(World.INSTANCE.SelectedBlock, show);
            highlight.SetParent(parent, true);
            var go = highlight.gameObject;
            go.name = "image highlight";
            return go;
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
                var props = new VideoBlockProperties(Block.GetProps() as VideoBlockProperties);
                if (video == null) return;
                props.rotation = new SerializableVector3(videoContainer.transform.eulerAngles);
                Block.SetProps(props, land);
            };
            return videoContainer.transform;
        }
    }
}