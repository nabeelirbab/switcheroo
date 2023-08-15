using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Seed
{
    public static class ItemSeed
    {
        public static Dictionary<Guid, Item> SeedItems(this ModelBuilder modelBuilder, IEnumerable<Category> categories)
        {
            var retVal = new Dictionary<Guid, Item>();
            var random = new Random();
            var now = DateTime.UtcNow;

            var itemPairs = new List<KeyValuePair<Guid, Guid>>();
            foreach (var category in categories)
            {
                Guid? tempId = null;
                foreach (var _ in Enumerable.Range(0, random.Next(5, 10)))
                {
                    var userId = UserSeed.AdminId;
                    var id = Guid.NewGuid();
                    if (!tempId.HasValue)
                    {
                        tempId = id;
                    }
                    else
                    {
                        itemPairs.Add(new KeyValuePair<Guid, Guid>(tempId.Value, id));
                        userId = UserSeed.TestUserId;
                    }
                    
                    var newItem = new Item(
                        title: LoremIpsum.GetWordsBetween(6, 19),
                        description: LoremIpsum.GetWordsBetween(5, 20, 2, 6),
                        askingPrice: random.Next(10, 50),
                        isHidden: random.Next(1, 2) == 2,
                        isSwapOnly: random.Next(1, 2) == 2,
                        latitude: (decimal) -37.7913312,
                        longitude: (decimal) 145.2608988
                    )
                    {
                        Id = id,
                        CreatedByUserId = userId,
                        CreatedAt = now,
                        UpdatedByUserId = userId,
                        UpdatedAt = now
                    };

                    retVal.Add(id, newItem);
                    modelBuilder.Entity<Item>().HasData(newItem);

                    SeedItemCategories(id, category, modelBuilder);
                    SeedItemImages(id, modelBuilder);
                }
            }

            OfferSeed.SeedOffers(modelBuilder, itemPairs);

            return retVal;
        }

        private static void SeedItemImages(Guid id, ModelBuilder modelBuilder)
        {
            foreach (var itemImage in Enumerable
                            .Range(1, 4)
                            .Select(z => new ItemImage("https://picsum.photos/200/300", id) { Id = Guid.NewGuid() }))
            {
                modelBuilder.Entity<ItemImage>().HasData(itemImage);
            }
        }

        private static void SeedItemCategories(Guid id, Category category, ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemCategory>().HasData(new ItemCategory(id, category.Id) { Id = Guid.NewGuid() });
        }
    }
}
