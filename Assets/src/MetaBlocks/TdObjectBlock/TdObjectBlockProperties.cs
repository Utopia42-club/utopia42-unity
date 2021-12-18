using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    [System.Serializable]
    public class TdObjectBlockProperties
    {
        public string url;
        public Vector3 scale;
        public Vector3 eulerAngles;

        public TdObjectBlockProperties()
        {

        }

        public TdObjectBlockProperties(TdObjectBlockProperties obj)
        {
            if (obj != null)
            {
                url = obj.url;
                scale = obj.scale;
                eulerAngles = obj.eulerAngles;
            }
        }
        
        public void SetProps(string url)
        {
            this.url = url;
            // TODO: set scale and eulerAngles ?
            // TODO: position ?
        }
        
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as TdObjectBlockProperties;
            return Equals(url, prop.url) && Equals(scale, prop.scale) && Equals(eulerAngles, prop.eulerAngles);
        }

        public bool IsEmpty()
        {
            return (url == null || url.Equals("")) && scale == null && eulerAngles == null;
        }
    }
}
