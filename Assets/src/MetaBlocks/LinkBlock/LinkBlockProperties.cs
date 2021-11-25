namespace src.MetaBlocks.LinkBlock
{
    [System.Serializable]
    public class LinkBlockProperties
    {
        public string url;
        public int[] pos;

        public LinkBlockProperties()
        {
        }

        public LinkBlockProperties(LinkBlockProperties obj)
        {
            if (obj != null)
            {
                url = obj.url;
                pos = obj.pos;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;

            return obj is LinkBlockProperties props &&
                   url == props.url &&
                   pos[0] == props.pos[0] &&
                   pos[1] == props.pos[1] &&
                   pos[2] == props.pos[2];
        }

        public bool IsEmpty()
        {
            return (url == null || url.Length == 0) && pos == null;
        }
    }
}
