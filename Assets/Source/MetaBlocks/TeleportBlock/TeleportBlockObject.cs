using System.Collections.Generic;
using Source.Canvas;
using Source.Model;
using UnityEngine;

namespace Source.MetaBlocks.TeleportBlock
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
                if (!CanEdit) return;
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    if (PropertyEditor.INSTANCE.ReferenceObjectID == GetInstanceID() &&
                        PropertyEditor.INSTANCE.IsActive)
                        PropertyEditor.INSTANCE.Hide();
                    else
                    {
                        EditProps();
                    }
                }
            });
        }

        private void EditProps()
        {
            var editor = new TeleportBlockEditor((value) =>
            {
                Block.SetProps(value, land);
                if (snackItem != null) SetupDefaultSnack();
            }, GetInstanceID());
            editor.SetValue(Block.GetProps() as TeleportBlockProperties);
            editor.Show();
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
            if (!CanEdit) return lines;
            lines.Add("Press Z for details");
            if (Player.INSTANCE.HammerMode)
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
    }
}