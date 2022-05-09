using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public abstract class MetaFocusable : MonoBehaviour
    {
        public MetaBlockObject metaBlockObject { protected set; get; }
        protected bool initialized = false;
        protected Voxels.Face face;

        public void UnFocus()
        {
            if (!initialized) return;
            metaBlockObject.UnFocus();
        }

        public void Focus()
        {
            if (!initialized) return;

            if (BlockSelectionController.INSTANCE.SelectionActive)
            {
                metaBlockObject.ShowFocusHighlight();
                return;
            }
            metaBlockObject.Focus(face);
        }

        public abstract Vector3 GetBlockPosition();
    }
}