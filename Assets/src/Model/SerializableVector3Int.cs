using System;
using UnityEngine;

namespace src.Model
{
    [Serializable]
    public class SerializableVector3Int
    {
        public static SerializableVector3Int Max =>
            new SerializableVector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

        public static SerializableVector3Int Min =>
            new SerializableVector3Int(int.MinValue, int.MinValue, int.MinValue);

        public static SerializableVector3Int Zero => new SerializableVector3Int(0, 0, 0);
        public static SerializableVector3Int One => new SerializableVector3Int(1, 1, 1);

        public int x;
        public int y;
        public int z;

        public SerializableVector3Int(Vector3Int v) : this(v.x, v.y, v.z)
        {
        }

        public SerializableVector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Int ToVector3()
        {
            return new Vector3Int(x, y, z);
        }

        protected bool Equals(SerializableVector3Int other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SerializableVector3Int) obj);
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

        public SerializableVector3Int Clone()
        {
            return new SerializableVector3Int(x, y, z);
        }
    }
}