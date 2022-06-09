using System;
using src.Model;

namespace src.MetaBlocks.TeleportBlock
{
    [Serializable]
    public class TeleportBlockProperties :  ICloneable
    {

        public TeleportBlockProperties()
        {
        }
        
        public TeleportBlockProperties(TeleportBlockProperties obj)
        {
            if (obj != null)
            {
            }
        }
        
        public void UpdateProps(TeleportBlockProperties props)
        {
            if (props == null) return;
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as TeleportBlockProperties;
            return true;
        }

        public object Clone()
        {
            return new TeleportBlockProperties()
            {
            };
        }
        
    }
}