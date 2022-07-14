using System.Collections.Generic;

namespace Source.Ui.AssetInventory.Models
{
    public class SearchCriteria
    {
        public int? lastId { get; set; }
        public int limit { get; set; }
        public Dictionary<string, object> searchTerms { get; set; }

        public SearchCriteria Clone()
        {
            return new SearchCriteria()
            {
                lastId = lastId,
                limit = limit,
                searchTerms = new Dictionary<string, object>(searchTerms)
            };
        }
    }
}