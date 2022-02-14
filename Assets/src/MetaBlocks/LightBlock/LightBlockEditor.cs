using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.LightBlock
{
    public class LightBlockEditor : MonoBehaviour
    {
        private const float DefaultIntensity = 10;
        private const float DefaultRange = 10;
        public static readonly string PREFAB = "MetaBlocks/LightBlockEditor";

        [SerializeField] public InputField intensity;
        [SerializeField] public InputField range;

        public LightBlockProperties GetValue()
        {
            if (HasValue(intensity) && HasValue(range))
            {
                return new LightBlockProperties
                {
                    intensity = float.Parse(intensity.text),
                    range = float.Parse(range.text)
                };
            }

            return null;
        }

        public void SetValue(LightBlockProperties value)
        {
            if (value == null)
            {
                intensity.text = DefaultIntensity.ToString();
                range.text = DefaultRange.ToString();
                return;
            }

            intensity.text = value.intensity.ToString();
            range.text = value.range.ToString();
        }

        private bool HasValue(InputField f)
        {
            return !string.IsNullOrEmpty(f.text);
        }
    }
}