using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Database.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Seed
{
    public static class MessageSeed
    {
        public static Dictionary<Guid, Message> SeedMessages(this ModelBuilder modelBuilder, IEnumerable<Offer> offers)
        {
            var retVal = new Dictionary<Guid, Message>();
            var random = new Random();
            var now = DateTime.UtcNow;

            foreach (var offer in offers)
            {
                var potentialUserIds = new [] { UserSeed.AdminId, UserSeed.TestUserId };
                foreach (var _ in Enumerable.Range(0, random.Next(5, 10)))
                {
                    var id = Guid.NewGuid();
                    var randomUserId = potentialUserIds[random.Next(0, 2)];
                    var newMessage = new Message(
                        offer.Id,
                        LoremIpsum.GetWordsBetween(2, 15),
                        null
                    )
                    {
                        Id = id,
                        CreatedByUserId = randomUserId,
                        UpdatedByUserId = randomUserId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    retVal.Add(id, newMessage);
                    modelBuilder.Entity<Message>().HasData(newMessage);
                }
            }

            return retVal;
        }
    }
}