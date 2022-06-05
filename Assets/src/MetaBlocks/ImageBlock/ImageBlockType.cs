using UnityEngine;

namespace src.MetaBlocks.ImageBlock
{
    public class ImageBlockType : MetaBlockType
    {
        public ImageBlockType(byte id) : base(id, "image", typeof(ImageBlockObject), typeof(MediaBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error)
        {
            return null;
        }
    }
}
