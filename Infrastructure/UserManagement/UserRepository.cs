using System;
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

        public async Task<User> GetById(Guid id)
        {
            var user = await db.Users
                .AsNoTracking()
                .Where(user => user.Id == id)
                .Select(Database.Schema.User.ToDomain)
                .SingleOrDefaultAsync();

            if (user == null) throw new InfrastructureException($"Couldn't find user {id}");

            return user;
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
                    _logger.LogWarning($"DeleteUser: User with ID {id} not found.");
                    return false;
                }

                _logger.LogInformation($"DeleteUser: Deleting user {user.Id}");
                db.Users.Remove(user);
                await db.SaveChangesAsync();

                var checkUser = await db.Users
                    .AsNoTracking()
                    .Where(user => user.Id == id)
                    .Select(Database.Schema.User.ToDomain)
                    .SingleOrDefaultAsync();

                if (checkUser == null)
                {
                    _logger.LogInformation($"DeleteUser: User {user.Id} deleted successfully.");
                }
                else
                {
                    _logger.LogError($"DeleteUser: User {user.Id} deletion failed.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteUser: An error occurred while deleting the user {ex.Message}");
                return false;
            }
        }
    }
}
