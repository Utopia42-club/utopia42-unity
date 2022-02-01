using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectFocusable : MetaFocusable
    {
        public override void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face = null)
        {
            if (initialized) return;
            this.MetaBlockObject = metaBlockObject;
            initialized = true;
        }

        public override void Focus()
        {
            if (!initialized) return;
            MetaBlockObject.Focus(null);
        }

        public override Vector3 GetBlockPosition()
        {
            return transform.parent.parent.position;
        }
    }
}