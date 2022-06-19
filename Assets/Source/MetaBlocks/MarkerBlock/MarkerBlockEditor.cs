using System;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.MarkerBlock
{
    public class MarkerBlockEditor
    {
        private TextField name;

        public MarkerBlockEditor(Action<MarkerBlockProperties> onSave, int instanceID)
        {
            var root = PropertyEditor.INSTANCE.Setup("UiDocuments/PropertyEditors/MarkerBlockEditor",
                "Marker Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                }, instanceID);
            name = root.Q<TextField>("name");
            UiUtils.Utils.RegisterUiEngagementCallbacksForTextField(name);
        }


        public MarkerBlockProperties GetValue()
        {
            if (HasValue(name))
            {
                return new MarkerBlockProperties
                {
                    name = name.text.Trim()
                };
            }

            return null;
        }

        public void SetValue(MarkerBlockProperties value)
        {
            if (value == null)
            {
                name.value = "";
                return;
            }

            name.value = value.name == null ? "" : value.name;
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