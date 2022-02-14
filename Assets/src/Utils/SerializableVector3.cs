using UnityEngine;

namespace src.Utils
{
    [System.Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public static SerializableVector3 From(Vector3 vector3)
        {
            return new SerializableVector3
            {
                x = vector3.x,
                y = vector3.y,
                z = vector3.z
            };
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
            if (obj.GetType() != GetType()) return false;
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

        public SerializableVector3 Clone()
        {
            return new SerializableVector3
            {
                x = x,
                y = y,
                z = z
            };
        }
    }
}