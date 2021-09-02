public class ImageBlockTpe : MetaBlockType
{
    public ImageBlockTpe(byte id) : base(id, "image", typeof(ImageBlockObject), typeof(MediaBlockProperties))
    {
    }
}
