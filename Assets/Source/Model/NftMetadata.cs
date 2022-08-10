using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Source.Model
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
}