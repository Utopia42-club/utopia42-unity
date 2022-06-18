using System;
using UnityEngine;

namespace Source.Model
{
    [Serializable]
    public class SerializableVector3
    {
        public static SerializableVector3 Max =>
            new SerializableVector3(float.MaxValue, float.MaxValue, float.MaxValue);

        public static SerializableVector3 Min =>
            new SerializableVector3(float.MinValue, float.MinValue, float.MinValue);

        public static SerializableVector3 Zero => new SerializableVector3(0, 0, 0);
        public static SerializableVector3 One => new SerializableVector3(1, 1, 1);


        public float x;
        public float y;
        public float z;

        public SerializableVector3()
        {
        }

        public SerializableVector3(Vector3 v) : this(v.x, v.y, v.z)
        {
        }

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        
        public Vector3Int ToVector3Int()
        {
            return Vector3Int.FloorToInt(new Vector3(x, y, z));
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
            return new SerializableVector3(x, y, z);
        }
    }
}