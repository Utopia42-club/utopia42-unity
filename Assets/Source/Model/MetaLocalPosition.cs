using Source.Utils;
using UnityEngine;

namespace Source.Model
{
    public class MetaLocalPosition
    {
        public readonly Vector3 position;
        private const int Precision = 2; // FormatKey in LandDetails should be adapted in case of a change
        public const float Step = 0.01f; 

        public MetaLocalPosition(float x, float y, float z)
        {
            this.position = Vectors.Truncate(x, y, z, Precision);
        } 
        
        public MetaLocalPosition(Vector3 localPosition)
        {
            position = Vectors.Truncate(localPosition, Precision);
        }
        public MetaLocalPosition(Vector3 localPosition, Vector3Int chunk)
        {
            var chunkSize = Chunk.CHUNK_SIZE;
            position = Vectors.Truncate(localPosition, Precision);
            
            position.x -= chunk.x * chunkSize.x;
            position.y -= chunk.y * chunkSize.y;
            position.z -= chunk.z * chunkSize.z;
        }

        private bool Equals(MetaLocalPosition other)
        {
            return LandDetails.FormatKey(position).Equals(LandDetails.FormatKey(other.position));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MetaLocalPosition) obj);
        }

        public override int GetHashCode()
        {
            return LandDetails.FormatKey(position).GetHashCode();
        }
    }
}