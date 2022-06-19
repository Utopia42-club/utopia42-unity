using System;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.TeleportBlock
{
    public class TeleportBlockEditor
    {
        private TextField posX;
        private TextField posY;
        private TextField posZ;

        public TeleportBlockEditor(Action<TeleportBlockProperties> onSave, int instanceID)
        {
            var root = PropertyEditor.INSTANCE.Setup("Ui/PropertyEditors/TeleportBlockEditor",
                "Teleport Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                }, instanceID);
            posX = root.Q<TextField>("x");
            posY = root.Q<TextField>("y");
            posZ = root.Q<TextField>("z");
            Ui.Utils.Utils.RegisterUiEngagementCallbacksForTextField(posX);
            Ui.Utils.Utils.RegisterUiEngagementCallbacksForTextField(posY);
            Ui.Utils.Utils.RegisterUiEngagementCallbacksForTextField(posZ);
        }
        
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
                posX.value = value.destination[0].ToString();
                posY.value = value.destination[1].ToString();
                posZ.value = value.destination[2].ToString();
            }
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