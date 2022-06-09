using System;
using System.Collections.Generic;
using src.Canvas;
using src.Model;
using src.Service;
using UnityEngine;

namespace src.MetaBlocks.TeleportBlock
{
    public class TeleportBlockObject : MetaBlockObject
    {
        protected GameObject portal;
        protected ObjectScaleRotationController scaleRotationController;

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
        }

        protected virtual void RenderPortal()
        { 
            if (portal != null)
                Destroy(portal);
            portal = Instantiate(Resources.Load<GameObject>(TeleportBlockType.PORTAL_PREFAB), World.INSTANCE.transform);
            portal.transform.SetParent(transform, false);
            portal.transform.localPosition = Vector3.zero;
            portal.transform.localScale = new Vector3(2, 2, 2);
        }

        protected override void OnStateChanged(State state)
        {
            
        }

        protected virtual List<string> GetSnackLines()
        {
            var lines = new List<string>();
            return lines;
        }
        
        protected override void OnDestroy()
        {
            if (portal != null)
                Destroy(portal);
            base.OnDestroy();
        }

        public override void ShowFocusHighlight()
        {
            if (portal == null) return;
            // Player.INSTANCE.RemoveHighlightMesh();
            // Player.INSTANCE.tdObjectHighlightMesh = CreateMeshHighlight(World.INSTANCE.HighlightBlock);
        }

        public override void RemoveFocusHighlight()
        {
            Player.INSTANCE.RemoveHighlightMesh();
        }

        public override GameObject CreateSelectHighlight(Transform parent, bool show = true)
        {
            return null;
        }

        public override void SetToMovingState()
        {
        }

        public override void ExitMovingState()
        {
        }
    }
}