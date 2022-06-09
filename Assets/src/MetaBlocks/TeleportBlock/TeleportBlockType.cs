using src.Model;
using UnityEngine;

namespace src.MetaBlocks.TeleportBlock
{
    public class TeleportBlockType : MetaBlockType
    {
        public const float Gap = 0.2f;
        public static readonly string PORTAL_PREFAB = "Portal";

        public TeleportBlockType(byte id) : base(id, "teleport", typeof(TeleportBlockObject),
            typeof(TeleportBlockProperties))
        {
        }

        public override GameObject CreatePlaceHolder(bool error, bool withCollider)
        {
            return Object.Instantiate(Resources.Load<GameObject>(PORTAL_PREFAB), World.INSTANCE.transform);
        }

        public override MetaPosition GetPutPosition(Vector3 purePosition)
        {
            var pos = Player.INSTANCE.transform.forward.z > 0
                ? purePosition - Gap * Vector3.forward
                : purePosition + Gap * Vector3.forward;
            pos += 0.5f * 12 * Vector3.up;
            return new MetaPosition(pos);
        }
    }
}