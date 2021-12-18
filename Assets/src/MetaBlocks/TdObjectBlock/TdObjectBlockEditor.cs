using UnityEngine;
using UnityEngine.UI;

namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockEditor : MonoBehaviour
    {
        public static readonly string PREFAB = "MetaBlocks/TdObjectBlockEditor";
        [SerializeField]
        public InputField url;

        public string GetValue()
        {
            if (HasValue(url))
            {
                return url.text;
            }
            return null;
        }
        
        public void SetValue(string url)
        {
            this.url.text = url == null ? "" : url;
        }

        private bool HasValue(InputField f)
        {
            return f.text != null && f.text.Length > 0;
        }
    }
}

