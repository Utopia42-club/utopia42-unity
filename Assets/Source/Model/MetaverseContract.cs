using System;

namespace Source.Model
{
    [Serializable]
    public class MetaverseContract
    {
        public int networkId;
        public string networkName;
        public string address;
        public string networkRpcProvider;
    }
}