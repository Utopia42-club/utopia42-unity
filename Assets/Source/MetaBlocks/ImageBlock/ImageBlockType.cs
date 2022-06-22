using Source.Model;
using UnityEngine;

namespace Source.MetaBlocks.ImageBlock
{
    public class ImageBlockType : MetaBlockType
    {
        public const float Gap = 0.2f;
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

        public override MetaPosition GetPlaceHolderPutPosition(Vector3 purePosition)
        {
            var pos = Player.INSTANCE.Forward.z > 0
                ? purePosition - Gap * Vector3.forward
                : purePosition + Gap * Vector3.forward;
            pos += 0.5f * MediaBlockEditor.DEFAULT_DIMENSION * Vector3.up;
            return new MetaPosition(pos);
        }
    }
}