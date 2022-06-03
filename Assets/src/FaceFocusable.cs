using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public class FaceFocusable : MetaFocusable
    {
        public void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face)
        {
            if (initialized) return;
            this.metaBlockObject = metaBlockObject;
            this.face = face;
            initialized = true;
        }
    }
}