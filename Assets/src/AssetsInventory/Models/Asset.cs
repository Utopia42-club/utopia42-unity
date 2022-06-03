namespace src.AssetsInventory.Models
{
    public class Asset
    {
        public int id { get; set; }
        public string walletId { get; set; }
        public string name { get; set; }
        public string glbUrl { get; set; }
        public string thumbnailUrl { get; set; }
        public Category category { get; set; }
        public Pack pack { get; set; }
        public State state { get; set; }

        public enum State
        {
            PUBLIC,
            PRIVATE
        }
    }
}