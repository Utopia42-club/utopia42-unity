using System;

namespace Source.Model
{
    [Serializable]
    public class LandMetadata
    {
        public int network;
        public string contract;
        public long landId;
        public string description;
        public string imageIpfsKey;

        public LandMetadata(int network, string contract, long landId, string imageIpfsKey)
        {
            this.network = network;
            this.contract = contract;
            this.landId = landId;
            this.imageIpfsKey = imageIpfsKey;
        }
    }
}