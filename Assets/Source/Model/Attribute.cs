using System;
using Newtonsoft.Json;

namespace Source.Model
{
    [Serializable]
    public class Attribute
    {
        [JsonProperty("trait_type")] public string traitType;
        public string value;
    }
}