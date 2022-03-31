using System;
using src.MetaBlocks.ImageBlock;
using src.Utils;

namespace src.MetaBlocks.NftBlock
{
    [Serializable]
    public class NftBlockProperties : ICloneable
    {
        public FaceProps front;
        public FaceProps back;
        public FaceProps right;
        public FaceProps left;
        public FaceProps top;
        public FaceProps bottom;

        public NftBlockProperties()
        {
        }

        public NftBlockProperties(NftBlockProperties obj)
        {
            if (obj != null)
            {
                left = obj.left;
                right = obj.right;
                top = obj.top;
                bottom = obj.bottom;
                front = obj.front;
                back = obj.back;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || GetType() != obj.GetType())
                return false;
            var prop = obj as NftBlockProperties;
            return Equals(front, prop.front) && Equals(back, prop.back) &&
                   Equals(top, prop.top) && Equals(bottom, prop.bottom) &&
                   Equals(left, prop.left) && Equals(right, prop.right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (front != null ? front.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (back != null ? back.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (right != null ? right.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (left != null ? left.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (top != null ? top.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (bottom != null ? bottom.GetHashCode() : 0);
                return hashCode;
            }
        }

        public object Clone()
        {
            return new NftBlockProperties()
            {
                left = left?.Clone(),
                right = right?.Clone(),
                top = top?.Clone(),
                bottom = bottom?.Clone(),
                front = front?.Clone(),
                back = back?.Clone()
            };
        }

        public FaceProps GetFaceProps(Voxels.Face face)
        {
            if (face == Voxels.Face.BACK) return back;
            if (face == Voxels.Face.FRONT) return front;
            if (face == Voxels.Face.RIGHT) return right;
            if (face == Voxels.Face.LEFT) return left;
            if (face == Voxels.Face.TOP) return top;
            if (face == Voxels.Face.BOTTOM) return bottom;
            return null;
        }

        public void SetFaceProps(Voxels.Face face, FaceProps props)
        {
            if (face == Voxels.Face.BACK) back = props;
            if (face == Voxels.Face.FRONT) front = props;
            if (face == Voxels.Face.RIGHT) right = props;
            if (face == Voxels.Face.LEFT) left = props;
            if (face == Voxels.Face.TOP) top = props;
            if (face == Voxels.Face.BOTTOM) bottom = props;
        }

        public bool IsEmpty()
        {
            return back == null && front == null &&
                   right == null && left == null &&
                   top == null && bottom == null;
        }

        [Serializable]
        public class FaceProps
        {
            public string collection;
            public long tokenId;
            public int width;
            public int height;
            public bool detectCollision = true;

            public override bool Equals(object obj)
            {
                if (obj == this) return true;
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                    return false;
                var prop = obj as FaceProps;
                return Equals(collection, prop.collection) && Equals(tokenId, prop.tokenId) &&
                       Equals(width, prop.width) && Equals(height, prop.height) &&
                       Equals(detectCollision, prop.detectCollision);
            }

            public FaceProps Clone()
            {
                return new FaceProps()
                {
                    collection = collection,
                    tokenId = tokenId,
                    width = width,
                    height = height,
                    detectCollision = detectCollision
                };
            }

            public MediaBlockProperties.FaceProps ToImageFaceProp(string imageUrl)
            {
                return new MediaBlockProperties.FaceProps()
                {
                    url = imageUrl,
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
}