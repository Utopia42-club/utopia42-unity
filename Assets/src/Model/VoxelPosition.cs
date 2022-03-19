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
            var chunkSize = Chunk.CHUNK_SIZE;
            chunk = Vectors.TruncateFloor(x / chunkSize.x, y / chunkSize.y, z / chunkSize.z);
            local = Vectors.TruncateFloor(x, y, z);

            local.x -= chunk.x * chunkSize.x;
            local.y -= chunk.y * chunkSize.y;
            local.z -= chunk.z * chunkSize.z;
        }

        public Vector3Int ToWorld()
        {
            return ToWorld(chunk, local);
        }

        public static Vector3Int ToWorld(Vector3Int chunk, Vector3Int local)
        {
            chunk.Scale(Chunk.CHUNK_SIZE);
            return chunk + local;
        }
    }
}