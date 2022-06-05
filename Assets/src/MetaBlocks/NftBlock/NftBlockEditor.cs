using src.Model;
using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.NftBlock
{
    public class NftBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/NftBlockEditor";
        public static readonly int DEFAULT_DIMENSION = 3;
        [SerializeField] public InputField collection;
        [SerializeField] public InputField tokenId;
        
        [SerializeField] public InputField rotationX;
        [SerializeField] public InputField rotationY;
        [SerializeField] public InputField rotationZ;
        
        [SerializeField] public InputField width;
        [SerializeField] public InputField height;
        [SerializeField] public Toggle detectCollision;

        public NftBlockProperties GetValue()
        {
            if (!HasValue(collection) || !HasValue(tokenId) || !HasValue(rotationX) || !HasValue(rotationY) ||
                !HasValue(rotationZ)) return null;
            return new NftBlockProperties
            {
                collection = collection.text,
                tokenId = long.Parse(tokenId.text),
                rotation = new SerializableVector3(float.Parse(rotationX.text), float.Parse(rotationY.text),
                    float.Parse(rotationZ.text)),
                width = HasValue(width) ? int.Parse(width.text) : DEFAULT_DIMENSION,
                height = HasValue(height) ? int.Parse(height.text) : DEFAULT_DIMENSION,
                detectCollision = detectCollision.isOn
            };
        }

        public void SetValue(NftBlockProperties value)
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
                collection.text = "";
                tokenId.text = "";
                width.text = DEFAULT_DIMENSION.ToString();
                height.text = DEFAULT_DIMENSION.ToString();
                detectCollision.isOn = true;
                return;
            }

            collection.text = value.collection;
            tokenId.text = value.tokenId.ToString();
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