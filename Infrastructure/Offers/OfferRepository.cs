using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Domain.Offers;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Domain.Services;
using System.Text.Json;
using Domain;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace Infrastructure.Offers
{
    public class OfferRepository : IOfferRepository
    {
        private readonly SwitcherooContext db;


        private readonly ILoggerManager _loggerManager;

        public OfferRepository(SwitcherooContext db, ILoggerManager loggerManager)
        {
            this.db = db;
            _loggerManager = loggerManager;
        }

        public async Task<Offer>? CreateOffer(Offer offer)
        {
            try
            {
                var now = DateTime.UtcNow;
                Offer? myoffer = null;

                var targetUserId = db.Items.Where(x => x.Id.Equals(offer.TargetItemId))
                           .Select(x => x.CreatedByUserId).FirstOrDefault();

                var userIds = new[] { offer.CreatedByUserId, targetUserId };

                var sourceUserFCMToken = await db.Users.Where(u => u.Id == offer.CreatedByUserId).Select(u => u.FCMToken).FirstOrDefaultAsync();
                var targetUserFCMToken = await db.Users.Where(u => u.Id == targetUserId).Select(u => u.FCMToken).FirstOrDefaultAsync();

                if (!offer.CreatedByUserId.HasValue || !offer.UpdatedByUserId.HasValue)
                    throw new InfrastructureException("No createdByUserId provided");

                var match = db.Offers.Include(o => o.SourceItem).Include(o => o.TargetItem)
                    .Where(x => (x.SourceItemId.Equals(offer.TargetItemId) && x.TargetItemId.Equals(offer.SourceItemId)) && (x.SourceItem.CreatedByUserId == offer.CreatedByUserId || x.TargetItem.CreatedByUserId == offer.CreatedByUserId || x.CreatedByUserId == offer.CreatedByUserId))
                    .FirstOrDefault();
                if (match != null)
                {
                    if (match.CreatedByUserId == offer.CreatedByUserId) throw new InfrastructureException($"Offer has been already created!!");
                    if ((match.Cash == null && offer.Cash == null) || (match.Cash != null && offer.Cash != null))
                    {
                        if (offer.Cash != match.Cash)
                        {
                            var newDbOffer = new Database.Schema.Offer(offer.SourceItemId, offer.TargetItemId)
                            {
                                CreatedByUserId = offer.CreatedByUserId.Value,
                                UpdatedByUserId = offer.UpdatedByUserId.Value,
                                CreatedAt = now,
                                UpdatedAt = now,
                                Cash = offer.Cash,
                                SourceStatus = Database.Schema.OfferStatus.Initiated,
                                IsRead = false
                            };

                            await db.Offers.AddAsync(newDbOffer);
                            if (!string.IsNullOrEmpty(targetUserFCMToken))
                            {
                                var app = FirebaseApp.DefaultInstance;
                                var messaging = FirebaseMessaging.GetMessaging(app);

                                var message = new FirebaseAdmin.Messaging.Message()
                                {
                                    Token = targetUserFCMToken,
                                    Notification = new Notification
                                    {
                                        Title = "New Cash Offer",
                                        Body = "You have a new cash offer"
                                        // Other notification parameters can be added here
                                    },
                                    Data = new Dictionary<string, string>
                                    {
                                        {"NavigateTo", "NewCashOffer"},
                                        {"TargetItemId", offer.TargetItemId.ToString()}
                                    }
                                };
                                string response = await messaging.SendAsync(message);
                            }
                            else
                            {
                                throw new InfrastructureException($"No FCM Token exist against this user");
                            }
                            await db.SaveChangesAsync();
                            myoffer = await GetOfferById(newDbOffer.CreatedByUserId, newDbOffer.Id);
                            return myoffer;
                        }
                        else
                        {
                            match.TargetStatus = Database.Schema.OfferStatus.Initiated;
                            //await SendMatchingPushNotification(sourceUserFCMToken);
                            await SendMatchingPushNotification(targetUserFCMToken, match.SourceItemId.ToString(), match.SourceItem.MainImageUrl, match.TargetItemId.ToString(), match.TargetItem.MainImageUrl);
                            await db.SaveChangesAsync();
                            myoffer = await GetOfferById(match.CreatedByUserId, match.Id);
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
                            Cash = null,
                            SourceStatus = Database.Schema.OfferStatus.Initiated,
                            IsRead = false
                        };

                        await db.Offers.AddAsync(newDbOffer);
                        await db.SaveChangesAsync();
                        myoffer = await GetOfferById(newDbOffer.CreatedByUserId, newDbOffer.Id);
                        return myoffer;
                    }
                }
                else
                {

                    if (offer.Cash != null)
                    {
                        var targetitem = db.Items.Where(i => i.Id.Equals(offer.TargetItemId)).FirstOrDefault();
                        if (targetitem.IsSwapOnly)
                        {
                            if (offer.Cash >= 1)
                            {
                                // For 20% limit
                                // var lowerAmountLimit = Decimal.Multiply((decimal)targetitem.AskingPrice, (decimal)0.60);
                                // var upperAmountBound = Decimal.Multiply((decimal)targetitem.AskingPrice, (decimal)1.40);

                                // if (lowerAmountLimit < offer.Cash && offer.Cash < upperAmountBound)
                                // {
                                var newDbOffer = new Database.Schema.Offer(offer.SourceItemId, offer.TargetItemId)
                                {
                                    CreatedByUserId = offer.CreatedByUserId.Value,
                                    UpdatedByUserId = offer.UpdatedByUserId.Value,
                                    CreatedAt = now,
                                    UpdatedAt = now,
                                    Cash = offer.Cash,
                                    SourceStatus = Database.Schema.OfferStatus.Initiated,
                                    IsRead = false
                                };

                                await db.Offers.AddAsync(newDbOffer);
                                try
                                {
                                    if (!string.IsNullOrEmpty(targetUserFCMToken))
                                    {
                                        var app = FirebaseApp.DefaultInstance;
                                        var messaging = FirebaseMessaging.GetMessaging(app);

                                        var message = new FirebaseAdmin.Messaging.Message()
                                        {
                                            Token = targetUserFCMToken,
                                            Notification = new Notification
                                            {
                                                Title = "New Cash Offer",
                                                Body = "You have a new cash offer",

                                                // Other notification parameters can be added here
                                            },
                                            Data = new Dictionary<string, string>
                                            {
                                                {"NavigateTo", "NewCashOffer"},
                                                {"TargetItemId", offer.TargetItemId.ToString()}
                                            }
                                        };
                                        string response = await messaging.SendAsync(message);
                                    }
                                    else
                                    {
                                        throw new InfrastructureException($"No FCM Token exist against this user");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                                await db.SaveChangesAsync();
                                myoffer = await GetOfferById(newDbOffer.CreatedByUserId, newDbOffer.Id);
                                // }
                                // else
                                // {
                                //    throw new InfrastructureException($"You can only offer from {(int)lowerAmountLimit}$ to {(int)upperAmountBound}$ against this product");
                                //}
                            }
                            else
                            {
                                throw new InfrastructureException("Pleas give a valid cash offer");
                            }
                        }
                        else
                        {
                            throw new InfrastructureException("you cannot give cash offer to this user");
                        }
                    }
                    else
                    {
                        var newDbOffer = new Database.Schema.Offer(offer.SourceItemId, offer.TargetItemId)
                        {
                            CreatedByUserId = offer.CreatedByUserId.Value,
                            UpdatedByUserId = offer.UpdatedByUserId.Value,
                            Cash = null,
                            CreatedAt = now,
                            UpdatedAt = now,
                            SourceStatus = Database.Schema.OfferStatus.Initiated,
                            IsRead = false
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
                _loggerManager.LogError($"Exception: {ex.Message}");
                throw new InfrastructureException($"Exception: {ex.Message}");
            }
        }

        private async Task SendMatchingPushNotification(string? userFCMToken, string sourceItemId, string sourceItemImage, string targetItemId, string targetItemImage)
        {
            try
            {
                if (!string.IsNullOrEmpty(userFCMToken))
                {
                    var app = FirebaseApp.DefaultInstance;
                    var messaging = FirebaseMessaging.GetMessaging(app);

                    var message = new FirebaseAdmin.Messaging.Message()
                    {
                        Token = userFCMToken,
                        Notification = new Notification
                        {
                            Title = "Product Matched",
                            Body = "One of your product is matched"
                            // Other notification parameters can be added here
                        },
                        Data = new Dictionary<string, string>
                    {
                        {"NavigateTo", "NewMatchingOffer"},
                        {"IsMatch", "true"},
                        {"SourceItemId", sourceItemId},
                        {"SourceItemImage", sourceItemImage},
                        {"TargetItemId", targetItemId},
                        {"TargetItemImage", targetItemImage}
                    }
                    };
                    string response = await messaging.SendAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.InnerException);
            }

        }
        public async Task<bool> DeleteOffer(Guid Id, Guid userId)
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

                offer.IsDeleted = true;
                offer.DeletedByUserId = userId;
                offer.DeletedAt = DateTimeOffset.Now;
                await db.SaveChangesAsync();

                var targetUserId = db.Items.Where(x => x.Id.Equals(offer.TargetItemId))
                        .Select(x => x.CreatedByUserId).FirstOrDefault();

                var sourceUserId = db.Items.Where(x => x.Id.Equals(offer.SourceItemId))
                        .Select(x => x.CreatedByUserId).FirstOrDefault();

                if (userId != sourceUserId)
                {

                    var userFCMToken = db.Users
                    .Where(x => x.Id == sourceUserId)
                    .Select(x => x.FCMToken).FirstOrDefault();

                    var app = FirebaseApp.DefaultInstance;
                    var messaging = FirebaseMessaging.GetMessaging(app);

                    var message = new FirebaseAdmin.Messaging.Message()
                    {
                        Token = userFCMToken,
                        Notification = new Notification
                        {
                            Title = "Offer Rejected",
                            Body = "One of your offer is rejected"
                            // Other notification parameters can be added here
                        }
                    };
                    string response = await messaging.SendAsync(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<bool> RestoreOffer(Guid offerId)
        {
            try
            {
                var offer = await db.Offers
                    .Where(o => o.Id == offerId && o.IsDeleted)
                    .SingleOrDefaultAsync();
                if (offer == null)
                {
                    return false; // No deleted offer found with the given ID.
                }

                offer.IsDeleted = false;
                offer.DeletedByUserId = null;
                offer.DeletedAt = null;
                await db.SaveChangesAsync();

                var userFcmTokens = db.Items.Where(x => x.Id.Equals(offer.TargetItemId) || x.Id.Equals(offer.SourceItemId))
                    .Select(x => x.CreatedByUser.FCMToken).ToList();

                if (userFcmTokens.Count > 0)
                {
                    var app = FirebaseApp.DefaultInstance;
                    var messaging = FirebaseMessaging.GetMessaging(app);

                    var message = new FirebaseAdmin.Messaging.MulticastMessage()
                    {
                        Tokens = userFcmTokens,
                        Notification = new Notification
                        {
                            Title = "Offer Restored",
                            Body = "One of your offers has been restored"
                        }
                    };
                    var response = await messaging.SendMulticastAsync(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<bool> AcceptOffer(Guid offerId)
        {
            try
            {
                var offer = db.Offers.Where(u => u.Id == offerId).FirstOrDefault();

                offer.TargetStatus = Database.Schema.OfferStatus.Initiated;

                var userFCMToken = db.Users
                    .Where(x => x.Id == offer.CreatedByUserId)
                    .Select(x => x.FCMToken).FirstOrDefault();

                var newDummyMessage = new Domain.Offers.Message(
                         Guid.NewGuid(),
                             offer.CreatedByUserId,
                             offerId,
                             offer.Cash,
                             offer.CreatedByUserId,
                             "",
                             null,
                             offer.CreatedAt,
                             false
                         );
                string newDummyMessageJson = JsonSerializer.Serialize(newDummyMessage);
                var app = FirebaseApp.DefaultInstance;
                var messaging = FirebaseMessaging.GetMessaging(app);

                var message = new FirebaseAdmin.Messaging.Message()
                {
                    Token = userFCMToken,
                    Notification = new Notification
                    {
                        Title = "Offer Accpeted",
                        Body = "One of your offer is accepted"
                        // Other notification parameters can be added here
                    },
                    Data = new Dictionary<string, string>
                    {
                        {"NavigateTo", "OfferAccepted"},
                        {"TargetItemId", offer.TargetItemId.ToString()},
                        {"ChatListingStrigifiedObject", newDummyMessageJson}
                    }
                };
                string response = await messaging.SendAsync(message);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<bool> UnmatchOffer(Guid offerId)
        {
            try
            {
                var offer = db.Offers.Where(u => u.Id == offerId).FirstOrDefault();
                offer.TargetStatus = 0;
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }


        public async Task<IEnumerable<Offer>> GetCreatedOffers(Guid userId)
        {
            try
            {
                // Step 1: Retrieve myItems
                /* var myItems = await db.Items
                    .Where(z => z.CreatedByUserId == userId && z.IsSwapOnly == true)
                    .Select(z => z.Id)
                    .ToArrayAsync();*/

                // Step 2: Retrieve offers using myItems
                var offers = await db.Offers
                    .Where(z => z.CreatedByUserId.Equals(userId) && z.Cash != null && z.SourceStatus != z.TargetStatus)
                    .Select(offer => new Offer(
                    offer.Id,
                    offer.SourceItemId,
                    offer.TargetItemId,
                    offer.Cash,
                    offer.CreatedByUserId,
                    offer.UpdatedByUserId,
                    offer.CreatedAt.DateTime,
                    (int)offer.SourceStatus,
                    (int)offer.TargetStatus,
                    offer.IsRead))
                    .ToListAsync();

                return offers;
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<IEnumerable<Offer>> GetReceivedOffers(Guid userId)
        {
            try
            {
                // Step 1: Retrieve myItems
                var myItems = await db.Items
                   .Where(z => z.CreatedByUserId == userId && z.IsSwapOnly == true)
                   .Select(z => z.Id)
                   .ToArrayAsync();

                // Step 2: Retrieve offers using myItems
                var offers = await db.Offers
                    .Where(z => myItems.Contains(z.TargetItemId) && z.Cash != null)
                    .OrderByDescending(offer => offer.CreatedAt)
                    .Select(offer => new Offer(
                    offer.Id,
                    offer.SourceItemId,
                    offer.TargetItemId,
                    offer.Cash,
                    offer.CreatedByUserId,
                    offer.UpdatedByUserId,
                    offer.CreatedAt.DateTime,
                    (int)offer.SourceStatus,
                    (int)offer.TargetStatus,
                    offer.IsRead))
                    .ToListAsync();

                /*var filteredOffers = offers.Where(offer =>
                {
                    // Check for reciprocal offers
                    return offers.Any(otherOffer =>
                       offer.SourceItemId == otherOffer.TargetItemId &&
                       offer.TargetItemId == otherOffer.SourceItemId &&
                       offer.Cash != otherOffer.Cash
                    );
                }).ToList();

                // Keep only the first created offer in each reciprocal pair
                foreach (var offer in filteredOffers)
                {
                    var reciprocalOffer = offers.FirstOrDefault(otherOffer =>
                       offer.SourceItemId == otherOffer.TargetItemId &&
                       offer.TargetItemId == otherOffer.SourceItemId &&
                       offer.Cash != otherOffer.Cash
                    );

                    if (reciprocalOffer != null && offer.CreatedAt < reciprocalOffer.CreatedAt)
                    {
                        offers.Remove(reciprocalOffer);
                    }
                }

                offers.AddRange(filteredOffers);
                */

                return offers;
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<int> GetNotificationCount(Guid userId)
        {
            try
            {
                // Step 1: Retrieve myItems
                var myItems = await db.Items
                   .Where(z => z.CreatedByUserId == userId)
                   .Select(z => z.Id)
                   .ToArrayAsync();

                int countOfUnreadOffers = await db.Offers
                    .Where(z => myItems.Contains(z.TargetItemId))
                    .Where(offer => (int)offer.SourceStatus != (int)offer.TargetStatus)
                    .Where(offer => offer.IsRead == false)
                    .CountAsync();

                return countOfUnreadOffers;
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<bool> MarkNotificationRead(Guid userId)
        {
            try
            {
                // Step 1: Retrieve myItems
                var myItems = await db.Items
                   .Where(z => z.CreatedByUserId == userId)
                   .Select(z => z.Id)
                   .ToArrayAsync();

                var offers = await db.Offers
                    .Where(z => myItems.Contains(z.TargetItemId))
                    .Where(offer => (int)offer.SourceStatus != (int)offer.TargetStatus)
                    .Where(offer => offer.IsRead == false)
                    .ToListAsync();

                foreach (var offer in offers)
                {
                    offer.IsRead = true;
                }

                await db.SaveChangesAsync();

                return true;
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Offer> GetOfferById(Guid userId, Guid offerId)
        {
            /* var myItems = await db.Items
                 .Where(z => z.CreatedByUserId == userId)
                 .Select(z => z.Id)
                 .ToArrayAsync();
 */
            var offer = await db.Offers
                .Where(o => o.Id.Equals(offerId))
                .Where(o => o.CreatedByUserId.Equals(userId))
                .SingleOrDefaultAsync(z => z.Id == offerId);
            /*!myItems.Contains(offer.SourceItemId) || !myItems.Contains(offer.TargetItemId)*/
            if (offer == null)
            {
                throw new SecurityException("You cant access this offer");
            }

            return new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.Date, (int)offer.SourceStatus, (int)offer.TargetStatus, offer.IsRead);
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
                message.MessageReadAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            return new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.Date, (int)offer.SourceStatus, (int)offer.TargetStatus, offer.IsRead);
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
                    (int)offer.TargetStatus,
                    offer.IsRead))
                    .ToListAsync();

                return offers;
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<IEnumerable<Offer>> GetAllOffersByItemId(Guid itemId)
        {
            try
            {
                // Step 1: Retrieve myItems
                var myItems = await db.Items
                   .Where(z => z.Id == itemId)
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
                    (int)offer.TargetStatus,
                    offer.IsRead))
                    .ToListAsync();

                return offers;
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<Paginated<Offer>> GetAllMatchedOffers(int limit, string? cursor)
        {
            try
            {
                Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
                var query = db.Offers.IgnoreQueryFilters().Where(o => !(o.Cash > 0) && o.SourceStatus == o.TargetStatus).AsNoTracking();
                if (cursorGuid.HasValue)
                {
                    query = query.Where(item => item.Id.CompareTo(cursorGuid.Value) > 0);
                }
                var totalCountQuery = await query.CountAsync();
                var paginatedOffers = await query
                    .OrderBy(item => item.Id)
                    .Take(limit + 1)
                    .Select(offer => new Offer(
                    offer.Id,
                    offer.SourceItemId,
                    offer.TargetItemId,
                    offer.Cash,
                    offer.CreatedByUserId,
                    offer.UpdatedByUserId,
                    offer.CreatedAt.DateTime,
                    (int)offer.SourceStatus,
                    (int)offer.TargetStatus,
                    offer.IsRead)
                    {
                        IsDeleted = offer.IsDeleted,
                        DeletedAt = offer.DeletedAt,
                        DeletedByUserId = offer.DeletedByUserId
                    })
                    .ToListAsync();

                string? newCursor = paginatedOffers.Count > limit ? paginatedOffers.Last().Id.ToString() : null;
                if (newCursor != null)
                {
                    paginatedOffers = paginatedOffers.Take(limit).ToList();
                }

                var totalCount = totalCountQuery;


                return new Paginated<Domain.Offers.Offer>(paginatedOffers, newCursor ?? "", totalCount, paginatedOffers.Count == limit);
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
        public async Task<Paginated<Offer>> GetAllPendingMatchingOffers(int limit, string? cursor)
        {
            try
            {
                Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
                var query = db.Offers.IgnoreQueryFilters().Where(o => !(o.Cash > 0) && o.SourceStatus != o.TargetStatus).AsNoTracking();
                if (cursorGuid.HasValue)
                {
                    query = query.Where(item => item.Id.CompareTo(cursorGuid.Value) > 0);
                }
                var totalCountQuery = await query.CountAsync();
                var paginatedOffers = await query
                    .OrderBy(item => item.Id)
                    .Take(limit + 1)
                    .Select(offer => new Offer(
                    offer.Id,
                    offer.SourceItemId,
                    offer.TargetItemId,
                    offer.Cash,
                    offer.CreatedByUserId,
                    offer.UpdatedByUserId,
                    offer.CreatedAt.DateTime,
                    (int)offer.SourceStatus,
                    (int)offer.TargetStatus,
                    offer.IsRead)
                    {
                        IsDeleted = offer.IsDeleted,
                        DeletedAt = offer.DeletedAt,
                        DeletedByUserId = offer.DeletedByUserId
                    })
                    .ToListAsync();

                string? newCursor = paginatedOffers.Count > limit ? paginatedOffers.Last().Id.ToString() : null;
                if (newCursor != null)
                {
                    paginatedOffers = paginatedOffers.Take(limit).ToList();
                }

                var totalCount = totalCountQuery;


                return new Paginated<Domain.Offers.Offer>(paginatedOffers, newCursor ?? "", totalCount, paginatedOffers.Count == limit);
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
        public async Task<Paginated<Offer>> GetAllAcceptedCashOffers(int limit, string? cursor)
        {
            try
            {
                Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
                var query = db.Offers.IgnoreQueryFilters().Where(o => o.Cash > 0 && o.SourceStatus == o.TargetStatus).AsNoTracking();
                if (cursorGuid.HasValue)
                {
                    query = query.Where(item => item.Id.CompareTo(cursorGuid.Value) > 0);
                }
                var totalCountQuery = await query.CountAsync();
                var paginatedOffers = await query
                    .OrderBy(item => item.Id)
                    .Take(limit + 1)
                    .Select(offer => new Offer(
                    offer.Id,
                    offer.SourceItemId,
                    offer.TargetItemId,
                    offer.Cash,
                    offer.CreatedByUserId,
                    offer.UpdatedByUserId,
                    offer.CreatedAt.DateTime,
                    (int)offer.SourceStatus,
                    (int)offer.TargetStatus,
                    offer.IsRead)
                    {
                        IsDeleted = offer.IsDeleted,
                        DeletedAt = offer.DeletedAt,
                        DeletedByUserId = offer.DeletedByUserId
                    })
                    .ToListAsync();

                string? newCursor = paginatedOffers.Count > limit ? paginatedOffers.Last().Id.ToString() : null;
                if (newCursor != null)
                {
                    paginatedOffers = paginatedOffers.Take(limit).ToList();
                }

                var totalCount = totalCountQuery;


                return new Paginated<Domain.Offers.Offer>(paginatedOffers, newCursor ?? "", totalCount, paginatedOffers.Count == limit);
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
        public async Task<Paginated<Offer>> GetAllPendingCashOffers(int limit, string? cursor)
        {
            try
            {
                Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
                var query = db.Offers.IgnoreQueryFilters().Where(o => o.Cash > 0 && o.SourceStatus != o.TargetStatus).AsNoTracking();
                if (cursorGuid.HasValue)
                {
                    query = query.Where(item => item.Id.CompareTo(cursorGuid.Value) > 0);
                }
                var totalCountQuery = await query.CountAsync();
                var paginatedOffers = await query
                    .OrderBy(item => item.Id)
                    .Take(limit + 1)
                    .Select(offer => new Offer(
                    offer.Id,
                    offer.SourceItemId,
                    offer.TargetItemId,
                    offer.Cash,
                    offer.CreatedByUserId,
                    offer.UpdatedByUserId,
                    offer.CreatedAt.DateTime,
                    (int)offer.SourceStatus,
                    (int)offer.TargetStatus,
                    offer.IsRead)
                    {
                        IsDeleted = offer.IsDeleted,
                        DeletedAt = offer.DeletedAt,
                        DeletedByUserId = offer.DeletedByUserId
                    })
                    .ToListAsync();

                string? newCursor = paginatedOffers.Count > limit ? paginatedOffers.Last().Id.ToString() : null;
                if (newCursor != null)
                {
                    paginatedOffers = paginatedOffers.Take(limit).ToList();
                }

                var totalCount = totalCountQuery;


                return new Paginated<Domain.Offers.Offer>(paginatedOffers, newCursor ?? "", totalCount, paginatedOffers.Count == limit);
            }

            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
    }
}
