using System;
using src.MetaBlocks.ImageBlock;
using src.Model;

namespace src.MetaBlocks.NftBlock
{
    [Serializable]
    public class NftBlockProperties : BaseImageBlockProperties, ICloneable
    {
        public string collection;
        public long tokenId;

        public NftBlockProperties()
        {
        }

        public NftBlockProperties(NftBlockProperties obj)
        {
            if (obj != null)
            {
                collection = obj.collection;
                tokenId = obj.tokenId;
                width = obj.width;
                height = obj.height;
                detectCollision = obj.detectCollision;
                rotation = obj.rotation;
            }
        }

        public void UpdateProps(NftBlockProperties props)
        {
            if (props == null) return;
            collection = props.collection;
            tokenId = props.tokenId;
            width = props.width;
            height = props.height;
            detectCollision = props.detectCollision;
            rotation = props.rotation;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as NftBlockProperties;
            return Equals(collection, prop.collection) && Equals(tokenId, prop.tokenId) && Equals(width, prop.width) &&
                   Equals(height, prop.height) &&
                   Equals(detectCollision, prop.detectCollision) && Equals(rotation, prop.rotation);
        }

        public object Clone()
        {
            return new NftBlockProperties()
            {
                collection = collection,
                tokenId = tokenId,
                width = width,
                height = height,
                detectCollision = detectCollision,
                rotation = rotation.Clone()
            };
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(collection) || tokenId < 1;
        }

        public MediaBlockProperties ToImageProp(string imageUrl)
        {
            return new MediaBlockProperties()
            {
                url = imageUrl,
                rotation = rotation.Clone(),
                width = width,
                height = height,
                detectCollision = detectCollision,
            };
        }

        public string GetOpenseaUrl()
        {
            return $"https://opensea.io/assets/matic/{collection}/{tokenId}";
        }
    }
}