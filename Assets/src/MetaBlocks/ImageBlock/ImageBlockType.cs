public class ImageBlockType : MetaBlockType
{
    public ImageBlockType(byte id) : base(id, "image", typeof(ImageBlockObject), typeof(MediaBlockProperties))
    {
    }
}
