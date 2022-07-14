using System;
using System.Collections.Generic;
using Source.Canvas;
using Source.Model;
using UnityEngine;

namespace Source.MetaBlocks.LinkBlock
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
            if (CanEdit)
            {
                lines.Add("Press E for details");
                if (Player.INSTANCE.HammerMode)
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

        protected override void SetupDefaultSnack()
        {
            if (snackItem != null) snackItem.Remove();
            var props = (LinkBlockProperties) Block.GetProps();
            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (CanEdit && Input.GetKeyDown(KeyCode.E))
                    TryOpenEditor(EditProps);
                
                if (props != null && !props.IsEmpty() && Input.GetKeyDown(KeyCode.O))
                    OpenLink();
            });
        }

        private void EditProps()
        {
            var editor = new LinkBlockEditor((value) =>
            {
                if (value.pos != null) value.url = null;
                if (value.IsEmpty()) value = null;
                Block.SetProps(value, land);
                if (snackItem != null) SetupDefaultSnack();
            }, GetInstanceID());
            editor.SetValue(Block.GetProps() as LinkBlockProperties);
            editor.Show();
        }

        private void DestroyPlaceHolder(bool immediate = true)
        {
            if (placeHolder == null) return;
            DeepDestroy3DObject(placeHolder, immediate);
            placeHolder = null;
        }

        protected override void OnDestroy()
        {
            DestroyPlaceHolder(false);
            base.OnDestroy();
        }
    }
}