using System;
using UnityEngine;

namespace src.MetaBlocks.LightBlock
{
    [Serializable]
    public class LightBlockProperties : ICloneable
    {
        public float intensity;
        public float range;
        public Color color;

        public LightBlockProperties()
        {
        }

        public LightBlockProperties(LightBlockProperties obj)
        {
            if (obj != null)
            {
                intensity = obj.intensity;
                range = obj.range;
                color = obj.color;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;

            return obj is LightBlockProperties props && intensity == props.intensity && range == props.range && color.Equals(props.color);
        }

        public object Clone()
        {
            return new LightBlockProperties
            {
                range = range,
                intensity = intensity,
                color = color,
            };
        }
    }
}