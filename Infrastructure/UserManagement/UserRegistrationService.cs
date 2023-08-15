using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UserManagement
{
    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly UserManager<Database.Schema.User> userManager;
        private readonly SwitcherooContext db;

        public UserRegistrationService(SwitcherooContext db, UserManager<Database.Schema.User> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        public async Task<Guid> CreateUserAsync(User user, string password)
        {
            var now = DateTime.UtcNow;

            var newUser = new Database.Schema.User(
                user.Username,
                user.FirstName,
                user.LastName,
                user.Mobile,
                user.Gender,
                user.DateOfBirth,
                user.Distance,
                user.Blurb,
                user.AvatarUrl,
                user.Email,
                user.IsMatchNotificationsEnabled,
                user.IsChatNotificationsEnabled)
            {
                CreatedAt = now,
                UpdatedAt = now
            };

            var retVal = await userManager.CreateAsync(newUser, password);

            if (retVal.Succeeded)
            {
                await userManager.GenerateEmailConfirmationTokenAsync(newUser);
                return newUser.Id;
            }

            throw new InfrastructureException(retVal.Errors.Select(z => z.Description).ToArray());
        }

        public async Task<string> GenerateConfirmationCodeAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var anyExistingForUser = await db.UserVerificationCodes
                .Where(x => x.Email == user.Email)
                .ToListAsync();

            db.UserVerificationCodes.RemoveRange(anyExistingForUser);
            await db.SaveChangesAsync();

            var newCode = User.GenerateSixDigitVerificationCode();
            var newVerificationCode = new Database.Schema.UserVerificationCode(user.Email, newCode, token)
            {
                CreatedByUserId = user.Id,
                UpdatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var retVal = await db.UserVerificationCodes.AddAsync(newVerificationCode);
            await db.SaveChangesAsync();

            return retVal.Entity.SixDigitCode;
        }

        public async Task<string> GeneratePasswordResetConfirmationCodeAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var anyExistingForUser = await db.UserVerificationCodes
                .Where(x => x.Email == user.Email)
                .ToListAsync();

            db.UserVerificationCodes.RemoveRange(anyExistingForUser);
            await db.SaveChangesAsync();

            var newCode = User.GenerateSixDigitVerificationCode();
            var newVerificationCode = new Database.Schema.UserVerificationCode(user.Email, newCode, token)
            {
                CreatedByUserId = user.Id,
                UpdatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var retVal = await db.UserVerificationCodes.AddAsync(newVerificationCode);
            await db.SaveChangesAsync();

            return retVal.Entity.SixDigitCode;
        }

        public async Task<string?> RetrieveResetPasswordTokenAsync(string email, string verificationCode)
        {
            var retVal = await db.UserVerificationCodes
                .Where(x => x.Email == email && x.SixDigitCode == verificationCode)
                .SingleOrDefaultAsync();

            if (retVal == null) return null;
            
            // Delete entry in DB
            db.UserVerificationCodes.Remove(retVal);
            await db.SaveChangesAsync();

            return retVal.EmailConfirmationToken;
        }

        public async Task<bool> VerifyUserAsync(string email, string verificationCode)
        {
            var retVal = await db.UserVerificationCodes
                .Where(x => x.Email == email && x.SixDigitCode == verificationCode)
                .SingleOrDefaultAsync();

            if (retVal == null) return false;
            
            // Delete entry in DB
            db.UserVerificationCodes.Remove(retVal);
            await db.SaveChangesAsync();

            var user = await userManager.FindByEmailAsync(email);

            if (user == null) return false;

            var confirmEmailResult = await userManager.ConfirmEmailAsync(user, retVal.EmailConfirmationToken);

            return confirmEmailResult.Succeeded;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword, string token)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return false;
            }

            var result = await userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                throw new InfrastructureException(result.Errors.Select(z => z.Description).ToArray());
            }

            return result.Succeeded;
        }
    }
}
