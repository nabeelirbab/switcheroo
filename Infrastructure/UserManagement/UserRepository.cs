using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.UserManagement
{
    public class UserRepository : IUserRepository
    {
        private readonly SwitcherooContext db;
        private readonly ILogger<UserRepository> logger;

        public UserRepository(SwitcherooContext db, ILogger<UserRepository> logger)
        {
            this.db = db;
            this.logger = logger;
            logger.LogDebug("Nlog is integrated to User repository");
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
                Console.WriteLine($"DeleteUser {id}");
                var user = await db.Users
                .Where(u => u.Id == id)
                .SingleOrDefaultAsync();
                if (user == null)
                {
                    throw new InfrastructureException($"Couldn't find user {id}");
                }
                Console.WriteLine($"find user  {user.Id}");
                db.Users.Remove(user);
                await db.SaveChangesAsync();

                var checkUser = await db.Users
                    .AsNoTracking()
                    .Where(user => user.Id == id)
                    .Select(Database.Schema.User.ToDomain)
                    .SingleOrDefaultAsync();
                if (checkUser == null)
                {
                    Console.Write($"user deleted {checkUser}");
                    logger.LogDebug($"success logger {user}");

                }
                else
                {
                    logger.LogError($"user not deleted: {checkUser} ");
                }
                return true;
            }
            catch(Exception ex)
            {
                logger.LogError($"Error logger= {ex.Message} ");
                return false;
            }
        }
    }
}
