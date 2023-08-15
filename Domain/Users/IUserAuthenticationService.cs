using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Domain.Users
{
    public interface IUserAuthenticationService
    {
        Task<Guid> SignInAsync(string email, string password);

        Task<Guid> SignOutAsync(ClaimsPrincipal principal);

        Task<User> GetCurrentlySignedInUserAsync(ClaimsPrincipal principal);
    }
}
