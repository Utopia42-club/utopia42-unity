using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Source.Model;
using UnityEngine;

namespace Source.Utils
{
    public static class ColorBlocks
    {
        private static readonly ColorCache Cache = new ColorCache(1024);
        public static readonly string PLAYER_COLOR_BLOCKS = "PLAYER_COLOR_BLOCKS";

        public static bool IsColorBlockType(string colorBlockTypeName, out BlockType blockType)
        {
            if (!colorBlockTypeName.StartsWith("#"))
            {
                blockType = null;
                return false;
            }

            colorBlockTypeName = colorBlockTypeName.ToLower();
            if (Cache.TryGet(colorBlockTypeName, out blockType)) return true;

            if (colorBlockTypeName.Length != 7 || !ColorUtility.TryParseHtmlString(colorBlockTypeName, out var color))
            {
                Debug.LogError("Invalid block color: " + colorBlockTypeName);
                blockType = null;
                return true;
            }

            blockType = GetBlockTypeFromColor(color, colorBlockTypeName, false);
            Cache.Add(blockType);
            return true;
        }

        public static BlockType GetBlockTypeFromColor(Color32 color, string name = null, bool tryCache = true)
        {
            if (tryCache && Cache.TryGet(color, out var blockType)) return blockType;
            var id = GetTypeIdFromColor(color);
            if (name == null)
            {
                name = "#" + ColorUtility.ToHtmlStringRGB(color);
            }

            blockType = new BlockType(id, color, name);
            Cache.Add(blockType);
            return blockType;
        }

        public static Color32 GetColorFromBlockType(BlockType blockType)
        {
            return blockType.color ?? Color.white;
        }

        public static bool IsColorTypeId(uint id)
        {
            var bytes = BitConverter.GetBytes(id);
            return bytes[3] == 1;
        }

        public static bool IsColorTypeId(uint id, out BlockType blockType)
        {
            var bytes = BitConverter.GetBytes(id);
            if (bytes[3] != 1)
            {
                blockType = null;
                return false;
            }

            if (Cache.TryGet(id, out blockType)) return true;

            var color = new Color32(bytes[2], bytes[1], bytes[0], 255);
            blockType = GetBlockTypeFromColor(color);
            Cache.Add(blockType);
            return true;
        }

        private static uint GetTypeIdFromColor(Color32 color)
        {
            return (uint) (color.b + (color.g << 8) + (color.r << 16) + (1 << 24));
        }

        public static void SaveBlockColor(Color color)
        {
            var colorBlocks = GetPlayerColorBlocks();
            
            colorBlocks.Add(GetTypeIdFromColor(color));
            SetPlayerColorBlocks(colorBlocks);
        }

        public static void RemoveBlockColorFromSaving(Color color)
        {
            var colorBlocks = GetPlayerColorBlocks();
            var id = GetTypeIdFromColor(color);
            colorBlocks = colorBlocks.Where(c => c != id).ToList();
            SetPlayerColorBlocks(colorBlocks);
        }

        public static void SetPlayerColorBlocks(IEnumerable colorBlocks)
        {
            PlayerPrefs.SetString(PLAYER_COLOR_BLOCKS, JsonConvert.SerializeObject(colorBlocks));
        }

        public static List<uint> GetPlayerColorBlocks()
        {
            try
            {
                var deserializeObject =
                    JsonConvert.DeserializeObject<List<uint>>(PlayerPrefs.GetString(PLAYER_COLOR_BLOCKS, "[]"));
                return new HashSet<uint>(deserializeObject).ToList();
            }
            catch (JsonSerializationException e)
            {
                return new List<uint>();
            }
        }

        private class ColorCache
        {
            private readonly int maxCacheSize = 0;
            private readonly ConcurrentQueue<BlockType> types = new ConcurrentQueue<BlockType>();

            private readonly ConcurrentDictionary<uint, BlockType> idToTypes =
                new ConcurrentDictionary<uint, BlockType>();

            private readonly ConcurrentDictionary<Color32, BlockType> colorToTypes =
                new ConcurrentDictionary<Color32, BlockType>();

            private readonly ConcurrentDictionary<string, BlockType> nameToTypes =
                new ConcurrentDictionary<string, BlockType>();

            public ColorCache(int maxCacheSize)
            {
                this.maxCacheSize = maxCacheSize;
            }

            public bool TryGet(string name, out BlockType blockType)
            {
                return nameToTypes.TryGetValue(name, out blockType);
            }

            public bool TryGet(uint id, out BlockType blockType)
            {
                return idToTypes.TryGetValue(id, out blockType);
            }

            public bool TryGet(Color32 color, out BlockType blockType)
            {
                return colorToTypes.TryGetValue(color, out blockType);
            }

            public void Add(BlockType blockType)
            {
                if (blockType.color == null)
                {
                    Debug.LogError("Invalid color blockType. Failed to update cache.");
                    return;
                }

                if (types.Count > maxCacheSize) RemoveOlderHalf();
                if (idToTypes.TryAdd(blockType.id, blockType) &&
                    colorToTypes.TryAdd(blockType.color.Value, blockType) &&
                    nameToTypes.TryAdd(blockType.name, blockType))
                    types.Enqueue(blockType);
            }

            private void RemoveOldest()
            {
                if (!types.TryDequeue(out var type)) return;
                idToTypes.TryRemove(type.id, out _);
                colorToTypes.TryRemove(type.color.Value, out _);
                nameToTypes.TryRemove(type.name, out _);
            }

            private void RemoveOlderHalf()
            {
                while (types.Count > maxCacheSize / 2) RemoveOldest();
            }
        }
    }
}