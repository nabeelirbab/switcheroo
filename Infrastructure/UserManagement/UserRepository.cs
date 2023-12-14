using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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
        
        public async Task<List<User>> GetUserByUserId(Guid userId)
        {
            
            var users = await db.Users
                .AsNoTracking()
                .Where(user => user.Id == userId)
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

        public async Task<bool> DeleteUser(Guid id)
        {
            try
            {
                var user = await db.Users
                    .Where(u => u.Id == id)
                    .SingleOrDefaultAsync();
                if (user == null)
                {
                    throw new InfrastructureException("User not found.");
                }
                var items = await db.Items.Where(u => u.CreatedByUserId.Equals(id)).ToListAsync();

                var itemIds = items.Select(item => item.Id);

                var offers = await db.Offers
                    .Where(u => u.CreatedByUserId.Equals(id) || itemIds.Contains(u.TargetItemId))
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
                var itemsToDelete = await db.Items.Where(u => u.CreatedByUserId.Equals(id)).ToListAsync();
                if (itemsToDelete == null) { _logger.LogInformation($"No item Found"); }
                else { db.Items.RemoveRange(itemsToDelete); }

                // delete user
                db.Users.Remove(user);
                await db.SaveChangesAsync();

                var checkUser = await db.Users
                    .AsNoTracking()
                    .Where(user => user.Id == id)
                    .Select(Database.Schema.User.ToDomain)
                    .SingleOrDefaultAsync();

                if (checkUser == null)
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
    }
}
