using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public abstract class MetaFocusable : MonoBehaviour
    {
        protected MetaBlockObject metaBlockObject;
        protected bool initialized = false;

        public abstract void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face = null);

        public void UnFocus()
        {
            if (!initialized) return;
            metaBlockObject.UnFocus();
        }

        public abstract void Focus();

        public abstract Vector3 GetBlockPosition();
    }
}