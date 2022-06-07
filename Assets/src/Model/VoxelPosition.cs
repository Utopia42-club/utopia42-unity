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

        private VoxelPosition(float x, float y, float z)
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

        // TODO [detach metablock]: temp
        // public MetaPosition ToMetaPosition()
        // {
        //     return new MetaPosition()
        // }

        public static Vector3Int ToWorld(Vector3Int chunk, Vector3Int local)
        {
            chunk.Scale(Chunk.CHUNK_SIZE);
            return chunk + local;
        }
        
        protected bool Equals(VoxelPosition other)
        {
            return chunk.Equals(other.chunk) && local.Equals(other.local);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VoxelPosition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (chunk.GetHashCode() * 397) ^ local.GetHashCode();
            }
        }
    }
}