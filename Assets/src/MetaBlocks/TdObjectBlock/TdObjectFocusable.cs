using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectFocusable : MetaFocusable
    {
        public void Initialize(MetaBlockObject metaBlockObject)
        {
            if (initialized) return;
            this.metaBlockObject = metaBlockObject;
            initialized = true;
        }

        public override Vector3 GetBlockPosition()
        {
            return Vectors.TruncateFloor(transform.parent.parent.position);
        }
    }
}