using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public class FaceFocusable : MetaFocusable
    {
        public void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face = null)
        {
            if (initialized) return;
            this.metaBlockObject = metaBlockObject;
            this.face = face;
            initialized = true;
        }

        public override Vector3 GetBlockPosition()
        {
            return Vectors.TruncateFloor(transform.parent.position);
        }
    }
}