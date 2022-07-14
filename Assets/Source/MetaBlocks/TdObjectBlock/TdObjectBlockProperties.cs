using System;
using Source.Model;

namespace Source.MetaBlocks.TdObjectBlock
{
    [Serializable]
    public class TdObjectBlockProperties : ICloneable
    {
        public string url;

        public SerializableVector3 scale = SerializableVector3.One;
        public SerializableVector3 rotation = SerializableVector3.Zero;

        public SerializableVector3 initialPosition = SerializableVector3.Zero;
        public float initialScale = 0;

        public bool detectCollision = true;
        public TdObjectType type = TdObjectType.OBJ;
        
        public TdObjectBlockProperties()
        {
        }

        public TdObjectBlockProperties(TdObjectBlockProperties obj)
        {
            if (obj != null)
            {
                url = obj.url;
                scale = obj.scale;
                rotation = obj.rotation;
                initialPosition = obj.initialPosition;
                initialScale = obj.initialScale;
                detectCollision = obj.detectCollision;
                type = obj.type;
            }
        }

        public void UpdateProps(TdObjectBlockProperties props)
        {
            if (props == null) return;
            url = props.url;
            scale = props.scale;
            rotation = props.rotation;
            detectCollision = props.detectCollision;
            type = props.type;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as TdObjectBlockProperties;
            return Equals(url, prop.url) && Equals(scale, prop.scale) && Equals(rotation, prop.rotation)
                   && Equals(initialPosition, prop.initialPosition) && Equals(initialScale, prop.initialScale) &&
                   Equals(detectCollision, prop.detectCollision) && Equals(type, prop.type);
        }

        public object Clone()
        {
            return new TdObjectBlockProperties()
            {
                url = url,
                scale = scale.Clone(),
                rotation = rotation.Clone(),
                initialPosition = initialPosition.Clone(),
                initialScale = initialScale,
                detectCollision = detectCollision,
                type = type
            };
        }

        public bool IsEmpty()
        {
            return url == null || url.Equals("");
        }

        public enum TdObjectType
        {
            OBJ, GLB
        }
    }
}