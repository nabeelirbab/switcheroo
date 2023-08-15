using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Database.Schema
{
    public class Category
    {
        public Category(string name)
        {
            Name = name;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public List<ItemCategory> ItemCategories { get; set; } = new List<ItemCategory>();
    }
}
