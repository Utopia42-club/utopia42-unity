using System;
using System.Collections.Generic;
using src.Canvas;
using src.Model;
using UnityEngine;

namespace src.MetaBlocks.MarkerBlock
{
    public class MarkerBlockObject : MetaBlockObject
    {
        private GameObject placeHolder;

        public override void OnDataUpdate()
        {
        }

        protected override void DoInitialize()
        {
            UpdateState(State.Empty);
        }

        protected override void SetupDefaultSnack()
        {
            if (snackItem != null)
            {
                snackItem.Remove();
                snackItem = null;
            }

            if (!canEdit) return;
            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    RemoveFocusHighlight();
                    EditProps();
                }

                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new MetaPosition(transform.localPosition));
            });
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
            return new List<string>
            {
                "Press Z for details",
                "Press DEL to delete object"
            };
        }

        public override void ShowFocusHighlight()
        {
            if (placeHolder == null) return;
            AdjustHighlightBox(Player.INSTANCE.tdObjectHighlightBox, placeHolder.GetComponent<BoxCollider>(), true);
        }

        public override void RemoveFocusHighlight()
        {
            Player.INSTANCE.tdObjectHighlightBox.gameObject.SetActive(false);
        }

        public override GameObject CreateSelectHighlight(Transform parent, bool show = true)
        {
            if (placeHolder == null) return null;
            Transform highlight;
            highlight = Instantiate(Player.INSTANCE.tdObjectHighlightBox, default, Quaternion.identity);
            highlight.GetComponentInChildren<MeshRenderer>().material = World.INSTANCE.SelectedBlock;
            AdjustHighlightBox(highlight, placeHolder.GetComponent<BoxCollider>(), show);
            highlight.SetParent(parent, true);
            var go = highlight.gameObject;
            go.name = "link highlight";
            return go;
        }

        public override void SetToMovingState()
        {
            throw new NotImplementedException();
        }

        public override void ExitMovingState()
        {
            throw new NotImplementedException();
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Marker Block Properties")
                .WithContent(MarkerBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<MarkerBlockEditor>();

            var props = Block.GetProps();
            editor.SetValue(props == null ? null : props as MarkerBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                var props = new MarkerBlockProperties(Block.GetProps() as MarkerBlockProperties);
                if (value != null)
                    props.name = value.name;
                if (props.IsEmpty()) props = null;
                Block.SetProps(props, land);
                manager.CloseDialog(dialog);
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