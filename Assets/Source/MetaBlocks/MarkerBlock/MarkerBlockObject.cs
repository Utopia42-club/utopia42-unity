using System.Collections.Generic;
using Source.Canvas;
using UnityEngine;

namespace Source.MetaBlocks.MarkerBlock
{
    public class MarkerBlockObject : MetaBlockObject
    {
        private GameObject placeHolder;

        protected override void OnDestroy()
        {
            DestroyPlaceHolder(false);
            base.OnDestroy();
        }

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

            if (!CanEdit) return;
            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (Input.GetKeyDown(KeyCode.E))
                    TryOpenEditor(EditProps);
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
            var lines = new List<string>();
            lines.Add("Press E for details");
            if (Player.INSTANCE.HammerMode)
                lines.Add("Press DEL to delete object");
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

        private void EditProps()
        {
            var editor = new MarkerBlockEditor(value =>
            {
                var props = new MarkerBlockProperties(Block.GetProps() as MarkerBlockProperties);
                if (value != null)
                    props.name = value.name;
                if (props.IsEmpty()) props = null;
                Block.SetProps(props, land);
            }, GetInstanceID());
            editor.SetValue(Block.GetProps() as MarkerBlockProperties);
            editor.Show();
        }

        private void DestroyPlaceHolder(bool immediate = true)
        {
            if (placeHolder == null) return;
            DeepDestroy3DObject(placeHolder, immediate);
            placeHolder = null;
        }
    }
}