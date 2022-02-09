using src.Utils;
using UnityEngine;

namespace src.Model
{
    public class VoxelPosition
    {
        public readonly Vector3Int chunk;
        public readonly Vector3Int local;
        public VoxelPosition(Vector3 position)
            : this(position.x, position.y, position.z)
        {
        }

        public VoxelPosition(float x, float y, float z)
        {
            chunk = Vectors.FloorToInt(x / Chunk.CHUNK_WIDTH, y / Chunk.CHUNK_HEIGHT, z / Chunk.CHUNK_WIDTH);
            local = Vectors.FloorToInt(x, y, z);
            local.x -= chunk.x * Chunk.CHUNK_WIDTH;
            local.y -= chunk.y * Chunk.CHUNK_HEIGHT;
            local.z -= chunk.z * Chunk.CHUNK_WIDTH;
        }

        public Vector3Int ToWorld()
        {
            return ToWorld(chunk, local);
        }

        public static Vector3Int ToWorld(Vector3Int chunk, Vector3Int local)
        {
            return new Vector3Int(chunk.x * Chunk.CHUNK_WIDTH + local.x,
                chunk.y * Chunk.CHUNK_HEIGHT + local.y,
                chunk.z * Chunk.CHUNK_WIDTH + local.z);
        }

    }
}
