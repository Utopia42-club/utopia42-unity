using UnityEngine;

public class VoxelPosition
{
    public readonly Vector3Int chunk;
    public readonly Vector3Int local;

    public VoxelPosition(Vector3 position)
    {
        chunk = Vectors.FloorToInt(position.x / Chunk.CHUNK_WIDTH, position.y / Chunk.CHUNK_HEIGHT, position.z / Chunk.CHUNK_WIDTH);
        local = Vectors.FloorToInt(position);
        local.x -= chunk.x * Chunk.CHUNK_WIDTH;
        local.y -= chunk.y * Chunk.CHUNK_HEIGHT;
        local.z -= chunk.z * Chunk.CHUNK_WIDTH;
    }  
}
