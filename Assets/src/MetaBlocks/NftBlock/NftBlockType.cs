using src.MetaBlocks.ImageBlock;
using UnityEngine;

namespace src.MetaBlocks.NftBlock
{
    public class NftBlockType : MetaBlockType
    {
        public NftBlockType(byte id) : base(id, "nft", typeof(NftBlockObject), typeof(NftBlockProperties))
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