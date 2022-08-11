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
            var idStr = id.Length > 20 ? $"{id.Substring(0, 10)}...{id.Substring(id.Length - 10)}" : id;
            return $"{name} ({idStr})";
        }
    }
}