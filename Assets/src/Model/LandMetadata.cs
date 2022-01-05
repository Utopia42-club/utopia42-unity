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

        public LandMetadata(long landId, string imageIpfsKey)
        {
            this.landId = landId;
            this.imageIpfsKey = imageIpfsKey;
        }
    }
}