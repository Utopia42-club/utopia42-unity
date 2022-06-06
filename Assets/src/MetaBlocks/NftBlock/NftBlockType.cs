using src.MetaBlocks.ImageBlock;
using src.Model;
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
        
        public override MetaPosition GetPutPosition(Vector3 purePosition, Vector3 playerForward)
        {
            var pos = playerForward.z > 0
                ? purePosition - 0.2f * Vector3.forward
                : purePosition + 0.2f * Vector3.forward;
            pos += 0.5f * MediaBlockEditor.DEFAULT_DIMENSION * Vector3.up;
            return new MetaPosition(pos);
        }
    }
}