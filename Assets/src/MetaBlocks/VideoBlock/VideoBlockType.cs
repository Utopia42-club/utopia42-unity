public class VideoBlockType : MetaBlockType
{
    public VideoBlockType(byte id) : base(id, "video", typeof(VideoBlockObject), typeof(VideoBlockProperties))
    {
    }
}
