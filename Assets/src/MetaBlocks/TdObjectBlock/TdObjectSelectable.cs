using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectSelectable : MetaSelectable
    {
        public override void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face = null)
        {
            if(Initialized) return;
            this.metaBlockObject = metaBlockObject;
            Initialized = true;
        }

        public override void Select()
        {
            if(!Initialized) return;
            metaBlockObject.Focus(null);
        }
    }
}