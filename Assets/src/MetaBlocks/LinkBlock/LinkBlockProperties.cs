[System.Serializable]
public class LinkBlockProperties
{
    public FaceProps front;
    public FaceProps back;
    public FaceProps right;
    public FaceProps left;
    public FaceProps top;
    public FaceProps bottom;

    public LinkBlockProperties()
    {

    }

    public LinkBlockProperties(LinkBlockProperties obj)
    {
        if (obj != null)
        {
            left = obj.left;
            right = obj.right;
            top = obj.right;
            bottom = obj.bottom;
            front = obj.front;
            back = obj.back;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == this) return true;
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        var prop = obj as LinkBlockProperties;
        return Equals(front, prop.front) && Equals(back, prop.back) &&
            Equals(top, prop.top) && Equals(bottom, prop.bottom) &&
            Equals(left, prop.left) && Equals(right, prop.right);
    }

    public FaceProps GetFaceProps(Voxels.Face face)
    {
        if (face == Voxels.Face.BACK) return back;
        if (face == Voxels.Face.FRONT) return front;
        if (face == Voxels.Face.RIGHT) return right;
        if (face == Voxels.Face.LEFT) return left;
        if (face == Voxels.Face.TOP) return top;
        if (face == Voxels.Face.BOTTOM) return bottom;
        return null;
    }

    public void SetFaceProps(Voxels.Face face, FaceProps props)
    {
        if (face == Voxels.Face.BACK) back = props;
        if (face == Voxels.Face.FRONT) front = props;
        if (face == Voxels.Face.RIGHT) right = props;
        if (face == Voxels.Face.LEFT) left = props;
        if (face == Voxels.Face.TOP) top = props;
        if (face == Voxels.Face.BOTTOM) bottom = props;
    }

    public bool IsEmpty()
    {
        return back == null && front == null &&
            right == null && left == null &&
            top == null && bottom == null;
    }

    [System.Serializable]
    public class FaceProps
    {
        public int type;
        public string url;
        public int x;
        public int y;
        public int z;

        public override bool Equals(object obj)
        {
            return obj is FaceProps props &&
                   type == props.type &&
                   url == props.url &&
                   x == props.x &&
                   y == props.y &&
                   z == props.z;
        }
    }
}
