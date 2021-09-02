[System.Serializable]
public class MediaBlockProperties
{
    public FaceProps front;
    public FaceProps back;
    public FaceProps right;
    public FaceProps left;
    public FaceProps top;
    public FaceProps bottom;

    public override bool Equals(object obj)
    {
        if (obj == this) return true;
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        var prop = obj as MediaBlockProperties;
        return Equals(front, prop.front) && Equals(back, prop.back) &&
            Equals(top, prop.top) && Equals(bottom, prop.bottom) &&
            Equals(left, prop.left) && Equals(right, prop.right);
    }


    [System.Serializable]
    public class FaceProps
    {
        public string url;
        public int width;
        public int height;

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            var prop = obj as FaceProps;
            return Equals(url, prop.url) && Equals(width, prop.width) && Equals(height, prop.height);
        }

    }
}
