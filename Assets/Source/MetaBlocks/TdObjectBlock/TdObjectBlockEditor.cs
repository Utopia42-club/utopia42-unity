using System;
using System.Collections.Generic;
using Source.Model;
using Source.Ui.Utils;
using UnityEngine.UIElements;

namespace Source.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockEditor
    {
        private TextField url;

        private TextField scaleX;
        private TextField scaleY;
        private TextField scaleZ;

        private TextField rotationX;
        private TextField rotationY;
        private TextField rotationZ;

        private Toggle detectCollision;
        private DropdownField type;
        
        public TdObjectBlockEditor(Action<TdObjectBlockProperties> onSave, int instanceID)
        {
            var root = PropertyEditor.INSTANCE.Setup("Ui/PropertyEditors/TdObjectBlockEditor",
                "3D Block Properties", () =>
                {
                    onSave(GetValue());
                    PropertyEditor.INSTANCE.Hide();
                }, instanceID);

            url = root.Q<TextField>("url");
            rotationX = root.Q<TextField>("rotationX");
            rotationY = root.Q<TextField>("rotationY");
            rotationZ = root.Q<TextField>("rotationZ");
            scaleX = root.Q<TextField>("scaleX");
            scaleY = root.Q<TextField>("scaleY");
            scaleZ = root.Q<TextField>("scaleZ");
            detectCollision = root.Q<Toggle>("collisionDetect");
            type = root.Q<DropdownField>("type");
            type.choices = new List<string> {"OBJ (zip)", "GLB"};
            type.index = 0;
        }
        
        public TdObjectBlockProperties GetValue()
        {
            if (HasValue(url) && HasValue(scaleX) && HasValue(scaleY) && HasValue(scaleZ)
                && HasValue(rotationX) && HasValue(rotationY) && HasValue(rotationZ))
            {
                var props = new TdObjectBlockProperties();
                props.url = url.text.Trim();
                props.scale = new SerializableVector3(float.Parse(scaleX.text), float.Parse(scaleY.text),
                    float.Parse(scaleZ.text));
                props.rotation = new SerializableVector3(float.Parse(rotationX.text), float.Parse(rotationY.text),
                    float.Parse(rotationZ.text));
                props.detectCollision = detectCollision.value;
                props.type = type.index == 0
                    ? TdObjectBlockProperties.TdObjectType.OBJ
                    : TdObjectBlockProperties.TdObjectType.GLB;
                return props;
            }

            return null;
        }

        public void SetValue(TdObjectBlockProperties value)
        {
            if (value == null)
            {
                url.value = "";
                scaleX.value = "1";
                scaleY.value = "1";
                scaleZ.value = "1";
                rotationX.value = "0";
                rotationY.value = "0";
                rotationZ.value = "0";
                detectCollision.value = true;
                type.index = 0;
                return;
            }

            url.value = value.url == null ? "" : value.url;
            if (value.scale != null)
            {
                scaleX.value = value.scale.x.ToString();
                scaleY.value = value.scale.y.ToString();
                scaleZ.value = value.scale.z.ToString();
            }

            if (value.rotation != null)
            {
                rotationX.value = value.rotation.x.ToString();
                rotationY.value = value.rotation.y.ToString();
                rotationZ.value = value.rotation.z.ToString();
            }

            detectCollision.value = value.detectCollision;
            type.index = value.type == TdObjectBlockProperties.TdObjectType.OBJ ? 0 : 1;
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