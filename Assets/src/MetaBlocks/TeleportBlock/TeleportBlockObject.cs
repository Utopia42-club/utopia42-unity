using System.Collections.Generic;
using src.Canvas;
using src.Model;
using UnityEngine;

namespace src.MetaBlocks.TeleportBlock
{
    public class TeleportBlockObject : MetaBlockObject
    {
        private GameObject portal;

        public override void OnDataUpdate()
        {
            RenderPortal();
        }

        protected override void DoInitialize()
        {
            RenderPortal();
        }

        protected override void SetupDefaultSnack()
        {
            if (snackItem != null) snackItem.Remove();
            var props = (TeleportBlockProperties) Block.GetProps();
            snackItem = Snack.INSTANCE.ShowLines(GetSnackLines(), () =>
            {
                if (!canEdit) return;
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    UnFocus();
                    EditProps();
                }

                if (Input.GetButtonDown("Delete"))
                    GetChunk().DeleteMeta(new MetaPosition(transform.localPosition));
            });
        }

        private void EditProps()
        {
            var manager = GameManager.INSTANCE;
            var dialog = manager.OpenDialog();
            dialog
                .WithTitle("Teleport Block Properties")
                .WithContent(TeleportBlockEditor.PREFAB);
            var editor = dialog.GetContent().GetComponent<TeleportBlockEditor>();

            var props = Block.GetProps();
            editor.SetValue(props as TeleportBlockProperties);
            dialog.WithAction("OK", () =>
            {
                var value = editor.GetValue();
                Block.SetProps(value, land);
                manager.CloseDialog(dialog);
                if (snackItem != null) SetupDefaultSnack();
            });
        }

        protected virtual void RenderPortal()
        {
            if (portal != null)
                Destroy(portal);
            portal = CreatePortal(transform, true);
            portal.GetComponent<CapsuleCollider>().gameObject.AddComponent<MetaFocusable>().Initialize(this);
        }

        internal static GameObject CreatePortal(Transform parent, bool withCollider)
        {
            var portal = Instantiate(Resources.Load<GameObject>(TeleportBlockType.PortalPrefab), parent);
            portal.transform.localPosition = Vector3.zero;
            portal.transform.localScale = TeleportBlockType.LocalScale * Vector3.one;
            if (!withCollider)
            {
                DestroyImmediate(portal.GetComponent<Collider>());
            }

            return portal;
        }

        protected override void OnStateChanged(State state)
        {
        }

        protected virtual List<string> GetSnackLines()
        {
            var lines = new List<string>();
            if (!canEdit) return lines;
            lines.Add("Press Z for details");
            lines.Add("Press Del to delete");
            return lines;
        }

        protected override void OnDestroy()
        {
            DestroyPortal(false);
            base.OnDestroy();
        }

        private void DestroyPortal(bool immediate = true)
        {
            if (portal != null)
            {
                var focusable = portal.GetComponent<MetaFocusable>();
                if (focusable != null)
                    focusable.UnFocus();
                if (immediate)
                    DestroyImmediate(portal);
                else
                    Destroy(portal);
                portal = null;
            }
        }

        public override void ShowFocusHighlight()
        {
            if (portal == null) return;
            Player.INSTANCE.RemoveHighlightMesh();
            Player.INSTANCE.focusHighlight =
                CreateMeshHighlight(World.INSTANCE.HighlightBlock, "teleport focus highlight");
        }

        private Transform CreateMeshHighlight(Material material, string objectName, bool active = true)
        {
            var clone = Instantiate(portal.transform, portal.transform.parent);
            DestroyImmediate(clone.GetComponent<Collider>());
            var r = clone.GetComponent<MeshRenderer>();
            r.enabled = active;
            r.material = material;
            clone.localScale *= 1.01f;
            clone.name = objectName;
            return clone;
        }

        public override void RemoveFocusHighlight()
        {
            Player.INSTANCE.RemoveHighlightMesh();
        }

        public override GameObject CreateSelectHighlight(Transform parent, bool show = true)
        {
            if (portal == null) return null;
            var highlight = CreateMeshHighlight(World.INSTANCE.SelectedBlock, "portal select highlight", show);
            highlight.SetParent(parent, true);
            return highlight.gameObject;
        }

        public override void SetToMovingState()
        {
        }

        public override void ExitMovingState()
        {
        }
    }
}