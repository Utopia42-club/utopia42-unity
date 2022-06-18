using System;
using Source.MetaBlocks;
using Source.MetaBlocks.NftBlock;
using Source.Model;

namespace Source.Service.Migration.Models
{
    [Serializable]
    public class NftBlockPropertiesLegacy : BaseImageBlockProperties<NftFaceProps>
    {
    }

    [Serializable]
    public class NftFaceProps: MetaBlockFaceProperties
    {
        public string collection;
        public long tokenId;
        public override BaseImageBlockProperties toProperties(SerializableVector3 rotation)
        {
            var nft = new NftBlockProperties()
            {
                collection = collection,
                tokenId = tokenId,
                height = height,
                width = width,
                detectCollision = detectCollision,
                rotation = rotation
            };
            return nft;
        }
    }
}