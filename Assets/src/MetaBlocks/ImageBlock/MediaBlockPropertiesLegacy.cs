using System;

namespace src.MetaBlocks.ImageBlock
{
    [Serializable]
    public class MediaBlockPropertiesLegacy
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
            public string url;
            public int width;
            public int height;
            public bool detectCollision = true;
        }
    }
}