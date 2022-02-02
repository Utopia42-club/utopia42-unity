using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public class FaceFocusable : MetaFocusable
    {
        private Voxels.Face face;

        public override void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face = null)
        {
            if (initialized) return;
            this.metaBlockObject = metaBlockObject;
            this.face = face;
            initialized = true;
        }

        public override void Focus()
        {
            if (!initialized) return;
            metaBlockObject.Focus(face);
        }

        public override Vector3 GetBlockPosition()
        {
            return transform.parent.position;
        }
    }
}