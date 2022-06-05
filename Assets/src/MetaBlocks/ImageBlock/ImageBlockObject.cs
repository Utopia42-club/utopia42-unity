using System;
using System.Collections.Generic;
using src.Canvas;
using src.Model;
using src.Service;
using UnityEngine;

namespace src.MetaBlocks.ImageBlock
{
    public class ImageBlockObject : MetaBlockObject
    {
        protected GameObject image;
        protected GameObject imageContainer;
        protected SnackItem snackItem;
        protected ObjectScaleRotationController scaleRotationController;

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
            RenderFace();
        }

        protected override void DoInitialize()
        {
            base.DoInitialize();
            RenderFace();
        }

        public override void Focus()
        {
            if (snackItem != null) snackItem.Remove();
            SetupDefaultSnack();
            if (!canEdit) return;
            // TODO [detach metablock]: show highlight
        }

        protected virtual void SetupDefaultSnack()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (!canEdit) return;
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

        protected virtual void RenderFace()
        {
            DestroyImage();
            AddFace((MediaBlockProperties) GetBlock().GetProps());
        }

        protected void DestroyImage(bool immediate = true)
        {
            if (image != null)
            {
                var selectable = image.GetComponent<MetaFocusable>();
                if (selectable != null)
                    selectable.UnFocus();
                if (immediate)
                    DestroyImmediate(image);
                else
                    Destroy(image);
                image = null;
            }

            if (imageContainer != null)
            {
                if (immediate)
                    DestroyImmediate(imageContainer);
                else
                    Destroy(imageContainer);
                imageContainer = null;
            }
        }

        protected void AddFace(MediaBlockProperties props)
        {
            if (props == null)
            {
                UpdateState(State.Empty);
                return;
            }

            var transform = gameObject.transform;

            imageContainer = new GameObject
            {
                name = "image container"
            };
            imageContainer.transform.SetParent(transform, false);
            imageContainer.transform.localPosition = Vector3.zero;
            imageContainer.transform.localScale = new Vector3(props.width, props.height, 1);

            image = new GameObject();
            image.transform.SetParent(imageContainer.transform, false);
            image.transform.localPosition = new Vector3(-0.5f, -0.5f, 0);

            imageContainer.transform.eulerAngles = props.rotation.ToVector3();

            var imgFace = image.AddComponent<ImageFace>();
            var meshRenderer = imgFace.Initialize();
            if (!InLand(meshRenderer))
            {
                UpdateState(State.OutOfBound);
                return;
            }

            imgFace.Init(meshRenderer, FileService.ResolveUrl(props.url), this);
            image.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            var faceSelectable = image.AddComponent<MetaFocusable>();
            faceSelectable.Initialize(this);
        }

        protected override void OnStateChanged(State state)
        {
            if (MetaBlockState.IsErrorState(state))
                DestroyImage();

            if (snackItem != null) // TODO [detach metablock]: && focused?
            {
                ((SnackItem.Text) snackItem).UpdateLines(GetSnackLines());
            }

            // TODO [detach metablock]: update view! (show the green placeholder if the state is ok or loading (metadata)
        }

        protected override List<string> GetSnackLines()
        {
            var lines = new List<string>();
            if (canEdit)
            {
                lines.Add("Press Z for details");
                // if (state == State.Ok)
                lines.Add("Press V to edit rotation");
                lines.Add("Press DEL to delete object");
            }

            var line = MetaBlockState.ToString(state, "image");
            if (line.Length > 0)
                lines.Add("\n" + line);
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
                scaleRotationController.Attach(null, imageContainer.transform);
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
            var props = new MediaBlockProperties(GetBlock().GetProps() as MediaBlockProperties);
            if (image == null || state != State.Ok) return;
            props.rotation = new SerializableVector3(imageContainer.transform.eulerAngles);
            GetBlock().SetProps(props, land);

            SetupDefaultSnack();
            if (scaleRotationController == null) return;
            scaleRotationController.Detach();
            DestroyImmediate(scaleRotationController);
            scaleRotationController = null;
        }
    }
}