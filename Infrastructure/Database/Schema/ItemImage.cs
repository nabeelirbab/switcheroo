using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Schema
{
    public class ItemImage
    {
        public ItemImage(string url, Guid itemId)
        {
            Url = url;
            ItemId = itemId;
        }
     
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public Item Item { get; set; } = null!;

        [Required]
        public Guid ItemId { get; set; }
    }
}
