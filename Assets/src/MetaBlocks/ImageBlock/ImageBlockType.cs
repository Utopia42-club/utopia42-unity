using UnityEngine;

namespace src.MetaBlocks.ImageBlock
{
    public class ImageBlockType : MetaBlockType
    {
        public ImageBlockType(byte id) : base(id, "image", typeof(ImageBlockObject), typeof(MediaBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            ImageBlockObject
                .CreateImageFace(World.INSTANCE.transform, MediaBlockEditor.DEFAULT_DIMENSION,
                    MediaBlockEditor.DEFAULT_DIMENSION, Vector3.zero, out var container, out _, out var renderer, false)
                .PlaceHolderInit(renderer, this, error);
            return container;
        }
    }
}