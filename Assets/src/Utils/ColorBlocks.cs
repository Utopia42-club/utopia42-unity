using System;
using System.Collections.Concurrent;
using src.Model;
using UnityEngine;

namespace src.Utils
{
    public static class ColorBlocks
    {
        private static readonly ColorCache Cache = new ColorCache(1024);

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
            // var b = ColorUtility.TryParseHtmlString(blockType.name, out var color);
            // return b ? color : Color.white;
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

            var color = new Color32(bytes[2], bytes[1], bytes[0], 1);
            blockType = GetBlockTypeFromColor(color);
            Cache.Add(blockType);
            return true;
        }

        private static uint GetTypeIdFromColor(Color32 color)
        {
            return (uint) (color.b + (color.g << 8) + (color.r << 16) + (1 << 24));
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