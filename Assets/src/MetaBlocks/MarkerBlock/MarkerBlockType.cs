using Siccity.GLTFUtility;
using src.Model;
using UnityEngine;

namespace src.MetaBlocks.MarkerBlock
{
    public class MarkerBlockType : MetaBlockType
    {
        private const float LocalScale = 0.6f;
        public MarkerBlockType(byte id) : base(id, "marker", typeof(MarkerBlockObject), typeof(MarkerBlockProperties), true)
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            return Create3dPlaceHolder(!error ? "marker" : "3d_object_error", "marker placeholder", withCollider,
                LocalScale);
        }

        public override MetaPosition GetPlaceHolderPutPosition(Vector3 purePosition)
        {
            return new MetaPosition(Player.INSTANCE.PossibleHighlightBlockPosInt + 0.5f * Vector3.one);
        }
    }
}