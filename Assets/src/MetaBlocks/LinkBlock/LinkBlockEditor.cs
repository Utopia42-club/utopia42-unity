using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.LinkBlock
{
    public class LinkBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/LinkBlockEditor";

        public TMP_Dropdown type;
        public InputField url;
        public InputField x;
        public InputField y;
        public InputField z;
        public GameObject gameLinkEditor;
        public GameObject webLinkEditor;


        private void Update()
        {
            webLinkEditor.SetActive(type.value == 0);
            gameLinkEditor.SetActive(type.value == 1);
        }


        public LinkBlockProperties GetValue()
        {
            int value = type.value;

            if (value == 0)
            {
                if (HasValue(url))
                {
                    var props = new LinkBlockProperties();
                    props.url = url.text;
                    return props;
                }
            }
            else
            {
                if (HasValue(x) && HasValue(y) && HasValue(z))
                {
                    var props = new LinkBlockProperties();
                    props.pos = new int[] {int.Parse(x.text), int.Parse(y.text), int.Parse(z.text)};
                    return props;
                }
            }

            return null;
        }

        public void SetValue(LinkBlockProperties value)
        {
            type.value = (value == null || value.pos != null) ? 1 : 0;
            url.text = value == null ? "" : value.url;
            bool noPos = value == null || value.pos == null;
            if (noPos)
            {
                x.text = null;
                y.text = null;
                z.text = null;
            }
            else
            {
                x.text = value.pos[0].ToString();
                y.text = value.pos[1].ToString();
                z.text = value.pos[2].ToString();
            }
        }

        private bool HasValue(InputField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}