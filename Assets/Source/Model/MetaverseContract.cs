using System;

namespace Source.Model
{
    [Serializable]
    public class MetaverseContract
    {
        public String id;
        public long createdAt;
        public String name;
        public NftCollection collection;
        public NetworkData network;

        public override string ToString()
        {
            return $"{name} ({id.Substring(0, 5)}...{id.Substring(id.Length - 5)})";
        }
    }
}