using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectFocusable : MetaFocusable
    {
        public override void Initialize(MetaBlockObject metaBlockObject, Voxels.Face face = null)
        {
            if (initialized) return;
            this.metaBlockObject = metaBlockObject;
            initialized = true;
        }

        public override void Focus()
        {
            if (!initialized) return;

            if (BlockSelectionController.INSTANCE.SelectionActive)
            {
                Player.INSTANCE.ShowTdObjectHighlight(metaBlockObject as TdObjectBlockObject);
                return;
            }
            metaBlockObject.Focus(null);
        }

        public override Vector3 GetBlockPosition()
        {
            return Vectors.TruncateFloor(transform.parent.parent.position);
        }
    }
}