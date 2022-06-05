using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
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