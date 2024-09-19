using System;
using System.Net.Http;
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
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Domain.Notifications;
namespace Infrastructure.Offers
{
    public class OfferRepository : IOfferRepository
    {
        private readonly SwitcherooContext db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemNotificationRepository _systemNotificationRepository;


        private readonly ILoggerManager _loggerManager;

        public OfferRepository(SwitcherooContext db, ILoggerManager loggerManager, IHttpContextAccessor httpContextAccessor, ISystemNotificationRepository systemNotificationRepository)
        {
            this.db = db;
            _loggerManager = loggerManager;
            _httpContextAccessor = httpContextAccessor;
            _systemNotificationRepository = systemNotificationRepository;
        }

        //public async Task<int> GetSwipesInfo(Guid userId)
        //{
        //    Console.WriteLine($"Debug: Current UTC time: {DateTime.UtcNow}");
        //    var today = DateTime.UtcNow.Date;
        //    Console.WriteLine($"Debug: Querying for date: {today:yyyy-MM-dd}");

        //    var offers = await db.Offers
        //        .Where(o => o.CreatedByUserId == userId && !o.IsDeleted)
        //        .ToListAsync();
        //    int todayCount = 0;
        //    foreach (var offer in offers)
        //    {
        //        if (offer.Cash == null || offer.Cash <= 0)
        //            continue;
        //        if (offer.CreatedAt.Date == today)
        //            todayCount += 1;
        //    }
        //    return todayCount;
        //}
        private async Task<string> GetUserTimeZone()
        {
            var userIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(userIp))
            {
                throw new Exception("Could not determine the user's IP address.");
            }

            // Fetch timezone information using the IP geolocation API
            string apiKey = "fcfdbee0835b41099b47686aa3a3e758";
            string apiUrl = $"https://api.ipgeolocation.io/timezone?apiKey={apiKey}&ip={userIp}";
            string userTimeZoneId = "";
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync(apiUrl);
                var jsonResponse = JObject.Parse(response);

                userTimeZoneId = jsonResponse["timezone"]?.ToString();
                if (string.IsNullOrEmpty(userTimeZoneId))
                {
                    throw new Exception("Could not determine the user's time zone.");
                }
            }
            return userTimeZoneId;
        }
        public async Task<Tuple<int, int>> GetTodayAndYesturdaySwipesInfo(Guid userId)
        {
            // Get the current date in the user's timezone
            var userTimeZoneId = "Eastern Standard Time";
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId);
            var todayEastern = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone).Date;
            var yesterdayEastern = todayEastern.AddDays(-1);

            Console.WriteLine($"Debug: Current date in user timezone ({userTimeZoneId}): {todayEastern:yyyy-MM-dd}");

            // Query offers from the database
            var offers = await db.Offers
                .Where(o => o.CreatedByUserId == userId && !o.IsDeleted)
                .Where(o => o.Cash == null || o.Cash <= 0)
                .ToListAsync();

            int todayCount = 0;
            int yesturdayCount = 0;
            foreach (var offer in offers)
            {
                if (!(offer.Cash == null || offer.Cash <= 0))
                    continue;

                // Convert offer's CreatedAt to the user's timezone
                var offerCreatedInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(offer.CreatedAt.DateTime, easternZone).Date;
                if (offerCreatedInUserTimeZone == todayEastern)
                    todayCount += 1;
                if (offerCreatedInUserTimeZone == yesterdayEastern)
                    yesturdayCount += 1;
            }

            return Tuple.Create(todayCount, yesturdayCount);

        }
        public async Task<int> GetSwipesInfo(Guid userId)
        {
            Tuple<int, int> info = await GetTodayAndYesturdaySwipesInfo(userId);
            return info.Item1;
        }
        public async Task<int> GetYesturdaySwipesInfo(Guid userId)
        {
            Tuple<int, int> info = await GetTodayAndYesturdaySwipesInfo(userId);
            return info.Item2;
        }
        public async Task<Offer>? CreateOffer(Offer offer)
        {
            try
            {
                var now = DateTime.UtcNow;
                Offer? myoffer = null;

                // Find out the total number of swipes today and restrict if quota reached
                int swipes_used = await GetSwipesInfo(offer.CreatedByUserId.Value);
                if (swipes_used == 10)
                    throw new InfrastructureException("You have used all your swipes for today. Come back tomorrow and try again.");

                var targetUserId = db.Items.Where(x => x.Id.Equals(offer.TargetItemId))
                           .Select(x => x.CreatedByUserId).FirstOrDefault();

                var userIds = new[] { offer.CreatedByUserId, targetUserId };

                var sourceUserFCMToken = await db.Users.Where(u => u.Id == offer.CreatedByUserId).Select(u => u.FCMToken).FirstOrDefaultAsync();
                //var targetUserFCMToken = await db.Users.Where(u => u.Id == targetUserId).Select(u => u.FCMToken).FirstOrDefaultAsync();

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
                            var targetItemForCashOffer = await db.Items.Where(i => i.Id == offer.TargetItemId).FirstOrDefaultAsync();

                            await db.SaveChangesAsync();
                            myoffer = await GetOfferById(newDbOffer.CreatedByUserId, newDbOffer.Id);
                            string newCashOfferNotificationData = JsonSerializer.Serialize(myoffer);
                            var notificationData = new Dictionary<string, string>
                                    {
                                        {"TargetItemId", offer.TargetItemId.ToString()}
                                    };
                            var newCashOfferNotification = SystemNotification.NewCashOfferNotification(targetItemForCashOffer.Title, offer.Cash, targetUserId, newCashOfferNotificationData);
                            await _systemNotificationRepository.CreateAsync(newCashOfferNotification, true, notificationData);
                            return myoffer;
                        }
                        else
                        {
                            match.TargetStatus = Database.Schema.OfferStatus.Initiated;
                            await db.SaveChangesAsync();
                            myoffer = await GetOfferById(match.CreatedByUserId, match.Id);
                            var data = JsonSerializer.Serialize(myoffer);
                            var matchingOfferNotificationData = new Dictionary<string, string>
                            {
                                {"IsMatch", "true"},
                                {"SourceItemId", match.SourceItemId.ToString()},
                                {"SourceItemImage", match.SourceItem.MainImageUrl},
                                {"TargetItemId", match.TargetItemId.ToString()},
                                {"TargetItemImage", match.TargetItem.MainImageUrl}
                            };
                            var matchingOfferNotificationForTargetUser = SystemNotification.ItemMatchedNotification(targetUserId, data);
                            await _systemNotificationRepository.CreateAsync(matchingOfferNotificationForTargetUser, true, matchingOfferNotificationData);
                            var sourceUserId = await db.Items.Where(i => i.Id == myoffer.SourceItemId).Select(i => i.CreatedByUserId).FirstOrDefaultAsync();
                            var matchingOfferNotificationForSourceUser = SystemNotification.ItemMatchedNotification(sourceUserId, data);
                            await _systemNotificationRepository.CreateAsync(matchingOfferNotificationForSourceUser, true, matchingOfferNotificationData);
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
                                await db.SaveChangesAsync();
                                myoffer = await GetOfferById(newDbOffer.CreatedByUserId, newDbOffer.Id);
                                string newCashOfferNotificationData = JsonSerializer.Serialize(myoffer);
                                var notificationData = new Dictionary<string, string>
                                    {
                                        {"TargetItemId", offer.TargetItemId.ToString()}
                                    };
                                var targetItemForCashOffer = await db.Items.Where(i => i.Id == offer.TargetItemId).FirstOrDefaultAsync();
                                var newCashOfferNotification = SystemNotification.NewCashOfferNotification(targetItemForCashOffer.Title, offer.Cash, targetUserId, newCashOfferNotificationData);
                                await _systemNotificationRepository.CreateAsync(newCashOfferNotification, true, notificationData);
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
                if (offer.ConfirmedBySourceUser == true && offer.ConfirmedByTargetUser == true) throw new InfrastructureException("Cannot delete offer because it is already confirmed!");

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
                var offer = db.Offers.Include(o => o.TargetItem).ThenInclude(i => i.CreatedByUser).Where(u => u.Id == offerId).FirstOrDefault();
                offer.TargetStatus = Database.Schema.OfferStatus.Initiated;
                string data = JsonSerializer.Serialize(offer);
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
                var notification = SystemNotification.OfferAcceptedNotification(offer.TargetItem.Title, offer.Cash, offer.TargetItem.CreatedByUser.LastName, offer.CreatedByUserId, data);
                var notificationData = new Dictionary<string, string>
                    {
                        {"TargetItemId", offer.TargetItemId.ToString()},
                        {"ChatListingStrigifiedObject", newDummyMessageJson}
                    };
                await _systemNotificationRepository.CreateAsync(notification, true, notificationData);
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
                if (offer.ConfirmedBySourceUser == true && offer.ConfirmedByTargetUser == true) throw new InfrastructureException("Cannot Un-Match offer because it is already confirmed!");
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser))
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser))
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

            return new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.Date, (int)offer.SourceStatus, (int)offer.TargetStatus, offer.IsRead,
                offer.ConfirmedBySourceUser,
                offer.ConfirmedByTargetUser);
        }
        public async Task<Offer> GetOfferById(Guid offerId)
        {
            /* var myItems = await db.Items
                 .Where(z => z.CreatedByUserId == userId)
                 .Select(z => z.Id)
                 .ToArrayAsync();
 */
            var offer = await db.Offers
                .Where(o => o.Id.Equals(offerId))
                .SingleOrDefaultAsync(z => z.Id == offerId);
            /*!myItems.Contains(offer.SourceItemId) || !myItems.Contains(offer.TargetItemId)*/
            if (offer == null)
            {
                throw new SecurityException("You cant access this offer");
            }

            return new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.Date, (int)offer.SourceStatus, (int)offer.TargetStatus, offer.IsRead,
                offer.ConfirmedBySourceUser,
                offer.ConfirmedByTargetUser);
        }

        public async Task<Offer> GetOfferByOfferId(Guid offerId)
        {
            var offer = await db.Offers
                .Where(o => o.Id.Equals(offerId))
                .SingleOrDefaultAsync(z => z.Id == offerId);
            return new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.Date, (int)offer.SourceStatus, (int)offer.TargetStatus, offer.IsRead,
                offer.ConfirmedBySourceUser,
                offer.ConfirmedByTargetUser);
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

            return new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.Date, (int)offer.SourceStatus, (int)offer.TargetStatus, offer.IsRead, offer.ConfirmedBySourceUser, offer.ConfirmedByTargetUser);
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser))
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser))
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
                var query = db.Offers.IgnoreQueryFilters().Where(o => (o.Cash <= 0 || o.Cash == null) && o.SourceStatus == o.TargetStatus).AsNoTracking();
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser)
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

        public async Task<Paginated<Offer>> GetAllConfirmedOffers(int limit, string? cursor)
        {
            try
            {
                Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
                var query = db.Offers.IgnoreQueryFilters().Where(o => (o.Cash <= 0 || o.Cash == null) && o.SourceStatus == o.TargetStatus && o.ConfirmedBySourceUser == true && o.ConfirmedByTargetUser == true).AsNoTracking();
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser)
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
        public async Task<Paginated<Offer>> GetAllOffersConfirmedByOneParty(int limit, string? cursor)
        {
            try
            {
                Guid? cursorGuid = cursor != null ? Guid.Parse(cursor) : (Guid?)null;
                var query = db.Offers.IgnoreQueryFilters().Where(o => (o.Cash <= 0 || o.Cash == null) &&
                o.SourceStatus == o.TargetStatus &&
                (o.ConfirmedBySourceUser == true || o.ConfirmedByTargetUser == true))
                    .AsNoTracking();
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser)
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser)
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser)
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
                    offer.IsRead,
                    offer.ConfirmedBySourceUser,
                    offer.ConfirmedByTargetUser)
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

        public async Task<List<Offer>> GetMatchedOffers()
        {
            var offers = await db.Offers
                .Where(o => o.SourceStatus == Infrastructure.Database.Schema.OfferStatus.Initiated && o.TargetStatus == Infrastructure.Database.Schema.OfferStatus.Initiated)
            .Select(offer => new Offer(offer.Id, offer.SourceItemId, offer.TargetItemId, offer.Cash, offer.CreatedByUserId, offer.UpdatedByUserId, offer.CreatedAt.DateTime.Date, (int)offer.SourceStatus, (int)offer.TargetStatus, offer.IsRead,
            offer.ConfirmedBySourceUser,
            offer.ConfirmedByTargetUser))
            .ToListAsync();
            return offers;
        }
        public async Task<Offer> ConfirmOffer(Guid offerId, Guid userId)
        {
            var offer = await db.Offers
                .Include(o => o.SourceItem)
                .ThenInclude(i => i.CreatedByUser)
                .Include(o => o.TargetItem)
                .ThenInclude(i => i.CreatedByUser)
                .Where(o => o.Id == offerId)
                .FirstOrDefaultAsync();
            if (offer == null) throw new InfrastructureException("Invalid Offer Id");
            else if (offer.CreatedByUserId != userId && offer.TargetItem.CreatedByUserId != userId) throw new InfrastructureException("Oops, how can you confirm when you're not even involved with this offer?");
            else if (offer.SourceStatus != offer.TargetStatus) throw new InfrastructureException("Oops, how can you confirm when the offer hasn't even been matched yet?");

            if (offer.ConfirmedBySourceUser == true && offer.ConfirmedByTargetUser == true) throw new InfrastructureException("Cannot unconfirm offer once it is confirmed!");


            string data = JsonSerializer.Serialize(offer);
            if (offer.CreatedByUserId == userId && offer.ConfirmedBySourceUser != true)
            {
                offer.ConfirmedBySourceUser = true;
                var notification = SystemNotification.OfferConfirmationNotification(offer.SourceItem.Title,
                    offer.TargetItem.Title,
                    offer.Cash > 0,
                    offer.Cash,
                    offer.SourceItem.CreatedByUser.LastName,
                    offer.ConfirmedByTargetUser == true,
                    offer.TargetItem.CreatedByUser.Id,
                    data);
                await _systemNotificationRepository.CreateAsync(notification);
                if (offer.ConfirmedByTargetUser == true)
                {
                    notification.UserId = offer.TargetItem.CreatedByUserId;
                    await _systemNotificationRepository.CreateAsync(notification);
                }

            }
            //else if (offer.CreatedByUserId == userId && offer.ConfirmedBySourceUser == true)
            //{
            //    offer.ConfirmedBySourceUser = false;
            //    var notification = SystemNotification.OfferConfirmationCancellationNotification(offer.SourceItem.Title,
            //        offer.TargetItem.Title,
            //        offer.Cash > 0,
            //        offer.Cash,
            //        offer.SourceItem.CreatedByUser.LastName,
            //        offer.TargetItem.CreatedByUser.Id,
            //        data);
            //    await _systemNotificationRepository.CreateAsync(notification);
            //}
            else if (offer.ConfirmedByTargetUser != true)
            {
                offer.ConfirmedByTargetUser = true;
                var notification = SystemNotification.OfferConfirmationNotification(offer.TargetItem.Title,
                    offer.SourceItem.Title,
                    offer.Cash > 0,
                    offer.Cash,
                    offer.TargetItem.CreatedByUser.LastName,
                    offer.ConfirmedBySourceUser == true,
                    offer.SourceItem.CreatedByUser.Id,
                    data);
                await _systemNotificationRepository.CreateAsync(notification);
                if (offer.ConfirmedBySourceUser == true)
                {
                    notification.UserId = offer.SourceItem.CreatedByUserId;
                    await _systemNotificationRepository.CreateAsync(notification);
                }
            }
            //else if (offer.ConfirmedByTargetUser == true)
            //{
            //    offer.ConfirmedByTargetUser = false;
            //    var notification = SystemNotification.OfferConfirmationCancellationNotification(offer.TargetItem.Title,
            //        offer.SourceItem.Title,
            //        offer.Cash > 0,
            //        offer.Cash,
            //        offer.TargetItem.CreatedByUser.LastName,
            //        offer.SourceItem.CreatedByUser.Id,
            //        data);
            //    await _systemNotificationRepository.CreateAsync(notification);

            //}
            await db.SaveChangesAsync();
            return await GetOfferById(offerId);

        }
    }
}
