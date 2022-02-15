using System;
using System.Collections.Generic;
using UnityEngine;

namespace src.Model
{
    [Serializable]
    public class LandDetails
    {
        public string v;
        public string wallet;
        public Dictionary<string, MetaBlock> metadata;
        public Dictionary<string, Block> changes;

        public static Vector3Int PraseKey(string key)
        {
            var coords = key.Split('_');
            return new Vector3Int(int.Parse(coords[0]), int.Parse(coords[1]), int.Parse(coords[2]));
        }

        public static string FormatKey(Vector3Int pos)
        {
            return string.Format("{0}_{1}_{2}", pos.x, pos.y, pos.z);
        }
    }
}