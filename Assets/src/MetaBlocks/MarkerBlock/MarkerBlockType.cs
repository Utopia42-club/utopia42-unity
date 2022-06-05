using UnityEngine;

namespace src.MetaBlocks.MarkerBlock
{
    public class MarkerBlockType : MetaBlockType
    {
        public MarkerBlockType(byte id) : base(id, "marker", typeof(MarkerBlockObject), typeof(MarkerBlockProperties), true)
        {
        }

        public override GameObject CreatePlaceHolder(bool error)
        {
            return null;
        }
    }
}