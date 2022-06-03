using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public abstract class MetaFocusable : Focusable
    {
        public MetaBlockObject metaBlockObject { protected set; get; }
        protected Voxels.Face face;

        public override void UnFocus()
        {
            if (!initialized) return;
            metaBlockObject.UnFocus();
        }

        public override void Focus(Vector3? point = null)
        {
            if (!initialized) return;

            if (World.INSTANCE.SelectionActive)
            {
                metaBlockObject.ShowFocusHighlight();
                return;
            }
            metaBlockObject.Focus(face);
        }
        
        public override Vector3? GetBlockPosition()
        {
            return metaBlockObject.transform.position;
        }
    }
}