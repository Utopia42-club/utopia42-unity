using System.Collections.Generic;
using System.Linq;
using src.MetaBlocks;
using src.MetaBlocks.ImageBlock;
using src.MetaBlocks.LinkBlock;
using src.MetaBlocks.MarkerBlock;
using src.MetaBlocks.NftBlock;
using src.MetaBlocks.TdObjectBlock;
using src.MetaBlocks.TeleportBlock;
using src.MetaBlocks.VideoBlock;
using src.Model;
using UnityEngine;

namespace src.Utils
{
    public static class Blocks
    {
        private static readonly Dictionary<uint, BlockType> TYPES = new Dictionary<uint, BlockType>();
        public static readonly BlockType AIR;
        public static MetaBlockType TdObjectBlockType => TYPES[34] as MetaBlockType; 
        public static MetaBlockType VideoBlockType => TYPES[32] as MetaBlockType; 

        static Blocks()
        {
            TYPES[0] = AIR = new BlockType(0, "air", false, 0, 0, 0, 0, 0, 0);
            TYPES[1] = new BlockType(1, "grass", true, 10, 10, 10, 10, 7, 11);
            TYPES[2] = new BlockType(2, "dark_grass", true, 35, 35, 35, 35, 7, 13);
            TYPES[3] = new BlockType(3, "bedrock", true, 0, 0, 0, 0, 0, 0);
            TYPES[4] = new BlockType(4, "dirt", true, 7, 7, 7, 7, 7, 7);
            TYPES[5] = new BlockType(5, "stone", true, 29, 29, 29, 29, 29, 29);
            TYPES[6] = new BlockType(6, "sand", true, 27, 27, 27, 27, 27, 27);
            TYPES[7] = new BlockType(7, "bricks", true, 3, 3, 3, 3, 3, 3);
            TYPES[8] = new BlockType(8, "wood", true, 19, 19, 19, 19, 20, 20);
            TYPES[9] = new BlockType(9, "planks", true, 21, 21, 21, 21, 21, 21);
            TYPES[10] = new BlockType(10, "cobblestone", true, 4, 4, 4, 4, 4, 4);
            TYPES[11] = new BlockType(11, "black_terracotta", true, 1, 1, 1, 1, 1, 1);
            TYPES[12] = new BlockType(12, "blue_wool", true, 2, 2, 2, 2, 2, 2);
            TYPES[13] = new BlockType(13, "cyan_wool", true, 5, 5, 5, 5, 5, 5);
            TYPES[14] = new BlockType(14, "diamond", true, 6, 6, 6, 6, 6, 6);
            TYPES[15] = new BlockType(15, "end_stone", true, 8, 8, 8, 8, 8, 8);
            TYPES[16] = new BlockType(16, "gold", true, 9, 9, 9, 9, 9, 9);
            TYPES[17] = new BlockType(17, "gravel", true, 12, 12, 12, 12, 12, 12);
            TYPES[18] = new BlockType(18, "green_wool", true, 13, 13, 13, 13, 13, 13);
            TYPES[19] = new BlockType(19, "ice", true, 14, 14, 14, 14, 14, 14);
            TYPES[20] = new BlockType(20, "lime_wool", true, 15, 15, 15, 15, 15, 15);
            TYPES[21] = new BlockType(21, "magma", true, 16, 16, 16, 16, 16, 16);
            TYPES[22] = new BlockType(22, "mossy_stone_bricks", true, 17, 17, 17, 17, 17, 17);
            TYPES[23] = new BlockType(23, "nether_bricks", true, 18, 18, 18, 18, 18, 18);
            TYPES[24] = new BlockType(24, "polished_andesite", true, 22, 22, 22, 22, 22, 22);
            TYPES[25] = new BlockType(25, "purple_wool", true, 23, 23, 23, 23, 23, 23);
            TYPES[26] = new BlockType(26, "purpur", true, 24, 24, 24, 24, 24, 24);
            TYPES[27] = new BlockType(27, "quartz", true, 25, 25, 25, 25, 25, 25);
            TYPES[28] = new BlockType(28, "red_wool", true, 26, 26, 26, 26, 26, 26);
            TYPES[29] = new BlockType(29, "snow", true, 28, 28, 28, 28, 28, 28);
            TYPES[30] = new BlockType(30, "stone_bricks", true, 30, 30, 30, 30, 30, 30);
            TYPES[31] = new ImageBlockType(31);
            TYPES[32] = new VideoBlockType(32);
            TYPES[33] = new LinkBlockType(33);
            TYPES[34] = new TdObjectBlockType(34);
            TYPES[35] = new MarkerBlockType(35);
            // TYPES[36] = new LightBlockType(36);
            TYPES[37] = new NftBlockType(37);
            TYPES[38] = new TeleportBlockType(38);
        }


        public static List<string> GetNonMetaBlockTypes()
        {
            return TYPES.Values
                .Where(blockType => !(blockType is MetaBlockType))
                .Select(x => x.name).ToList();
        }

        public static List<BlockType> GetBlockTypes()
        {
            return TYPES.Values.ToList();
        }

        public static BlockType GetBlockType(uint id)
        {
            return ColorBlocks.IsColorTypeId(id, out var blockType) ? blockType : TYPES[id];
        }

        public static BlockType GetBlockType(string name, bool excludeMetaBlocks = false,
            bool excludeBaseBlocks = false)
        {
            if (ColorBlocks.IsColorBlockType(name, out var blockType))
                return blockType;

            foreach (var entry in from entry in TYPES
                where !excludeMetaBlocks || !(entry.Value is MetaBlockType)
                where !excludeBaseBlocks || entry.Value is MetaBlockType
                where entry.Value.name.Equals(name)
                select entry)
                return entry.Value;

            Debug.LogError("Invalid block type: " + name);
            return null;
        }
    }
}