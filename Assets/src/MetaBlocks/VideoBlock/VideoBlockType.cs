public class VideoBlockType : MetaBlockType
{
    public VideoBlockType(int id) : base(id, "video", typeof(VideoBlockObject), typeof(VideoBlockProperties))
    {
    }
}
