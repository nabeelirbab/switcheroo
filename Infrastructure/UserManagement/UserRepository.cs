using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Users;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.UserManagement
{
    public class UserRepository : IUserRepository
    {
        private readonly SwitcherooContext db;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(SwitcherooContext db, ILogger<UserRepository> logger)
        {
            this.db = db;
            _logger = logger;
            _logger.LogDebug("Nlog is integrated to User repository");
        }

        public async Task<User> GetByEmail(string email)
        {
            var user = await db.Users
                .AsNoTracking()
                .Where(user => user.Email == email)
                .Select(Database.Schema.User.ToDomain)
                .SingleOrDefaultAsync();

            if (user == null) throw new InfrastructureException($"Couldn't find user with email {email}");

            return user;
        }

        public async Task<Paginated<User>> GetAllUsers(int limit, string? cursor)
        {
            IEnumerable<Guid> requiredIds;

            var Users = await db.Users.ToListAsync();

            var usersIdsSorted = Users.Select(x => x.Id).ToList();

            if (cursor != null)
            {
                requiredIds = usersIdsSorted
                .SkipWhile(x => cursor != "" && x.ToString() != cursor)
                .Skip(1)
                .Take(limit);
            }
            else
            {
                requiredIds = usersIdsSorted.Take(limit);
            }

            var totalCount = usersIdsSorted.Count();

            var data = await db.Users
                .AsNoTracking()
                .Where(x => requiredIds.Contains(x.Id))
                .OrderByDescending(x => x.CreatedAt)
                .Select(Database.Schema.User.ToDomain)
                .ToListAsync();

            //Item count
            foreach (var user in data)
            {
                var userItems = await db.Items
                    .Where(i => i.CreatedByUserId == user.Id)
                    .ToListAsync();
                user.ItemCount = userItems.Count;
            }

            //Matched items
            foreach (var user in data)
            {
                var userItems = await db.Items
                    .Where(i => i.CreatedByUserId == user.Id)
                    .ToListAsync();

                if (userItems.Count != 0)
                {
                    var sourceItemIds = userItems.Select(item => item.Id).ToList();

                    var offers = await db.Offers
                        .Where(offer =>
                            sourceItemIds.Contains(offer.SourceItemId) || sourceItemIds.Contains(offer.TargetItemId))
                        .Where(offer =>
                            offer.SourceStatus == Database.Schema.OfferStatus.Initiated &&
                            offer.TargetStatus == Database.Schema.OfferStatus.Initiated)
                        .ToListAsync();

                    user.MatchedItemCount = offers.Count;
                }
                else
                {
                    user.MatchedItemCount = 0;
                }
            }


            var newCursor = data.Count > 0 ? data.Last().Id.ToString() : "";

            return new Paginated<User>(data, newCursor ?? "", totalCount, data.Count == limit);
        }

        public async Task<User> GetById(Guid? id)
        {
            var user = await db.Users
                .AsNoTracking()
                .Where(user => user.Id == id)
                .Select(Database.Schema.User.ToDomain)
                .SingleOrDefaultAsync();

            if (user == null) throw new InfrastructureException($"Couldn't find user {id}");

            return user;
        }

        public async Task<List<User>> GetUserByUserId(List<Guid> userIds)
        {

            var users = await db.Users
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .Select(Database.Schema.User.ToDomain)
                .ToListAsync();

            return users;
        }
        public async Task<string> GetTargetUserForMessage(Guid? userId, Guid offerId, bool getFCMToken = false)
        {
            var query = db.Offers
                .Where(o => o.Id == offerId)
                .Select(o => new
                {
                    SourceUser = o.CreatedByUserId,
                    SourceUserFcm = getFCMToken ? o.CreatedByUser.FCMToken : null,
                    TargetUser = o.TargetItem.CreatedByUserId,
                    TargetUserFcm = getFCMToken ? o.TargetItem.CreatedByUser.FCMToken : null
                });

            var offerUsers = await query.FirstOrDefaultAsync();

            if (offerUsers == null)
            {
                return null; // Or handle as appropriate
            }

            if (userId == offerUsers.SourceUser)
            {
                return getFCMToken ? offerUsers.TargetUserFcm : offerUsers.TargetUser.ToString();
            }
            else
            {
                return getFCMToken ? offerUsers.SourceUserFcm : offerUsers.SourceUser.ToString();
            }
        }


        public async Task<List<User>> GetTargetUser(Guid? userId, Guid offerId)
        {
            try
            {
                var itemIds = db.Offers.Where(o => o.Id.Equals(offerId)).Select(o => new
                {
                    SourceItemId = o.SourceItemId,
                    TargetItemId = o.TargetItemId,
                    OfferCreatedBy = o.CreatedByUserId,
                    Cash = o.Cash,
                }).FirstOrDefault();


                var sourceItemId = itemIds.SourceItemId;
                var targetItemId = itemIds.TargetItemId;
                var Cash = itemIds.Cash != null && itemIds.Cash > 0 ? true : false;
                List<Domain.Items.Item> items;
                if (Cash)
                {
                    items = await db.Items
                    .Where(item => (item.Id == sourceItemId || item.Id == targetItemId))
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();
                }
                else
                {

                    items = await db.Items
                        .Where(item => (item.Id == sourceItemId || item.Id == targetItemId) && item.CreatedByUserId != userId)
                        .Select(Database.Schema.Item.ToDomain)
                        .ToListAsync();
                }

                var creatorIds = items.Select(item => item.CreatedByUserId).Distinct().ToList();

                var users = await db.Users
                    .AsNoTracking()
                    .Where(user => creatorIds.Contains(user.Id))
                    .Select(Database.Schema.User.ToDomain)
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<List<User>> GetUserById(Guid? userId)
        {
            try
            {
                var users = await db.Users
                    .AsNoTracking()
                    .Where(user => user.Id == userId)
                    .Select(Database.Schema.User.ToDomain)
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<List<KeyValue>> GetUsersGenderCount()
        {
            try
            {
                var keyValueList = await db.Users
                                      .Where(user => user.Gender != null)
                                      .GroupBy(user => user.Gender)
                                      .Select(group => new KeyValue(group.Key, group.Count()))
                                      .ToListAsync();

                return keyValueList;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception {ex.Message}");
            }
        }

        public async Task<User> UpdateUserDateOfBirth(Guid id, DateTime? dateOfBirth)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.DateOfBirth = dateOfBirth;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserDistance(Guid id, int? distance)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Distance = distance;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserLocation(Guid id, decimal? latitude, decimal? longitude)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Latitude = latitude;
            dbUser.Longitude = longitude;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserFCMToken(Guid id, string? fcmToken)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            if (string.IsNullOrEmpty(fcmToken))
            {
                throw new InfrastructureException($"token not found in payload");
            }
            else
            {
                dbUser.FCMToken = fcmToken;
                db.Users.Update(dbUser);
                await db.SaveChangesAsync();
            }

            return await GetById(id);
        }


        public async Task<User> UpdateUserEmail(Guid id, string email)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");
            if (await db.Users.AnyAsync(z => z.Email == email)) throw new InfrastructureException($"Email already in use {email}");

            dbUser.Email = email;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserGender(Guid id, string? gender)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Gender = gender;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserMobile(Guid id, string? mobile)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Mobile = mobile;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserName(Guid id, string firstName, string lastName)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.FirstName = firstName;
            dbUser.LastName = lastName;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<User> UpdateUserProfileDetails(Guid id, string? blurb, string? avatarUrl)
        {
            var dbUser = await db.Users.SingleOrDefaultAsync(x => x.Id == id);

            if (dbUser == null) throw new InfrastructureException($"Couldn't find user with id {id}");

            dbUser.Blurb = blurb;
            dbUser.AvatarUrl = avatarUrl;
            dbUser.UpdatedAt = DateTime.UtcNow;

            db.Users.Update(dbUser);
            await db.SaveChangesAsync();

            return await GetById(id);
        }

        public async Task<bool> DeleteUser(List<Guid> userIds, Guid deletedByUserId)
        {
            if (userIds == null || !userIds.Any())
            {
                throw new ArgumentNullException(nameof(userIds), "User IDs cannot be null or empty.");
            }

            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // Soft delete Users
                var usersToUpdate = await db.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                if (!usersToUpdate.Any())
                {
                    throw new KeyNotFoundException("No users found with the provided IDs.");
                }
                usersToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTime.Now; });

                // Soft delete Items
                var itemsToUpdate = await db.Items
                    .Where(item => userIds.Contains(item.CreatedByUserId))
                    .ToListAsync();
                itemsToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTime.Now; });

                // Get item IDs for later queries
                var itemIds = itemsToUpdate.Select(item => item.Id).ToList();

                // Soft delete Offers
                var offersToUpdate = await db.Offers
                    .Where(offer => userIds.Contains(offer.CreatedByUserId) || itemIds.Contains(offer.TargetItemId))
                    .ToListAsync();
                offersToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTime.Now; });

                // Soft delete Messages linked to the Offers
                var offerIds = offersToUpdate.Select(offer => offer.Id).ToList();
                var messagesToUpdate = await db.Messages
                    .Where(message => offerIds.Contains(message.OfferId))
                    .ToListAsync();
                messagesToUpdate.ForEach(u => { u.IsDeleted = true; u.DeletedByUserId = deletedByUserId; u.DeletedAt = DateTime.Now; });

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting users with IDs {string.Join(", ", userIds)}");
                await transaction.RollbackAsync();
                throw; // Rethrow to let the caller handle it or to fail visibly if unhandled
            }
        }


        public async Task<bool> NotifyMe(Guid? id, bool NewMatchingNotification, bool NewCashOfferNotification, bool CashOfferAcceptedNotification)
        {
            try
            {
                if (id == null) return false;

                var userFCMToken = db.Users
                .Where(x => x.Id == id)
                .Select(x => x.FCMToken).FirstOrDefault();
                if (string.IsNullOrEmpty(userFCMToken)) throw new InfrastructureException($"No FCM Token exists for this user");

                var app = FirebaseApp.DefaultInstance;
                var messaging = FirebaseMessaging.GetMessaging(app);
                string response = "";
                if (NewMatchingNotification)
                {
                    var offer = db.Offers.Include(o => o.SourceItem).Include(o => o.TargetItem).Where(x => (x.CreatedByUserId == id || x.TargetItem.CreatedByUserId == id) && x.SourceItemId != x.TargetItemId).FirstOrDefault();
                    if (offer == null) throw new InfrastructureException($"No Matching Offer is created by this user.....");
                    var message = new Message()
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
                                {"SourceItemId", offer.SourceItemId.ToString()},
                                {"SourceItemImage", offer.SourceItem.MainImageUrl},
                                {"TargetItemId", offer.TargetItemId.ToString()},
                                {"TargetItemImage", offer.TargetItem.MainImageUrl}
                            }
                    };

                    response = await messaging.SendAsync(message);
                }
                else if (NewCashOfferNotification)
                {
                    var offer = db.Offers.Include(o => o.SourceItem).Include(o => o.TargetItem).Where(x => x.TargetItem.CreatedByUserId == id && x.Cash > 0).FirstOrDefault();
                    if (offer == null) throw new InfrastructureException($"No Cash Offer received by this user.....");
                    var message = new FirebaseAdmin.Messaging.Message()
                    {
                        Token = userFCMToken,
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

                    response = await messaging.SendAsync(message);
                }
                else if (CashOfferAcceptedNotification)
                {
                    var offer = db.Offers.Include(o => o.SourceItem).Include(o => o.TargetItem).Where(x => x.CreatedByUserId == id && x.Cash > 0 && x.SourceStatus == x.TargetStatus).FirstOrDefault();
                    if (offer == null) throw new InfrastructureException($"No Cash Offer received by this user.....");
                    var newDummyMessage = new Domain.Offers.Message(
                         Guid.NewGuid(),
                             offer.CreatedByUserId,
                             offer.Id,
                             offer.Cash,
                             offer.CreatedByUserId,
                             "",
                             null,
                             offer.CreatedAt,
                             false
                         );
                    string newDummyMessageJson = JsonSerializer.Serialize(newDummyMessage);
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

                    response = await messaging.SendAsync(message);
                }
                if (response != null) return true;
                return false;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"No request sent to you {ex.Message}");
            }
        }
        public async Task<bool> CheckIfUserByEmail(string email)
        {
            return await db.Users
                           .AsNoTracking()
                           .AnyAsync(user => user.Email == email);
        }
    }
}
