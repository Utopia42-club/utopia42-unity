namespace src.MetaBlocks.NftBlock
{
    public class NftBlockType : MetaBlockType
    {
        public NftBlockType(byte id) : base(id, "nft", typeof(NftBlockObject), typeof(NftBlockProperties))
        {
        }
    }
}
