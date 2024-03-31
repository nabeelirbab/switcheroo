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
        private readonly RoleManager<IdentityRole<Guid>> roleManager;
        private readonly SwitcherooContext db;

        public UserRegistrationService(SwitcherooContext db, UserManager<Database.Schema.User> userManager, RoleManager<IdentityRole<Guid>> _roleManager)
        {
            this.db = db;
            this.userManager = userManager;
            roleManager = _roleManager;
        }

        public async Task<Guid> CreateUserAsync(User user, string password, bool emailConfirmed = false)
        {
            var now = DateTime.UtcNow;

            var newUser = new Database.Schema.User(
                user.Username,
                user.FirstName,
                user.LastName,
                user.Mobile,
                user.Gender,
                user.DateOfBirth,
                user.Distance ?? 25,
                user.Latitude,
                user.Longitude,
                user.FCMToken,
                user.Blurb,
                user.AvatarUrl,
                user.Email,
                user.IsMatchNotificationsEnabled,
                user.IsChatNotificationsEnabled)
            {
                CreatedAt = now,
                UpdatedAt = now,
                EmailConfirmed = emailConfirmed
            };

            var retVal = await userManager.CreateAsync(newUser, password);
            if (retVal.Succeeded)
            {
                await UpdateUserRoleAsync(newUser.Id.ToString(), "User");
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

        public async Task<string> GetSixDigitCodeByUserIdAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var userVerificationCode = await db.UserVerificationCodes
                .Where(x => x.Email == user.Email)
                .FirstOrDefaultAsync();

            if (userVerificationCode == null)
            {
                throw new Exception("Verification code not found for the user.");
            }

            return userVerificationCode.SixDigitCode;
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


            var user = await db.Users.Where(u => u.Id.Equals(retVal.CreatedByUserId)).FirstOrDefaultAsync();
            if (user == null) { throw new InfrastructureException("No User Found"); }
            user.EmailConfirmed = true;
            db.Users.Update(user);
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

        public string GenerateRandomPassword(int length = 12)
        {
            string LowerCase = "abcdefghijklmnopqrstuvwxyz";
            string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string Digits = "0123456789";
            string SpecialChars = "!@#$%^&*";
            string charSet = LowerCase + UpperCase + Digits + SpecialChars;
            var random = new Random();
            var password = new char[length];

            password[0] = LowerCase[random.Next(LowerCase.Length)];
            password[1] = UpperCase[random.Next(UpperCase.Length)];
            password[2] = Digits[random.Next(Digits.Length)];
            password[3] = SpecialChars[random.Next(SpecialChars.Length)];

            for (int i = 4; i < length; i++)
            {
                password[i] = charSet[random.Next(charSet.Length)];
            }

            return new string(password.OrderBy(s => Guid.NewGuid()).ToArray());
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }
            var roleExists = await roleManager.RoleExistsAsync(newRole);
            if (!roleExists)
            {
                throw new ArgumentException("Role does not exist");
            }
            var currentRoles = await userManager.GetRolesAsync(user);

            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return false;
            }

            var addResult = await userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                return false;
            }

            return true;
        }
    }
}
