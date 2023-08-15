using System;
using System.Collections.Generic;
using Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Seed
{
    public static class OfferSeed
    {
        public static Dictionary<Guid, Offer> SeedOffers(this ModelBuilder modelBuilder, List<KeyValuePair<Guid, Guid>> itemPairs)
        {
            var retVal = new Dictionary<Guid, Offer>();
            var random = new Random();
            var now = DateTime.UtcNow;

            var potentialUserIds = new[] { UserSeed.AdminId, UserSeed.TestUserId };

            for (var x = 0; x < itemPairs.Count; x++)
            {
                var itemKvp = itemPairs[x];
                var id = Guid.NewGuid();
                var randomUserId = potentialUserIds[random.Next(0, 2)];
                var newOffer = new Offer(
                    itemKvp.Key,
                    itemKvp.Value
                )
                {
                    Id = id,
                    CreatedByUserId = randomUserId,
                    UpdatedByUserId = randomUserId,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                retVal.Add(id, newOffer);
                modelBuilder.Entity<Offer>().HasData(newOffer);
            }


            MessageSeed.SeedMessages(modelBuilder, retVal.Values);

            return retVal;
        }
    }
}