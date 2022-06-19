using System;
using System.Collections.Generic;
using Source.Canvas;
using Source.Model;
using Source.Service;
using UnityEngine;

namespace Source.MetaBlocks.ImageBlock
{
    public class ImageBlockObject : MetaBlockObject
    {
        protected GameObject image;
        protected GameObject imageContainer;
        private string currentUrl = "";

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
                if (!canEdit) return;
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    UnFocus();
                    EditProps();
                }

                if (Input.GetButtonDown("Delete"))
                {
                    World.INSTANCE.TryDeleteMeta(new MetaPosition(transform.position));
                }
            });
        }

        protected virtual void RenderFace()
        {
            AddFace((MediaBlockProperties) Block.GetProps());
        }

        protected void DestroyImage(bool immediate = true)
        {
            if (image != null)
            {
                var focusable = image.GetComponent<MetaFocusable>();
                if (focusable != null)
                    focusable.UnFocus();
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

            if (currentUrl.Equals(props.url) && !MetaBlockState.IsErrorState(State) && State != State.Empty)
            {
                Reload(props.width, props.height, props.rotation.ToVector3(), props.detectCollision);
                return;
            }

            currentUrl = props.url;
            DestroyImage();
            var imgFace = CreateImageFace(gameObject.transform, props.width, props.height, props.rotation.ToVector3(),
                out imageContainer, out image, out var meshRenderer, true);

            if (!InLand(meshRenderer))
            {
                UpdateState(State.OutOfBound);
                return;
            }

            imgFace.Init(meshRenderer, FileService.ResolveUrl(props.url), this);
            image.layer = props.detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            image.AddComponent<MetaFocusable>().Initialize(this);
        }

        protected void Reload(int width, int height, Vector3 rotation, bool detectCollision)
        {
            imageContainer.transform.localScale = new Vector3(width, height, 1);
            imageContainer.transform.eulerAngles = rotation;
            image.layer = detectCollision
                ? LayerMask.NameToLayer("Default")
                : LayerMask.NameToLayer("3DColliderOff");
            UpdateState(State);
        }

        internal static ImageFace CreateImageFace(Transform transform, int width, int height, Vector3 rotation,
            out GameObject container, out GameObject image, out MeshRenderer meshRenderer, bool withCollider)
        {
            container = new GameObject
            {
                name = "image container"
            };
            container.transform.SetParent(transform, false);
            container.transform.localPosition = Vector3.zero;
            container.transform.localScale = new Vector3(width, height, 1);

            image = new GameObject();
            image.transform.SetParent(container.transform, false);
            image.transform.localPosition = new Vector3(-0.5f, -0.5f, 0);

            container.transform.eulerAngles = rotation;

            var imgFace = image.AddComponent<ImageFace>();
            meshRenderer = imgFace.Initialize(withCollider);
            return imgFace;
        }

        protected override void OnStateChanged(State state)
        {
            ((SnackItem.Text) snackItem)?.UpdateLines(GetSnackLines());
            var error = MetaBlockState.IsErrorState(state);
            if (!error && state != State.Empty) return;

            DestroyImage();
            var props = (BaseImageBlockProperties) Block.GetProps();
            var rotation = props == null ? Vector3.zero : props.rotation.ToVector3();
            var width = props == null
                ? MediaBlockEditor.DEFAULT_DIMENSION
                : (error ? Math.Min(props.width, MediaBlockEditor.DEFAULT_DIMENSION) : props.width);
            var height = props == null
                ? MediaBlockEditor.DEFAULT_DIMENSION
                : (error ? Math.Min(props.height, MediaBlockEditor.DEFAULT_DIMENSION) : props.height);

            CreateImageFace(gameObject.transform, width, height, rotation,
                out imageContainer, out image, out var r, true).PlaceHolderInit(r, Block.type, error);
            image.AddComponent<MetaFocusable>().Initialize(this);
        }

        protected virtual List<string> GetSnackLines()
        {
            var lines = new List<string>();
            if (canEdit)
            {
                lines.Add("Press Z for details");
                lines.Add("Press DEL to delete object");
            }

            var line = MetaBlockState.ToString(State, "image");
            if (line.Length > 0)
                lines.Add((lines.Count > 0 ? "\n" : "") + line);
            return lines;
        }

        private void EditProps()
        {
            var editor = new MediaBlockEditor((value) =>
            {
                var props = new MediaBlockProperties(Block.GetProps() as MediaBlockProperties);

                props.UpdateProps(value);
                if (props.IsEmpty()) props = null;

                Block.SetProps(props, land);
            });
            editor.SetValue(Block.GetProps() as MediaBlockProperties);
            editor.Show();
        }

        protected override void OnDestroy()
        {
            DestroyImage(false);
            base.OnDestroy();
        }

        public override void ShowFocusHighlight()
        {
            if (image == null) return;
            Player.INSTANCE.RemoveHighlightMesh();
            Player.INSTANCE.focusHighlight = CreateMeshHighlight(World.INSTANCE.HighlightBlock);
        }

        private Transform CreateMeshHighlight(Material material, bool active = true)
        {
            var clone = Instantiate(image.transform, image.transform.parent);
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
            if (image == null) return null;
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
                var props = new MediaBlockProperties(Block.GetProps() as MediaBlockProperties);
                if (image == null) return;
                props.rotation = new SerializableVector3(imageContainer.transform.eulerAngles);
                Block.SetProps(props, land);
            };
            return imageContainer.transform;
        }
    }
}