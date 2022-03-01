using System;
using src.Model;
using UnityEngine;

namespace src.Utils
{
    public static class ColorBlocks
    {
        public static bool IsColorBlockType(string colorBlockTypeName, out BlockType blockType)
        {
            if (!colorBlockTypeName.StartsWith("#"))
            {
                blockType = null;
                return false;
            }

            if (colorBlockTypeName.Length != 7 || !ColorUtility.TryParseHtmlString(colorBlockTypeName, out var color))
            {
                Debug.LogError("Invalid block color: " + colorBlockTypeName);
                blockType = null;
                return true;
            }

            blockType = GetBlockTypeFromColor(color, colorBlockTypeName);
            return true;
        }

        public static BlockType GetBlockTypeFromColor(Color32 color, string name = null)
        {
            var id = GetTypeIdFromColor(color);
            if (name == null)
                name = "#" + ColorUtility.ToHtmlStringRGB(color);
            return new BlockType(id, color, name);
        }

        public static bool IsColorTypeId(uint id, out BlockType blockType)
        {
            var bytes = BitConverter.GetBytes(id);
            
            if (bytes[3] != 1)
            {
                blockType = null;
                return false;
            }

            var color = new Color32(bytes[2], bytes[1], bytes[0], 1);
            blockType = GetBlockTypeFromColor(color);
            return true;
        }

        public static uint GetTypeIdFromColor(Color32 color)
        {
            return (uint) (color.b + (color.g << 8) + (color.r << 16) + (1 << 24));
        }
    }
}