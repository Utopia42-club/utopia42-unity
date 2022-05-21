using src.Model;
using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/TdObjectBlockEditor";
        [SerializeField] public InputField url;

        [SerializeField] public InputField scaleX;
        [SerializeField] public InputField scaleY;
        [SerializeField] public InputField scaleZ;

        [SerializeField] public InputField offsetX;
        [SerializeField] public InputField offsetY;
        [SerializeField] public InputField offsetZ;

        [SerializeField] public InputField rotationX;
        [SerializeField] public InputField rotationY;
        [SerializeField] public InputField rotationZ;

        [SerializeField] public Toggle detectCollision;
        [SerializeField] public Dropdown type;

        public TdObjectBlockProperties GetValue()
        {
            if (HasValue(url) && HasValue(scaleX) && HasValue(scaleY) && HasValue(scaleZ)
                && HasValue(offsetX) && HasValue(offsetY) && HasValue(offsetZ)
                && HasValue(rotationX) && HasValue(rotationY) && HasValue(rotationZ))
            {
                var props = new TdObjectBlockProperties();
                props.url = url.text.Trim();
                props.scale = new SerializableVector3(float.Parse(scaleX.text), float.Parse(scaleY.text),
                    float.Parse(scaleZ.text));
                props.offset = new SerializableVector3(float.Parse(offsetX.text), float.Parse(offsetY.text),
                    float.Parse(offsetZ.text));
                props.rotation = new SerializableVector3(float.Parse(rotationX.text), float.Parse(rotationY.text),
                    float.Parse(rotationZ.text));
                props.detectCollision = detectCollision.isOn;
                props.type = type.value == 0
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
                url.text = "";
                scaleX.text = "1";
                scaleY.text = "1";
                scaleZ.text = "1";
                offsetX.text = "0";
                offsetY.text = "0";
                offsetZ.text = "0";
                rotationX.text = "0";
                rotationY.text = "0";
                rotationZ.text = "0";
                detectCollision.isOn = true;
                type.value = 0;
                return;
            }

            url.text = value.url == null ? "" : value.url;
            if (value.scale != null)
            {
                scaleX.text = value.scale.x.ToString();
                scaleY.text = value.scale.y.ToString();
                scaleZ.text = value.scale.z.ToString();
            }

            if (value.offset != null)
            {
                offsetX.text = value.offset.x.ToString();
                offsetY.text = value.offset.y.ToString();
                offsetZ.text = value.offset.z.ToString();
            }

            if (value.rotation != null)
            {
                rotationX.text = value.rotation.x.ToString();
                rotationY.text = value.rotation.y.ToString();
                rotationZ.text = value.rotation.z.ToString();
            }

            detectCollision.isOn = value.detectCollision;
            type.value = value.type == TdObjectBlockProperties.TdObjectType.OBJ ? 0 : 1;
        }

        private bool HasValue(InputField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}