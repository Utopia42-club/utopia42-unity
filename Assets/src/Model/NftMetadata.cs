using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace src.Model
{
    [Serializable]
    public class NftMetadata
    {
        public string id;
        public string name;
        public string description;
        public string language;
        public string image;
        [JsonProperty("image_url")] public string imageUrl;
        public string thumbnail;
        public List<Attribute> attributes;
    }

    [Serializable]
    public class Attribute
    {
        [JsonProperty("trait_type")] public string traitType;
        public string value;
    }
}