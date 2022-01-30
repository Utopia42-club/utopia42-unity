using System;
using src.Utils;

namespace src.MetaBlocks.VideoBlock
{
    [System.Serializable]
    public class VideoBlockProperties : ICloneable
    {
        public FaceProps front;
        public FaceProps back;
        public FaceProps right;
        public FaceProps left;
        public FaceProps top;
        public FaceProps bottom;

        public VideoBlockProperties()
        {
        }

        public VideoBlockProperties(VideoBlockProperties obj)
        {
            if (obj != null)
            {
                left = obj.left;
                right = obj.right;
                top = obj.right;
                bottom = obj.bottom;
                front = obj.front;
                back = obj.back;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as VideoBlockProperties;
            return Equals(front, prop.front) && Equals(back, prop.back) &&
                   Equals(top, prop.top) && Equals(bottom, prop.bottom) &&
                   Equals(left, prop.left) && Equals(right, prop.right);
        }

        public object Clone()
        {
            return new VideoBlockProperties()
            {
                left = left.Clone(),
                right = right.Clone(),
                top = top.Clone(),
                bottom = bottom.Clone(),
                front = front.Clone(),
                back = back.Clone()
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

        [System.Serializable]
        public class FaceProps
        {
            public string url;
            public int width;
            public int height;
            public float previewTime = 0;

            public override bool Equals(object obj)
            {
                if (obj == this) return true;
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                    return false;
                var prop = obj as FaceProps;
                return Equals(url, prop.url) && Equals(width, prop.width) && Equals(height, prop.height)
                       && Equals(previewTime, prop.previewTime);
            }

            public FaceProps Clone()
            {
                return new FaceProps()
                {
                    url = url,
                    width = width,
                    height = height,
                    previewTime = previewTime
                };
            }
        }
    }
}