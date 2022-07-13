using System;

namespace Source.MetaBlocks.LightBlock
{
    [Serializable]
    public class LightBlockProperties : ICloneable
    {
        public float intensity;
        public float range;
        public string hexColor;

        public LightBlockProperties()
        {
        }

        public LightBlockProperties(LightBlockProperties obj)
        {
            if (obj != null)
            {
                intensity = obj.intensity;
                range = obj.range;
                hexColor = obj.hexColor;
            }
        }

        public object Clone()
        {
            return new LightBlockProperties
            {
                range = range,
                intensity = intensity,
                hexColor = hexColor
            };
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null || !GetType().Equals(obj.GetType()))
                return false;

            return obj is LightBlockProperties props && intensity == props.intensity && range == props.range &&
                   hexColor.Equals(props.hexColor);
        }
    }
}