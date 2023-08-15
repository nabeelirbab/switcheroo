using System;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Schema
{
    public class ItemCategory
    {
        public ItemCategory(Guid itemId, Guid categoryId)
        {
            ItemId = itemId;
            CategoryId = categoryId;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public Item Item { get; set; } = null!;

        [Required]
        public Guid ItemId { get; set; }

        [Required]
        public Category Category { get; set; } = null!;

        [Required]
        public Guid CategoryId { get; set; }
    }
}