using System.Collections.Generic;

namespace Source.Ui.AssetsInventory.Models
{
    public class SearchCriteria
    {
        public int? lastId { get; set; }
        public int limit { get; set; }
        public Dictionary<string, object> searchTerms { get; set; }
    }
}