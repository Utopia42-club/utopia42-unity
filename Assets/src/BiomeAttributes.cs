using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "MinecraftTutorial/Biome Attribute")]
public class BiomeAttributes : ScriptableObject {

    public string biomeName;

    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    public Lode[] lodes;

    BiomeAttributes()
    {
        Dictionary<int, BlockType> types = new Dictionary<int, BlockType>();
        //types[0] = new BlockType(0, "air", false, 0, 0, 0, 0, 0, 0);
        types[1] = new BlockType(1, "grass", true, 10, 10, 10, 10, 7, 11);
        types[2] = new BlockType(2, "bedrock", true, 0, 0, 0, 0, 0, 0);
        types[3] = new BlockType(3, "dirt", true, 7, 7, 7, 7, 7, 7);
        types[4] = new BlockType(4, "stone", true, 29, 29, 29, 29, 29, 29);
        types[5] = new BlockType(5, "sand", true, 27, 27, 27, 27, 27, 27);
        types[6] = new BlockType(6, "bricks", true, 3, 3, 3, 3, 3, 3);
        types[7] = new BlockType(7, "wood", true, 19, 19, 19, 19, 20, 20);
        types[8] = new BlockType(8, "planks", true, 21, 21, 21, 21, 21, 21);
        types[9] = new BlockType(9, "cobblestone", true, 4, 4, 4, 4, 4, 4);
        types[10] = new BlockType(10, "black_terracotta", true, 1, 1, 1, 1, 1, 1);
        types[11] = new BlockType(11, "blue_wool", true, 2, 2, 2, 2, 2, 2);
        types[12] = new BlockType(12, "cyan_wool", true, 5, 5, 5, 5, 5, 5);
        types[13] = new BlockType(13, "diamond", true, 6, 6, 6, 6, 6, 6);
        types[14] = new BlockType(14, "end_stone", true, 8, 8, 8, 8, 8, 8);
        types[15] = new BlockType(15, "gold", true, 9, 9, 9, 9, 9, 9);
        types[16] = new BlockType(16, "gravel", true, 12, 12, 12, 12, 12, 12);
        types[17] = new BlockType(17, "green_wool", true, 13, 13, 13, 13, 13, 13);
        types[18] = new BlockType(18, "ice", true, 14, 14, 14, 14, 14, 14);
        types[19] = new BlockType(19, "lime_wool", true, 15, 15, 15, 15, 15, 15);
        types[20] = new BlockType(20, "magma", true, 16, 16, 16, 16, 16, 16);
        types[21] = new BlockType(21, "mossy_stone_bricks", true, 17, 17, 17, 17, 17, 17);
        types[22] = new BlockType(22, "nether_bricks", true, 18, 18, 18, 18, 18, 18);
        types[23] = new BlockType(23, "polished_andesite", true, 22, 22, 22, 22, 22, 22);
        types[24] = new BlockType(24, "purple_wool", true, 23, 23, 23, 23, 23, 23);
        types[25] = new BlockType(25, "purpur", true, 24, 24, 24, 24, 24, 24);
        types[26] = new BlockType(26, "quartz", true, 25, 25, 25, 25, 25, 25);
        types[27] = new BlockType(27, "red_wool", true, 26, 26, 26, 26, 26, 26);
        types[28] = new BlockType(28, "snow", true, 28, 28, 28, 28, 28, 28);
        types[29] = new BlockType(29, "stone_bricks", true, 30, 30, 30, 30, 30, 30);

        var lodeList = new List<Lode>();
        foreach(var entry in types)
        {
            var type = entry.Value;
            Lode lode;
            lodeList.Add(lode = new Lode());
            lode.blockID = type.id;
            lode.nodeName = type.name;
            lode.minHeight = 42 * ((lodeList.Count - 1) / types.Count);
            lode.maxHeight = 42* (1 - (lodeList.Count-1) / types.Count);
            lode.scale = 1;
            lode.threshold = lodeList.Count/types.Count;
            lode.noiseOffset = 0;
        }

        lodes = lodeList.ToArray();
    }

}

public class Lode {
    public string nodeName;
    public int blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}
