using src.Utils;
using UnityEngine;

namespace src.Model
{
    public class VoxelPosition
    {
        public readonly Vector3Int chunk;
        public readonly Vector3Int local;

        public VoxelPosition(SerializableVector3Int position)
            : this(position.x, position.y, position.z)
        {
        }
        
        public VoxelPosition(SerializableVector3 position)
            : this(position.x, position.y, position.z)
        {
        }
        
        public VoxelPosition(Vector3 position)
            : this(position.x, position.y, position.z)
        {
        }

        public VoxelPosition(Vector3Int chunk, Vector3Int local)
        {
            this.chunk = chunk;
            this.local = local;
        }

        public VoxelPosition(float x, float y, float z)
        {
            chunk = Vectors.TruncateFloor(x / Chunk.CHUNK_WIDTH, y / Chunk.CHUNK_HEIGHT, z / Chunk.CHUNK_WIDTH);
            local = Vectors.TruncateFloor(x, y, z);
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