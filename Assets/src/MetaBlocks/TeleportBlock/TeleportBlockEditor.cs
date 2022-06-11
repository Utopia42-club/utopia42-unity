using src.Model;
using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.TeleportBlock
{
    public class TeleportBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/TeleportBlockEditor";

        [SerializeField] public InputField posX;
        [SerializeField] public InputField posY;
        [SerializeField] public InputField posZ;

        public TeleportBlockProperties GetValue()
        {
            if (HasValue(posX) && HasValue(posY) && HasValue(posZ))
            {
                return new TeleportBlockProperties
                {
                    destination = new[]
                    {
                        int.Parse(posX.text), int.Parse(posY.text),
                        int.Parse(posZ.text)
                    }
                };
            }

            return null;
        }

        public void SetValue(TeleportBlockProperties value)
        {
            if (value == null) return;

            if (value.destination != null)
            {
                posX.text = value.destination[0].ToString();
                posY.text = value.destination[1].ToString();
                posZ.text = value.destination[2].ToString();
            }
        }

        private bool HasValue(InputField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}