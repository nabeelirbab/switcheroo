using System;
using System.Threading.Tasks;

namespace Domain.Users
{
    public interface IUserRegistrationService
    {
        Task<Guid> CreateUserAsync(User user, string password);

        Task<bool> VerifyUserAsync(string email, string verificationCode);

        Task<string> GenerateConfirmationCodeAsync(Guid userId);

        Task<string> GetSixDigitCodeByUserIdAsync(Guid userId);

        Task<string> GeneratePasswordResetConfirmationCodeAsync(string email);

        Task<string?> RetrieveResetPasswordTokenAsync(string email, string verificationCode);

        Task<bool> ResetPasswordAsync(string email, string newPassword, string token);

        string GenerateRandomPassword(int length=12);
    }
}
