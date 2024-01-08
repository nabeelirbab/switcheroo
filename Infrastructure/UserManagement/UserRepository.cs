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

        public async Task<List<User>> GetTargetUser(Guid? userId, Guid offerId)
        {
            try
            {
                var itemIds = db.Offers.Where(o => o.Id.Equals(offerId)).Select(o => new
                {
                    SourceItemId = o.SourceItemId,
                    TargetItemId = o.TargetItemId
                }).FirstOrDefault();

                var sourceItemId = itemIds.SourceItemId;
                var targetItemId = itemIds.TargetItemId;

                var items = await db.Items
                    .Where(item => (item.Id == sourceItemId || item.Id == targetItemId) && item.CreatedByUserId != userId)
                    .Select(Database.Schema.Item.ToDomain)
                    .ToListAsync();

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
                    .Where(user => user.Id==userId)
                    .Select(Database.Schema.User.ToDomain)
                    .ToListAsync();

                return users;
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

        public async Task<bool> DeleteUser(List<Guid> ids)
        {
            try
            {
                var users = await db.Users
                    .Where(u => ids.Contains(u.Id))
                    .ToListAsync();
                if (users.Count == 0)
                {
                    throw new InfrastructureException("User not found.");
                }
                var items = await db.Items.Where(u => ids.Contains(u.CreatedByUserId)).ToListAsync();

                var itemIds = items.Select(item => item.Id);

                // get offers either created by these users or target to this user
                var offers = await db.Offers
                    .Where(u => ids.Contains(u.CreatedByUserId) || itemIds.Contains(u.TargetItemId))
                    .ToListAsync();

                // delete messages
                var offerIds = offers.Select(offer => offer.Id).ToList();
                var messageOfferIds = await db.Messages
                        .Where(message => offerIds.Contains(message.OfferId))
                        .ToListAsync();
                if (messageOfferIds == null) { _logger.LogInformation($"No message Found"); }
                else { db.Messages.RemoveRange(messageOfferIds); }

                // delete offers
                if (offers == null) { _logger.LogInformation($"No offer Found"); }
                else { db.Offers.RemoveRange(offers); }

                // delete items
                var itemsToDelete = await db.Items.Where(u => ids.Contains(u.CreatedByUserId)).ToListAsync();
                if (itemsToDelete == null) { _logger.LogInformation($"No item Found"); }
                else { db.Items.RemoveRange(itemsToDelete); }

                // delete user
                db.Users.RemoveRange(users);
                await db.SaveChangesAsync();

                var checkUsers = await db.Users
                    .AsNoTracking()
                    .Where(user => ids.Contains(user.Id))
                    .Select(Database.Schema.User.ToDomain)
                    .ToListAsync();

                if (checkUsers.Count == 0)
                {
                    return true;
                }
                else
                {
                    throw new InfrastructureException("User not deleted.");
                }
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"DeleteUser: An error occurred while deleting the user {ex.Message}");
            }
        }

        public async Task<bool> NotifyMe(Guid? id)
        {
            try
            {
                if (id == null)
                {
                    return false;
                }
                else
                {
                    var userFCMToken = "c3H8xXLpSw-k1AgUdS2uqk:APA91bEP_NuFk4xXw7BwLW-I43ymlFyPRBWmlU5qJbQIQGFhTWeanVU7YzrdtdGmfWTlxW1BoFyUlveBo45nBR1aEGAl6fhdDnPEqnpPZRLZJAoa2UlIbEWNcAOcWGTnCN6ENzd-Vs35";
                      /*  db.Users
                    .Where(x => x.Id == id)
                    .Select(x => x.FCMToken).FirstOrDefault();*/

                    if (!string.IsNullOrEmpty(userFCMToken))
                    {
                        var app = FirebaseApp.DefaultInstance;
                        var messaging = FirebaseMessaging.GetMessaging(app);

                        var message = new Message()
                        {
                            Token = userFCMToken,
                            Notification = new Notification
                            {
                                Title = "Notify Me",
                                Body = "You Are Notified By Your Self"
                                // Other notification parameters can be added here
                            }
                        };
                        string response = await messaging.SendAsync(message);
                        if (response != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        throw new InfrastructureException($"No FCM Token exists for this user");
                    }
                }

            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"No request sent to you {ex.Message}");
            }
        }
    }
}
