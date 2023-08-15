using System.Collections.Generic;

namespace API.GraphQL.Items.Models
{
    public class ItemInput
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal AskingPrice { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool IsSwapOnly { get; set; }
        public List<string> Categories { get; set; } = null!;
        public List<string> ImageUrls { get; set; } = null!;
    }
}
