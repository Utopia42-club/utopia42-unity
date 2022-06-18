using System;
using Source.MetaBlocks.ImageBlock;
using Source.Model;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.NftBlock
{
    public class NftBlockEditor
    {
        private TextField collection;
        private TextField tokenId;

        private TextField rotationX;
        private TextField rotationY;
        private TextField rotationZ;

        private TextField width;
        private TextField height;
        private Toggle detectCollision;

        public NftBlockEditor(Action<NftBlockProperties> onSave)
        {
            var root = PropertyEditor.INSTANCE.Setup("UiDocuments/PropertyEditors/NftBlockEditor",
                "NFT Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                });

            collection = root.Q<TextField>("collection");
            tokenId = root.Q<TextField>("tokenId");
            rotationX = root.Q<TextField>("x");
            rotationY = root.Q<TextField>("y");
            rotationZ = root.Q<TextField>("z");
            width = root.Q<TextField>("w");
            height = root.Q<TextField>("h");
            detectCollision = root.Q<Toggle>("collisionDetect");
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(collection);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(tokenId);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(rotationX);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(rotationY);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(rotationZ);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(width);
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(height);
        }

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
                width = HasValue(width) ? int.Parse(width.text) : MediaBlockEditor.DEFAULT_DIMENSION,
                height = HasValue(height) ? int.Parse(height.text) : MediaBlockEditor.DEFAULT_DIMENSION,
                detectCollision = detectCollision.value
            };
        }

        public void SetValue(NftBlockProperties value)
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
                collection.value = "";
                tokenId.value = "";
                width.value = MediaBlockEditor.DEFAULT_DIMENSION.ToString();
                height.value = MediaBlockEditor.DEFAULT_DIMENSION.ToString();
                detectCollision.value = true;
                return;
            }

            collection.value = value.collection;
            tokenId.value = value.tokenId.ToString();
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