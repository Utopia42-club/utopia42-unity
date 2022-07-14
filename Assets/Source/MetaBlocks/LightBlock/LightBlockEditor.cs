using UnityEngine;
using UnityEngine.UI;

namespace Source.MetaBlocks.LightBlock
{
    public class LightBlockEditor : MonoBehaviour
    {
        private const float DefaultIntensity = 10;
        private const float DefaultRange = 10;
        public static readonly Color DefaultColor = Color.yellow;
        public static readonly string PREFAB = "MetaBlocks/LightBlockEditor";

        [SerializeField] private InputField intensity;
        [SerializeField] private InputField range;
        [SerializeField] private Image colorImage;
        // [SerializeField] private FlexibleColorPicker colorPicker;

        private void Update()
        {
            // if (colorImage != null && colorPicker != null)
            //     colorImage.color = colorPicker.color;
        }

        public LightBlockProperties GetValue()
        {
            // if (HasValue(intensity) && HasValue(range) && colorPicker != null)
            // {
            //     return new LightBlockProperties
            //     {
            //         intensity = float.Parse(intensity.text),
            //         range = float.Parse(range.text),
            //         hexColor = "#" + ColorUtility.ToHtmlStringRGB(colorPicker.color)
            //     };
            // }

            return null;
        }

        public void SetValue(LightBlockProperties value)
        {
            // if (value == null)
            // {
            //     intensity.text = DefaultIntensity.ToString();
            //     range.text = DefaultRange.ToString();
            //     colorPicker.SetColor(DefaultColor);
            //     return;
            // }
            //
            // intensity.text = value.intensity.ToString();
            // range.text = value.range.ToString();
            // colorPicker.SetColor(ColorUtility.TryParseHtmlString(value.hexColor, out var color) ? color : DefaultColor);
        }

        private bool HasValue(InputField f)
        {
            return !string.IsNullOrEmpty(f.text);
        }
    }
}