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
        [SerializeField] public InputField width;
        [SerializeField] public InputField height;
        [SerializeField] public Toggle detectCollision;

        public NftBlockProperties GetValue()
        {
            if (!HasValue(collection) || !HasValue(tokenId)) return null;
            return new NftBlockProperties
            {
                collection = collection.text,
                tokenId = long.Parse(tokenId.text),
                width = HasValue(width) ? int.Parse(width.text) : DEFAULT_DIMENSION,
                height = HasValue(height) ? int.Parse(height.text) : DEFAULT_DIMENSION,
                detectCollision = detectCollision.isOn
            };
        }

        public void SetValue(NftBlockProperties value)
        {
            collection.text = value == null ? "" : value.collection;
            tokenId.text = value == null ? "" : value.tokenId.ToString();
            width.text = value == null ? DEFAULT_DIMENSION.ToString() : value.width.ToString();
            height.text = value == null ? DEFAULT_DIMENSION.ToString() : value.height.ToString();
            detectCollision.isOn = value?.detectCollision ?? true;
        }

        private bool HasValue(InputField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}