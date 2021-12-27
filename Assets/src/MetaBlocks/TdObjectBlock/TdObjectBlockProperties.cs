using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    [System.Serializable]
    public class TdObjectBlockProperties
    {
        public string url;
        public SerializableVector3 scale = SerializableVector3.from(Vector3.one);
        public SerializableVector3 offset = SerializableVector3.from(Vector3.zero);
        public SerializableVector3 rotation = SerializableVector3.from(Vector3.zero);

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
            }
        }
        
        public void UpdateProps(TdObjectBlockProperties props)
        {
            if(props == null) return;
            this.url = props.url;
            this.scale = props.scale;
            this.offset = props.offset;
            this.rotation = props.rotation;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as TdObjectBlockProperties;
            return Equals(url, prop.url) && Equals(scale, prop.scale) && Equals(offset, prop.offset) && Equals(rotation, prop.rotation);
        }

        public bool IsEmpty()
        {
            return url == null || url.Equals("");
        }
    }

    [System.Serializable]
    public class SerializableVector3
    {
        
        public float x;
        public float y;
        public float z;

        public static SerializableVector3 from(Vector3 vector3)
        {
            var v = new SerializableVector3();
            v.x = vector3.x;
            v.y = vector3.y;
            v.z = vector3.z;
            return v;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        protected bool Equals(SerializableVector3 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SerializableVector3) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                hashCode = (hashCode * 397) ^ z.GetHashCode();
                return hashCode;
            }
        }
    }
}
