using System;
using Source.Model;

namespace Source.MetaBlocks.ImageBlock
{
    [Serializable]
    public class MediaBlockProperties : BaseImageBlockProperties, ICloneable
    {
        public string url;

        public MediaBlockProperties()
        {
        }
        
        public MediaBlockProperties(MediaBlockProperties obj)
        {
            if (obj != null)
            {
                url = obj.url;
                width = obj.width;
                height = obj.height;
                detectCollision = obj.detectCollision;
                rotation = obj.rotation;
            }
        }
        
        public void UpdateProps(MediaBlockProperties props)
        {
            if (props == null) return;
            url = props.url;
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
            var prop = obj as MediaBlockProperties;
            return Equals(url, prop.url) && Equals(width, prop.width) && Equals(height, prop.height) &&
                   Equals(detectCollision, prop.detectCollision) && Equals(rotation, prop.rotation);
        }

        public object Clone()
        {
            return new MediaBlockProperties()
            {
                url = url,
                width = width,
                height = height,
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