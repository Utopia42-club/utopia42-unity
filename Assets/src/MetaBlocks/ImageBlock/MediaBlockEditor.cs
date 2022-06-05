using src.Model;
using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.ImageBlock
{
    public class MediaBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/MediaBlockEditor";
        public static readonly int DEFAULT_DIMENSION = 3;
        [SerializeField] public InputField url;

        [SerializeField] public InputField rotationX;
        [SerializeField] public InputField rotationY;
        [SerializeField] public InputField rotationZ;

        [SerializeField] public InputField width;
        [SerializeField] public InputField height;

        [SerializeField] public Toggle detectCollision;

        public MediaBlockProperties GetValue()
        {
            if (!HasValue(url) || !HasValue(rotationX) || !HasValue(rotationY) || !HasValue(rotationZ)) return null;
            return new MediaBlockProperties
            {
                url = url.text,
                rotation = new SerializableVector3(float.Parse(rotationX.text), float.Parse(rotationY.text),
                    float.Parse(rotationZ.text)),
                width = HasValue(width) ? int.Parse(width.text) : DEFAULT_DIMENSION,
                height = HasValue(height) ? int.Parse(height.text) : DEFAULT_DIMENSION,
                detectCollision = detectCollision.isOn
            };
        }

        public void SetValue(MediaBlockProperties value)
        {
            if (value?.rotation == null)
            {
                rotationX.text = "0";
                rotationY.text = "0";
                rotationZ.text = "0";
            }
            else
            {
                rotationX.text = value.rotation.x.ToString();
                rotationY.text = value.rotation.y.ToString();
                rotationZ.text = value.rotation.z.ToString();
            }

            if (value == null)
            {
                url.text = "";
                width.text = DEFAULT_DIMENSION.ToString();
                height.text = DEFAULT_DIMENSION.ToString();
                detectCollision.isOn = true;
                return;
            }

            url.text = value.url;
            width.text = value.width.ToString();
            height.text = value.height.ToString();
            detectCollision.isOn = value.detectCollision;
        }

        private bool HasValue(InputField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}