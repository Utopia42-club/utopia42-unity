using System;

namespace Source.Model
{
    [Serializable]
    public class ConnectionDetail
    {
        public string wallet;
        public int network;
        public string networkName;
        public string networkRpc;
    }
}