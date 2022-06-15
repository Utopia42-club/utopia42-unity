namespace src.AssetsInventory.Models
{
    public class FavoriteItem
    {
        public int? id { get; set; }
        public string walletId { get; set; }
        public Asset asset { get; set; }
        public uint? blockId { get; set; }
    }
}