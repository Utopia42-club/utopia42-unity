using Source.Model;
using UnityEngine;

namespace Source.MetaBlocks.TeleportBlock
{
    public class TeleportBlockType : MetaBlockType
    {
        private const float Gap = 0.2f;
        internal const float LocalScale = 2f;
        internal const string PortalPrefab = "Portal";

        public TeleportBlockType(byte id) : base(id, "teleport", typeof(TeleportBlockObject),
            typeof(TeleportBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            var go = TeleportBlockObject.CreatePortal(World.INSTANCE.transform, withCollider);
            go.name = "Teleport place holder";
            return go;
        }

        public override MetaPosition GetPlaceHolderPutPosition(Vector3 purePosition)
        {
            return new MetaPosition(purePosition + (LocalScale - Gap) * Vector3.up);
        }
    }
}