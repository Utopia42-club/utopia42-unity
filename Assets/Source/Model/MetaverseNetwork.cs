using System;
using System.Collections.Generic;

namespace Source.Model
{
    [Serializable]
    public class MetaverseNetwork
    {
        public int networkId;
        public string networkName;
        public List<string> contracts;
    }
}