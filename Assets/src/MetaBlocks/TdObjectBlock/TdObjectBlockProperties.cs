using System;
using src.Model;

namespace src.MetaBlocks.TdObjectBlock
{
    [Serializable]
    public class TdObjectBlockProperties : ICloneable
    {
        public string url;

        public SerializableVector3 scale = SerializableVector3.One;
        public SerializableVector3 offset = SerializableVector3.Zero;
        public SerializableVector3 rotation = SerializableVector3.Zero;

        public SerializableVector3 initialPosition = SerializableVector3.Zero;
        public float initialScale = 0;

        public bool detectCollision = true;

        public TdObjectBlockProperties()
        {
        }

        public TdObjectBlockProperties(TdObjectBlockProperties obj)
        {
            if (obj != null)
            {
                url = obj.url;
                scale = obj.scale;
                offset = obj.offset;
                rotation = obj.rotation;
                initialPosition = obj.initialPosition;
                initialScale = obj.initialScale;
                detectCollision = obj.detectCollision;
            }
        }

        public void UpdateProps(TdObjectBlockProperties props)
        {
            if (props == null) return;
            this.url = props.url;
            this.scale = props.scale;
            this.offset = props.offset;
            this.rotation = props.rotation;
            this.detectCollision = props.detectCollision;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as TdObjectBlockProperties;
            return Equals(url, prop.url) && Equals(scale, prop.scale) && Equals(offset, prop.offset) &&
                   Equals(rotation, prop.rotation)
                   && Equals(initialPosition, prop.initialPosition) && Equals(initialScale, prop.initialScale) &&
                   Equals(detectCollision, prop.detectCollision);
        }

        public object Clone()
        {
            return new TdObjectBlockProperties()
            {
                url = url,
                scale = scale.Clone(),
                offset = offset.Clone(),
                rotation = rotation.Clone(),
                initialPosition = initialPosition.Clone(),
                initialScale = initialScale,
                detectCollision = detectCollision
            };
        }

        public bool IsEmpty()
        {
            return url == null || url.Equals("");
        }
    }
}