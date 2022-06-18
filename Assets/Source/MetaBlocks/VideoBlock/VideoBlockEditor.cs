using System;
using Source.MetaBlocks.ImageBlock;
using Source.Model;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.VideoBlock
{
    public class VideoBlockEditor
    {
        private TextField url;

        private TextField rotationX;
        private TextField rotationY;
        private TextField rotationZ;

        private TextField width;
        private TextField height;
        private TextField previewTime;
        private Toggle detectCollision;

        public VideoBlockEditor(Action<VideoBlockProperties> onSave)
        {
            var root = PropertyEditor.INSTANCE.Setup("UiDocuments/PropertyEditors/VideoBlockEditor",
                "Video Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                });

            url = root.Q<TextField>("url");
            rotationX = root.Q<TextField>("x");
            rotationY = root.Q<TextField>("y");
            rotationZ = root.Q<TextField>("z");
            width = root.Q<TextField>("w");
            height = root.Q<TextField>("h");
            previewTime = root.Q<TextField>("previewTime");
            detectCollision = root.Q<Toggle>("collisionDetect");
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(url);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(rotationX);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(rotationY);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(rotationZ);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(width);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(height);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(previewTime);
        }

        public VideoBlockProperties GetValue()
        {
            if (!HasValue(url) || !HasValue(rotationX) || !HasValue(rotationY) ||
                !HasValue(rotationZ)) return null;
            return new VideoBlockProperties
            {
                url = url.text,
                rotation = new SerializableVector3(float.Parse(rotationX.text), float.Parse(rotationY.text),
                    float.Parse(rotationZ.text)),
                width = HasValue(width) ? int.Parse(width.text) : MediaBlockEditor.DEFAULT_DIMENSION,
                height = HasValue(height) ? int.Parse(height.text) : MediaBlockEditor.DEFAULT_DIMENSION,
                previewTime = HasValue(previewTime) ? float.Parse(previewTime.text) : 0f,
                detectCollision = detectCollision.value
            };
        }

        public void SetValue(VideoBlockProperties value)
        {
            if (value?.rotation == null)
            {
                rotationX.value = "0";
                rotationY.value = "0";
                rotationZ.value = "0";
            }
            else
            {
                rotationX.value = value.rotation.x.ToString();
                rotationY.value = value.rotation.y.ToString();
                rotationZ.value = value.rotation.z.ToString();
            }

            if (value == null)
            {
                url.value = "";
                width.value = MediaBlockEditor.DEFAULT_DIMENSION.ToString();
                height.value = MediaBlockEditor.DEFAULT_DIMENSION.ToString();
                previewTime.value = "0";
                detectCollision.value = true;
                return;
            }

            url.value = value.url;
            width.value = value.width.ToString();
            height.value = value.height.ToString();
            previewTime.value = value.previewTime.ToString();
            detectCollision.value = value.detectCollision;
        }
        
        public void Show()
        {
            PropertyEditor.INSTANCE.Show();
        }

        private bool HasValue(TextField f)
        {
            return !string.IsNullOrEmpty(f.text);
        }
    }
}