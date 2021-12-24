using UnityEngine;

namespace src.MetaBlocks.TdObjectBlock
{
    [System.Serializable]
    public class TdObjectBlockProperties
    {
        public string url;
        public Vector3 scale = Vector3.one;
        public Vector3 offset = Vector3.zero;
        public Vector3 rotation = Vector3.zero;

        public TdObjectBlockProperties()
        {

        }

        public TdObjectBlockProperties(TdObjectBlockProperties obj)
        {
            if (obj != null)
            {
                url = obj.url;
                scale = obj.scale;
                offset = obj.offset;
                rotation = obj.rotation;
            }
        }
        
        public void UpdateProps(TdObjectBlockProperties props)
        {
            if(props == null) return;
            this.url = props.url;
            this.scale = props.scale;
            this.offset = props.offset;
            this.rotation = props.rotation;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as TdObjectBlockProperties;
            return Equals(url, prop.url) && Equals(scale, prop.scale) && Equals(offset, prop.offset) && Equals(rotation, prop.rotation);
        }

        public bool IsEmpty()
        {
            return url == null || url.Equals("");
        }
    }
}
