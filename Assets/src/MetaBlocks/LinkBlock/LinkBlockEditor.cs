using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace src.MetaBlocks.LinkBlock
{
    public class LinkBlockEditor
    {
        private readonly DropdownField typeField;
        private readonly TextField urlField;
        private readonly VisualElement positionBox;
        private readonly TextField xField;
        private readonly TextField yField;
        private readonly TextField zField;

        public LinkBlockEditor(Action<LinkBlockProperties> onSave)
        {
            var root = PropertyEditor.INSTANCE.Setup("UiDocuments/PropertyEditors/LinkBlockEditor",
                "Link Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                });

            typeField = root.Q<DropdownField>("type");
            typeField.choices = new List<string> {"Web Link", "Game Link"};
            urlField = root.Q<TextField>("url");
            positionBox = root.Q<VisualElement>("position");
            xField = root.Q<TextField>("x");
            yField = root.Q<TextField>("y");
            zField = root.Q<TextField>("z");

            typeField.RegisterValueChangedCallback(evt => UpdateFieldsVisibility());
            UpdateFieldsVisibility();
        }

        private void UpdateFieldsVisibility()
        {
            switch (typeField.index)
            {
                case 0:
                    urlField.style.display = DisplayStyle.Flex;
                    positionBox.style.display = DisplayStyle.None;
                    break;
                case 1:
                    urlField.style.display = DisplayStyle.None;
                    positionBox.style.display = DisplayStyle.Flex;
                    break;
                default:
                    urlField.style.display = DisplayStyle.None;
                    positionBox.style.display = DisplayStyle.None;
                    break;
            }
        }

        public LinkBlockProperties GetValue()
        {
            var value = typeField.index;

            if (value == 0)
            {
                if (HasValue(urlField))
                {
                    var props = new LinkBlockProperties
                    {
                        url = urlField.text
                    };
                    return props;
                }
            }
            else
            {
                if (HasValue(xField) && HasValue(yField) && HasValue(zField))
                {
                    var props = new LinkBlockProperties
                    {
                        pos = new[] {int.Parse(xField.text), int.Parse(yField.text), int.Parse(zField.text)}
                    };
                    return props;
                }
            }

            return null;
        }

        public void SetValue(LinkBlockProperties value)
        {
            typeField.index = (value == null || value.pos != null) ? 1 : 0;
            urlField.value = value == null ? "" : value.url;
            bool noPos = value == null || value.pos == null;
            if (noPos)
            {
                xField.value = null;
                yField.value = null;
                zField.value = null;
            }
            else
            {
                xField.value = value.pos[0].ToString();
                yField.value = value.pos[1].ToString();
                zField.value = value.pos[2].ToString();
            }
        }

        public void Show()
        {
            PropertyEditor.INSTANCE.Show();
        }

        private bool HasValue(TextField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}