using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.VideoBlock
{
    public class VideoBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/VideoBlockEditor";
        [SerializeField] private InputField url;
        [SerializeField] private InputField width;
        [SerializeField] private InputField height;
        [SerializeField] private InputField previewTime;
        [SerializeField] public Toggle detectCollision;

        public VideoBlockProperties.FaceProps GetValue()
        {
            if (HasValue(url) && HasValue(width) && HasValue(height))
            {
                var props = new VideoBlockProperties.FaceProps();
                props.url = url.text;
                props.width = int.Parse(width.text);
                props.height = int.Parse(height.text);
                props.previewTime = HasValue(previewTime) ? float.Parse(previewTime.text) : 0f;
                props.detectCollision = detectCollision.isOn;
                return props;
            }

            return null;
        }

        public void SetValue(VideoBlockProperties.FaceProps value)
        {
            url.text = value == null ? "" : value.url;
            width.text = value == null ? "" : value.width.ToString();
            height.text = value == null ? "" : value.height.ToString();
            previewTime.text = value == null ? "0" : value.previewTime.ToString();
            detectCollision.isOn = value == null ? true : value.detectCollision;
        }

        private bool HasValue(InputField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}