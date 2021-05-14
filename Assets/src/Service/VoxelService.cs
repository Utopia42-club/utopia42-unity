using System.Collections.Generic;
using UnityEngine;
public class VoxelService
{
    private byte air = 0;
    private byte grass = 1;
    private byte bedrock = 2;
    private byte dirt = 3;

    Dictionary<byte, BlockType> types = new Dictionary<byte, BlockType>();
    Dictionary<string, Dictionary<string, byte>> changes
        = new Dictionary<string, Dictionary<string, byte>>();

    public VoxelService()
    {
        types[0] = new BlockType(0, "Air", false, 0, 0, 0, 0, 0, 0);
        types[1] = new BlockType(1, "Grass", true, 0, 0, 0, 0, 0, 0);
        types[2] = new BlockType(2, "Bedrock", true, 0, 0, 0, 0, 0, 0);
        types[3] = new BlockType(3, "Dirt", true, 0, 0, 0, 0, 0, 0);
    }

    public void FillChunk(Vector3Int coordinate, byte[,,] voxels)
    {
        InitiateChunk(coordinate, voxels);

        Dictionary<string, byte> chunkChanges;
        if (changes.TryGetValue(Vectors.FormatKey(coordinate), out chunkChanges))
        {
            foreach (var change in chunkChanges)
            {
                var voxel = Vectors.ParseKey(change.Key);
                voxels[voxel.x, voxel.y, voxel.z] = change.Value;
            }
        }
    }


    private void InitiateChunk(Vector3Int position, byte[,,] voxels)
    {
        byte body;
        byte top;
        if (position.y < 0)
        {
            top = body = bedrock;
        }
        else if (position.y == 0)
        {
            body = dirt;
            top = grass;
        }
        else
            body = top = air;

        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int z = 0; z < voxels.GetLength(2); z++)
            {
                int maxy = voxels.GetLength(1) - 1;

                voxels[x, maxy, z] = top;
                for (int y = 0; y < maxy; y++)
                    voxels[x, y, z] = body;
            }
        }

    }

    public BlockType GetBlockType(byte id)
    {
        return types[id];
    }

    public bool IsSolid(VoxelPosition vp)
    {
        Dictionary<string, byte> chunkChanges;
        if (changes.TryGetValue(Vectors.FormatKey(vp.chunk), out chunkChanges))
        {
            byte type;
            if (chunkChanges.TryGetValue(Vectors.FormatKey(vp.local), out type))
            {
                return GetBlockType(type).isSolid;
            }
        }

        return vp.chunk.y <= 0;
    }
}
