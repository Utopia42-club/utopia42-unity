using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectFocusable : MetaFocusable
    {
        public override void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face = null)
        {
            if (Initialized) return;
            this.metaBlockObject = metaBlockObject;
            Initialized = true;
        }

        public override void Focus()
        {
            if (!Initialized) return;
            metaBlockObject.Focus(null);
        }
    }
}