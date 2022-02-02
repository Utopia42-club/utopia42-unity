using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.MarkerBlock
{
    public class MarkerBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/MarkerBlockEditor";

        public new InputField name;

        public MarkerBlockProperties GetValue()
        {
            if (HasValue(name))
            {
                return new MarkerBlockProperties
                {
                    name = name.text.Trim()
                };
            }

            return null;
        }

        public void SetValue(MarkerBlockProperties value)
        {
            if (value == null)
            {
                name.text = "";
                return;
            }

            name.text = value.name == null ? "" : value.name;
        }

        private bool HasValue(InputField f)
        {
            return !string.IsNullOrEmpty(f.text);
        }
    }
}