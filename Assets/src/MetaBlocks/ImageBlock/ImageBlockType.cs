public class ImageBlockType : MetaBlockType
{
    public ImageBlockType(int id) : base(id, "image", typeof(ImageBlockObject), typeof(MediaBlockProperties))
    {
    }
}
