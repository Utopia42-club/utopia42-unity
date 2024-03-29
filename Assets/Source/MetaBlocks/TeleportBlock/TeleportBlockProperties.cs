using System;

namespace Source.MetaBlocks.TeleportBlock
{
    [Serializable]
    public class TeleportBlockProperties : ICloneable
    {
        public int networkId;
        public string contractAddress;
        public int[] destination;

        public TeleportBlockProperties()
        {
        }

        public TeleportBlockProperties(TeleportBlockProperties obj)
        {
            if (obj != null)
            {
                destination = obj.destination;
            }
        }

        public void UpdateProps(TeleportBlockProperties props)
        {
            if (props == null) return;
            destination = props.destination;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as TeleportBlockProperties;
            return
                (destination == prop.destination
                 || destination.Length == prop.destination.Length &&
                 destination[0] == prop.destination[0] &&
                 destination[1] == prop.destination[1] &&
                 destination[2] == prop.destination[2]) &&
                networkId == prop.networkId &&
                string.Equals(contractAddress, prop.contractAddress, StringComparison.OrdinalIgnoreCase);
        }

        public object Clone()
        {
            var obj = new TeleportBlockProperties();
            if (destination != null)
            {
                obj.destination = new int[3];
                destination.CopyTo(obj.destination, 0);
            }

            return obj;
        }
    }
}