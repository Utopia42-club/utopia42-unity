using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelPosition
{
    public readonly Vector3Int chunk;
    public readonly Vector3Int local;

    public VoxelPosition(Vector3 position)
    {
        chunk = Vectors.FloorToInt(position / Chunk.CHUNK_LENGTH);
        local = Vectors.FloorToInt(position) - (chunk * Chunk.CHUNK_LENGTH);
    }  
}
