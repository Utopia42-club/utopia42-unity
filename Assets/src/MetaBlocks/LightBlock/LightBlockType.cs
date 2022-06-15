using UnityEngine;

namespace src.MetaBlocks.LightBlock
{
    public class LightBlockType : MetaBlockType
    {
        public LightBlockType(byte id) : base(id, "light", typeof(LightBlockObject), typeof(LightBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            return null;
        }
    }
}