using System;

namespace src.MetaBlocks.MarkerBlock
{
    [Serializable]
    public class MarkerBlockProperties : ICloneable
    {
        public string name;

        public MarkerBlockProperties()
        {
        }

        public MarkerBlockProperties(MarkerBlockProperties obj)
        {
            if (obj != null)
            {
                name = obj.name;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;

            return obj is MarkerBlockProperties props && name == props.name;
        }

        public object Clone()
        {
            return new MarkerBlockProperties
            {
                name = name
            };
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(name);
        }
    }
}