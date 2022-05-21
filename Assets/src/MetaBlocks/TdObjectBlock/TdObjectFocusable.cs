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
            var collider = GetComponent<Collider>();
            if(collider != null && collider is BoxCollider)
                return Vectors.TruncateFloor(transform.parent.parent.position);
            return Vectors.TruncateFloor(transform.parent.parent.parent.position);
        }
    }
}