namespace src.MetaBlocks.LinkBlock
{
    public class LinkBlockType : MetaBlockType
    {
        public LinkBlockType(byte id) : base(id, "link", typeof(LinkBlockObject), typeof(LinkBlockProperties))
        {
        }
    }
}
