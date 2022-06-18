using Source.Utils;
using UnityEngine;

namespace Source.MetaBlocks.TdObjectBlock
{
    public class TdObjectFocusable : MetaFocusable
    {
        public void Initialize(MetaBlockObject metaBlockObject)
        {
            if (initialized) return;
            this.MetaBlockObject = metaBlockObject;
            initialized = true;
        }
    }
}