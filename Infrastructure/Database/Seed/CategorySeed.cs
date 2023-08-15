using System;
using System.Collections.Generic;
using Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Seed
{
    public static class CategorySeed
    {
        public static IEnumerable<Category> SeedCategories(this ModelBuilder modelBuilder)
        {
            var retVal = new List<Category>();
            var categories = new[] { "Electronics", "White Goods", "Clothing", "Furniture" };

            foreach (var category in categories)
            {
                var newCat = new Category(category)
                {
                    Id = Guid.NewGuid()
                };
                retVal.Add(newCat);
                modelBuilder.Entity<Category>().HasData(newCat);
            }

            return retVal;
        }
    }
}
