using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Domain.Categories;
using Domain.Offers;
using Infrastructure.Database;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Offers
{
    public class OfferRepository : IOfferRepository
    {
        private readonly SwitcherooContext db;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;


        public OfferRepository(SwitcherooContext db, ILogger<ExceptionHandlerMiddleware> logger)
        {
            this.db = db;
            _logger = logger;
        }

        public async Task<Offer>? CreateOffer(Offer offer)
        {
            var now = DateTime.UtcNow;
            Offer? myoffer = null;
            try
            {
                if (!offer.CreatedByUserId.HasValue || !offer.UpdatedByUserId.HasValue)
                    throw new InfrastructureException("No createdByUserId provided");

                var match = db.Offers.Where(x => x.SourceItemId.Equals(offer.TargetItemId) && x.TargetItemId.Equals(offer.SourceItemId)).FirstOrDefault();
                if (match != null)
                {
                    match.TargetStatus = Database.Schema.OfferStatus.Initiated;
                    await db.SaveChangesAsync();
                    myoffer = await GetOfferById(match.CreatedByUserId, match.Id);
                }
                else
                {
                    if (offer.Cash != null)
                    {
                        // For 20% limit
                        var lowerAmountLimit = Decimal.Multiply((decimal)offer.Cash, (decimal)0.80);
                        var upperAmountBound = Decimal.Multiply((decimal)offer.Cash, (decimal)1.20);
                        if(lowerAmountLimit< offer.Cash && offer.Cash > upperAmountBound)
                        {
                            var newDbOffer = new Database.Schema.Offer(offer.SourceItemId, offer.TargetItemId)
                            {
                                CreatedByUserId = offer.CreatedByUserId.Value,
                                UpdatedByUserId = offer.UpdatedByUserId.Value,
                                CreatedAt = now,
                                UpdatedAt = now,
                                Cash = offer.Cash,
                                SourceStatus = Database.Schema.OfferStatus.Initiated
                            };

                            await db.Offers.AddAsync(newDbOffer);
                            await db.SaveChangesAsync();

                            myoffer = await GetOfferById(newDbOffer.CreatedByUserId, newDbOffer.Id);
                        }
                        else
                        {
                            throw new ArgumentException($"you can only offer from {(int)lowerAmountLimit}$ to {(int)upperAmountBound}$ against this product");
                        }
                    }
                    else
                    {
                        var newDbOffer = new Database.Schema.Offer(offer.SourceItemId, offer.TargetItemId)
                        {
                            CreatedByUserId = offer.CreatedByUserId.Value,
                            UpdatedByUserId = offer.UpdatedByUserId.Value,
                            CreatedAt = now,
                            UpdatedAt = now,
                            Cash = offer.Cash,
                            SourceStatus = Database.Schema.OfferStatus.Initiated
                        };

                        await db.Offers.AddAsync(newDbOffer);
                        await db.SaveChangesAsync();

                        myoffer = await GetOfferById(newDbOffer.CreatedByUserId, newDbOffer.Id);
                    }
                }
                return myoffer;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled exception occurred: {ex}");
                throw;
            }
        }

        public async Task<bool> DeleteOffer(Guid Id)
        {
            try
            {
                var offer = await db.Offers
                    .Where(u => u.Id == Id)
                    .SingleOrDefaultAsync();
                if (offer == null)
                {
                    return false;
                }

                db.Offers.Remove(offer);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled exception occurred: {ex}");
                return false;
            }
        }

        public async Task<IEnumerable<Offer>> GetAllOffers(Guid userId)
        {
            try
            {
                // Step 1: Retrieve myItems
                var myItems = await db.Items
                   .Where(z => z.CreatedByUserId == userId)
                   .Select(z => z.Id)
                   .ToArrayAsync();

                // Step 2: Retrieve offers using myItems
                var offers = await db.Offers
                    .Where(z => myItems.Contains(z.SourceItemId) || myItems.Contains(z.TargetItemId))
                    .Select(offer => new Offer(
                    offer.Id,
                    offer.SourceItemId,
                    offer.TargetItemId,
                    offer.Cash,
                    offer.CreatedByUserId,
                    offer.UpdatedByUserId,
                    offer.CreatedAt.DateTime,
                    (int)offer.SourceStatus,
                    (int)offer.TargetStatus))
                    .ToListAsync();

                return offers;
            }

            catch (Exception ex)
            {
                _logger.LogError($"An unhandled exception occurred:{ex}");
                return Enumerable.Empty<Offer>();
            }
        }

        public async Task<Offer> GetOfferById(Guid userId, Guid offerId)
        {
            var myItems = await db.Items
                .Where(z => z.CreatedByUserId == userId)
                .Select(z => z.Id)
                .ToArrayAsync();

            var offer = await db.Offers
                .SingleOrDefaultAsync(z => z.Id == offerId);

            if (!myItems.Contains(offer.SourceItemId) && !myItems.Contains(offer.TargetItemId))
            {
                throw new SecurityException("You cant access this offer");
            }

            return new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.Date, (int)offer.SourceStatus, (int)offer.TargetStatus);
        }

        public async Task<Offer> MarkMessagesAsRead(Guid userId, Guid offerId)
        {
            var myItems = await db.Items
                .Where(z => z.CreatedByUserId == userId)
                .Select(z => z.Id)
                .ToArrayAsync();

            var offer = await db.Offers
                .SingleOrDefaultAsync(z => z.Id == offerId);

            if (!myItems.Contains(offer.SourceItemId) && !myItems.Contains(offer.TargetItemId))
            {
                throw new SecurityException("You cant access this offer");
            }

            var messages = await db.Messages
                .Where(z => z.OfferId == offerId && z.CreatedByUserId != userId)
                .ToArrayAsync();

            foreach (var message in messages)
            {
                message.MessageReadAt = DateTime.Now;
            }

            await db.SaveChangesAsync();

            return new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.Date, (int)offer.SourceStatus, (int)offer.TargetStatus);
        }
    }
}
