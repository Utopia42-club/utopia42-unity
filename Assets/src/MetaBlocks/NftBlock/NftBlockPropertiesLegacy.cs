using System;

namespace src.MetaBlocks.NftBlock
{
    [Serializable]
    public class NftBlockPropertiesLegacy
    {
        public FaceProps front;
        public FaceProps back;
        public FaceProps right;
        public FaceProps left;
        public FaceProps top;
        public FaceProps bottom;

        [Serializable]
        public class FaceProps
        {
            public string collection;
            public long tokenId;
            public int width;
            public int height;
            public bool detectCollision = true;
        }
    }
}