namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockType : MetaBlockType
    {
        public TdObjectBlockType(byte id) : base(id, "3d_object", typeof(TdObjectBlockObject), typeof(TdObjectBlockProperties))
        {
        }
    }
}
