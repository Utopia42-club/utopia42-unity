using System;
using System.Collections.Generic;
using UnityEngine;

namespace Source.Model
{
    [Serializable]
    public class LandDetails
    {
        public string v;
        public string wallet;
        public Dictionary<string, MetaBlockData> metadata;   
        public Dictionary<string, Block> changes;
        public LandProperties properties;

        public static Vector3Int ParseIntKey(string key)
        {
            var coords = key.Split('_');
            return new Vector3Int(int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2]));
        }
        
        public static MetaLocalPosition ParseKey(string key)
        {
            var coords = key.Split('_');
            return new MetaLocalPosition(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
        }

        public static string FormatIntKey(Vector3Int pos)
        {
            return $"{pos.x}_{pos.y}_{pos.z}";
        }
        
        public static string FormatKey(Vector3 pos)
        {
            return $"{pos.x:0.0}_{pos.y:0.0}_{pos.z:0.0}";
        }
    }
}