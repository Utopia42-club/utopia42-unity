using System;
using UnityEngine;

namespace src.Model
{
    [Serializable]
    public class Land
    {
        public long id;
        public SerializableVector3Int startCoordinate;
        public SerializableVector3Int endCoordinate;
        public string ipfsKey;
        public long time;
        public bool isNft;
        public string owner;
        public long ownerIndex;

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var land = (Land) obj;
            return id == land.id;
        }

        public bool Contains(Vector3 v)
        {
            return startCoordinate.x <= v.x && startCoordinate.z <= v.z &&
                   endCoordinate.x > v.x && endCoordinate.z > v.z;
        }

        public Rect ToRect()
        {
            return new Rect(startCoordinate.x, startCoordinate.z,
                endCoordinate.x - startCoordinate.x,
                endCoordinate.z - startCoordinate.z);
        }
    }
}