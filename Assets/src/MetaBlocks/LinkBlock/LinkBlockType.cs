public class LinkBlockType : MetaBlockType
{
    public LinkBlockType(int id) : base(id, "link", typeof(LinkBlockObject), typeof(LinkBlockProperties))
    {
    }
}
