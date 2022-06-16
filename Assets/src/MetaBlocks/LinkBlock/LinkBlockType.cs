using src.Model;
using UnityEngine;

namespace src.MetaBlocks.LinkBlock
{
    public class LinkBlockType : MetaBlockType
    {
        private const float LocalScale = 0.6f;

        public LinkBlockType(byte id) : base(id, "link", typeof(LinkBlockObject), typeof(LinkBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            return Create3dPlaceHolder(!error ? "link" : "3d_object_error", "link placeholder", withCollider,
                LocalScale);
        }

        public override MetaPosition GetPlaceHolderPutPosition(Vector3 purePosition)
        {
            return new MetaPosition(purePosition + LocalScale * Vector3.up);
        }
    }
}