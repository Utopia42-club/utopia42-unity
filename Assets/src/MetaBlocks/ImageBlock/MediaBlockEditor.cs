using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.ImageBlock
{
    public class MediaBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/MediaBlockEditor";
        public static readonly int DEFAULT_DIMENSION = 3;
        [SerializeField] public InputField url;
        [SerializeField] public InputField width;
        [SerializeField] public InputField height;
        [SerializeField] public Toggle detectCollision;

        public MediaBlockProperties.FaceProps GetValue()
        {
            if (HasValue(url))
            {
                var props = new MediaBlockProperties.FaceProps();
                props.url = url.text;
                props.width = HasValue(width) ? int.Parse(width.text) : DEFAULT_DIMENSION;
                props.height = HasValue(height) ? int.Parse(height.text) : DEFAULT_DIMENSION;
                props.detectCollision = detectCollision.isOn;
                return props;
            }

            return null;
        }

        public void SetValue(MediaBlockProperties.FaceProps value)
        {
            url.text = value == null ? "" : value.url;
            width.text = value == null ? DEFAULT_DIMENSION.ToString() : value.width.ToString();
            height.text = value == null ? DEFAULT_DIMENSION.ToString() : value.height.ToString();
            detectCollision.isOn = value == null ? true : value.detectCollision;
        }

        private bool HasValue(InputField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}