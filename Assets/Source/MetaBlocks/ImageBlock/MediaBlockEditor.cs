using System;
using Source.Model;
using Source.Ui.Utils;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.ImageBlock
{
    public class MediaBlockEditor
    {
        public static readonly int DEFAULT_DIMENSION = 3;
        private readonly Toggle detectCollision;
        private readonly TextField height;
        private readonly TextField rotationX;
        private readonly TextField rotationY;
        private readonly TextField rotationZ;

        private readonly TextField url;
        private readonly TextField width;


        public MediaBlockEditor(Action<MediaBlockProperties> onSave, int instanceID)
        {
            var root = PropertyEditor.INSTANCE.Setup("Ui/PropertyEditors/MediaBlockEditor",
                "Media Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                }, instanceID);

            url = root.Q<TextField>("url");
            rotationX = root.Q<TextField>("x");
            rotationY = root.Q<TextField>("y");
            rotationZ = root.Q<TextField>("z");
            width = root.Q<TextField>("w");
            height = root.Q<TextField>("h");
            detectCollision = root.Q<Toggle>("collisionDetect");
        }

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
                detectCollision = detectCollision.value
            };
        }

        public void SetValue(MediaBlockProperties value)
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
                width.value = DEFAULT_DIMENSION.ToString();
                height.value = DEFAULT_DIMENSION.ToString();
                detectCollision.value = true;
                return;
            }

            url.value = value.url;
            width.value = value.width.ToString();
            height.value = value.height.ToString();
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