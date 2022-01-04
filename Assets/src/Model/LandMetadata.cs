using System;
using System.Collections.Generic;

namespace src.Model
{
    [System.Serializable]
    public class LandMetadata
    {
        public string landId;
        public string name;
        public string description;
        public string image;

        public override int GetHashCode()
        {
            return landId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var other = (LandMetadata)obj;
            return landId == other.landId;
        }

        public static LandMetadata CreateLandMetadata(Land land, string imageIpfsKey)
        {
            var md = new LandMetadata();
            md.image = imageIpfsKey;
            md.name = "Land #" + land.id;
            md.landId = land.id.ToString();
            md.description =
                $"x1: {land.x1}, x2: {land.x2}\ny1: {land.y1}, y2: {land.y2}\nSize: {Math.Abs(land.x2 - land.x1) * Math.Abs(land.y2 - land.y1)}";
            return md;
        }
    }
}
