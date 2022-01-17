using src.MetaBlocks;
using src.Utils;
using UnityEngine;

namespace src
{
    public class FaceSelectable : MetaSelectable
    {
        private Voxels.Face face;

        public override void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face = null)
        {
            if (Initialized) return;
            this.metaBlockObject = metaBlockObject;
            this.face = face;
            Initialized = true;
        }

        public override void Select()
        {
            if (!Initialized) return;
            metaBlockObject.Focus(face);
        }
    }
}