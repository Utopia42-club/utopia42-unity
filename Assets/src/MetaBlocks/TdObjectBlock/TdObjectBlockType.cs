using Siccity.GLTFUtility;
using src.Model;
using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockType : MetaBlockType
    {
        private const float LocalScale = 0.6f;
        public TdObjectBlockType(byte id) : base(id, "3d_object", typeof(TdObjectBlockObject),
            typeof(TdObjectBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            return Create3dPlaceHolder(!error ? "3d_object" : "3d_object_error", "3d object placeholder", withCollider,
                LocalScale);
        }

        public override MetaPosition GetPutPosition(Vector3 purePosition)
        {
            return new MetaPosition(purePosition + LocalScale * Vector3.up);
        }
    }
}