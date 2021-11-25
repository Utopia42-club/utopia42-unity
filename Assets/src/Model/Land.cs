using System;
using UnityEngine;

namespace src.Model
{
    [Serializable]
    public class Land
    {
        public long id;
        public long x1;
        public long y1;
        public long x2;
        public long y2;
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
            return x1 == land.x1 && x2 == land.x2 && y1 == land.y1 && y2 == land.y2;
        }


        public bool Contains(ref Vector3Int position)
        {
            return Contains(position.x, position.z);
        }

        public bool Contains(ref Vector3 position)
        {
            return Contains(position.x, position.z);
        }

        public bool Contains(float x, float z)
        {
            return x1 <= x && x2 >= x
                           && y1 <= z && y2 >= z;
        }
    }
}