using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Domain.Users
{
    public interface IUserAuthenticationService
    {
        Task<Guid> SignInAsync(string email, string password);

        Task<Guid> SignInByEmailAsync(string email);

        Task<Guid> SignOutAsync(ClaimsPrincipal principal);

        Task<User> GetCurrentlySignedInUserAsync(ClaimsPrincipal principal);

        Task<Tuple<bool, string, string>> AuthenticateGoogleAsync(string idToken);
        Task<Tuple<bool, string, string>> AuthenticateFacebookAsync(string accessToken);
        Task<Tuple<bool,bool, string>> AuthenticateAppleAsync(string token);
    }
}
