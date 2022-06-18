using Source.Utils;
using UnityEngine;

namespace Source.Model
{
    public class MetaPosition
    {
        public readonly Vector3Int chunk;
        public readonly MetaLocalPosition local;

        public MetaPosition(SerializableVector3 position)
            : this(position.x, position.y, position.z)
        {
        }

        public MetaPosition(Vector3 position)
            : this(position.x, position.y, position.z)
        {
        }

        public MetaPosition(Vector3Int chunk, MetaLocalPosition local)
        {
            this.chunk = chunk;
            this.local = local;
        }

        public MetaPosition(float x, float y, float z)
        {
            var chunkSize = Chunk.CHUNK_SIZE;
            chunk = Vectors.TruncateFloor(x / chunkSize.x, y / chunkSize.y, z / chunkSize.z);
            local = new MetaLocalPosition(new Vector3(x, y, z), chunk);
        }

        public Vector3 ToWorld()
        {
            return ToWorld(chunk, local);
        }

        public VoxelPosition ToVoxelPosition()
        {
            return new VoxelPosition(ToWorld());
        }
        
        public static Vector3 ToWorld(Vector3Int chunk, MetaLocalPosition local)
        {
            chunk.Scale(Chunk.CHUNK_SIZE);
            return chunk + local.position;
        }

        protected bool Equals(MetaPosition other)
        {
            return chunk.Equals(other.chunk) && local.Equals(other.local);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MetaPosition) obj);
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