using System;
using System.Collections.Generic;
using src.Canvas;
using src.Model;
using UnityEngine;

namespace src.MetaBlocks.LinkBlock
{
    public class LinkBlockObject : MetaBlockObject
    {
        private GameObject placeHolder;
        public override void OnDataUpdate()
        {
        }

        protected override void DoInitialize()
        {
            UpdateState(State.Empty);
        }

        private void OpenLink()
        {
            LinkBlockProperties props = (LinkBlockProperties) Block.GetProps();
            if (props.pos == null)
                Application.OpenURL(props.url);
            else
                GameManager.INSTANCE.MovePlayerTo(new Vector3(props.pos[0], props.pos[1], props.pos[2]));
        }

        protected override void OnStateChanged(State state)
        {
            if (state != State.Empty) return; // only empty state is valid for marker metablock
            if (snackItem != null) SetupDefaultSnack();

            // setting place holder
            DestroyPlaceHolder();
            placeHolder = Block.type.CreatePlaceHolder(false, true);
            placeHolder.transform.SetParent(gameObject.transform, false);
            placeHolder.SetActive(true);
            placeHolder.GetComponentInChildren<Collider>()
                .gameObject.AddComponent<MetaFocusable>()
                .Initialize(this);
        }

        protected virtual List<string> GetSnackLines()
        {
            var lines = new List<string>();
            if (canEdit)
            {
                lines.Add("Press Z for details");
                lines.Add("Press Del to delete");
            }

            var props = (LinkBlockProperties) Block.GetProps();
            if (props != null && !props.IsEmpty())
            {
                if (props.pos == null)
                    lines.Add("Press O to open in web");
                else
                    lines.Add("Press O to transport");
            }

            return lines;
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
            MetaLocalPosition localPos, Action<GameObject> onLoad)
        {
        }

        public override void SetToMovingState()
        {
            throw new NotImplementedException();
        }

        public override void ExitMovingState()
        {
            throw new NotImplementedException();
        }

        protected override void SetupDefaultSnack()
        {
            if (snackItem != null) snackItem.Remove();
            var props = (LinkBlockProperties) Block.GetProps();
            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (canEdit)
                {
                    if (Input.GetKeyDown(KeyCode.Z))
                        EditProps();
                    if (Input.GetButtonDown("Delete"))
                        GetChunk().DeleteMeta(new MetaPosition(transform.localPosition));
                }

                if (props != null && !props.IsEmpty() && Input.GetKeyDown(KeyCode.O))
                    OpenLink();
            });
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Link Block Properties")
                .WithContent(LinkBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<LinkBlockEditor>();

            var props = Block.GetProps();
            editor.SetValue(props as LinkBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                if (value.pos != null) value.url = null;
                if (value.IsEmpty()) value = null;
                Block.SetProps(value, land);
                manager.CloseDialog(dialog);
                if (snackItem != null) SetupDefaultSnack();
            });
        }
        
        private void DestroyPlaceHolder(bool immediate = true)
        {
            if (placeHolder == null) return;
            foreach (var renderer in placeHolder.GetComponentsInChildren<Renderer>())
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat == null) continue;
                if (immediate)
                {
                    DestroyImmediate(mat.mainTexture);
                    if (!mat.Equals(World.INSTANCE.SelectedBlock) && !mat.Equals(World.INSTANCE.HighlightBlock))
                        DestroyImmediate(mat);
                }
                else
                {
                    Destroy(mat.mainTexture);
                    if (!mat.Equals(World.INSTANCE.SelectedBlock) && !mat.Equals(World.INSTANCE.HighlightBlock))
                        Destroy(mat);
                }
            }

            foreach (var meshFilter in placeHolder.GetComponentsInChildren<MeshFilter>())
            {
                if (immediate)
                    DestroyImmediate(meshFilter.sharedMesh);
                else
                    Destroy(meshFilter.sharedMesh);
            }


            if (immediate)
                DestroyImmediate(placeHolder.gameObject);
            else
                Destroy(placeHolder.gameObject);

            placeHolder = null;
        }
        
        protected override void OnDestroy()
        {
            DestroyPlaceHolder(false);
            base.OnDestroy();
        }
    }
}