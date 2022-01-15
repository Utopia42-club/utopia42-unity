namespace src.MetaBlocks.TdObjectBlock
{
    public class TdObjectBlockType : MetaBlockType
    {
        public const string Name = "3d_object";
        public TdObjectBlockType(byte id) : base(id, Name, typeof(TdObjectBlockObject), typeof(TdObjectBlockProperties))
        {
        }
    }
}
