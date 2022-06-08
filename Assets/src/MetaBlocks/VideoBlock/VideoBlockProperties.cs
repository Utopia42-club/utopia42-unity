using System;
using src.Model;
using src.Utils;

namespace src.MetaBlocks.VideoBlock
{
    [Serializable]
    public class VideoBlockProperties : BaseImageBlockProperties, ICloneable
    {
        public string url;
        public float previewTime = 0;

        public VideoBlockProperties()
        {
        }
        
        public VideoBlockProperties(VideoBlockProperties obj)
        {
            if (obj != null)
            {
                url = obj.url;
                width = obj.width;
                height = obj.height;
                previewTime = obj.previewTime;
                detectCollision = obj.detectCollision;
                rotation = obj.rotation;
            }
        }
        
        public void UpdateProps(VideoBlockProperties props)
        {
            if (props == null) return;
            url = props.url;
            width = props.width;
            height = props.height;
            previewTime = props.previewTime;
            detectCollision = props.detectCollision;
            rotation = props.rotation;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as VideoBlockProperties;
            return Equals(url, prop.url) && Equals(width, prop.width) && Equals(height, prop.height) &&
                   Equals(detectCollision, prop.detectCollision) && Equals(previewTime, prop.previewTime) && Equals(rotation, prop.rotation);
        }

        public object Clone()
        {
            return new VideoBlockProperties()
            {
                url = url,
                width = width,
                height = height,
                previewTime = previewTime,
                detectCollision = detectCollision,
                rotation = rotation.Clone()
            };
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(url);
        }
    }
}