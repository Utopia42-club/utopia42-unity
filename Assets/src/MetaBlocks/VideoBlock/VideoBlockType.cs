using UnityEngine;

namespace src.MetaBlocks.VideoBlock
{
    public class VideoBlockType : MetaBlockType
    {
        public VideoBlockType(byte id) : base(id, "video", typeof(VideoBlockObject), typeof(VideoBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder()
        {
            return null;
        }
    }
}
