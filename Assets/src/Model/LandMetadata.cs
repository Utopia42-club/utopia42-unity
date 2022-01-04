using System;
using System.Collections.Generic;

namespace src.Model
{
    [System.Serializable]
    public class LandMetadata
    {
        public long landId;
        public string description;
        public string imageIpfsKey;
        public long centerX;
        public long centerY;
        public long width;
        public long height;
        public long area;
        
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
            md.landId = land.id;
            md.imageIpfsKey = imageIpfsKey;
            // md.description = "";

            var width = Math.Abs(land.x2 - land.x1);
            var height = Math.Abs(land.y2 - land.y1);
            
            md.area = height * width;
            md.width = width;
            md.height = height;

            md.centerX = (long) Math.Floor(0.5 * (land.x1 + land.x2));
            md.centerY = (long) Math.Floor(0.5 * (land.y1 + land.y2));

            return md;
        }
    }
}
