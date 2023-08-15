using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.UserManagement
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly SignInManager<Database.Schema.User> signInManager;
        private readonly UserManager<Database.Schema.User> userManager;
        private readonly IUserRepository userRepository;

        public UserAuthenticationService(SignInManager<Database.Schema.User> signInManager, UserManager<Database.Schema.User> userManager, IUserRepository userRepository)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.userRepository = userRepository;
        }

        public async Task<User> GetCurrentlySignedInUserAsync(ClaimsPrincipal principal)
        {
            if (!principal?.Identity?.IsAuthenticated ?? false) throw new InfrastructureException("Unauthenticated");

            var user = await userManager.GetUserAsync(principal);
            if (user == null) throw new InfrastructureException("Unauthenticated");

            return await userRepository.GetById(user.Id);
        }

        public async Task<Guid> SignInAsync(string email, string password)
        {
            var result = await signInManager.PasswordSignInAsync(email, password, true, false);

            if (result.Succeeded)
            {
                var user = await signInManager.UserManager.FindByEmailAsync(email);

                return user.Id;
            }

            throw new InfrastructureException("Invalid credentials");
        }

        public async Task<Guid> SignOutAsync(ClaimsPrincipal principal)
        {
            var isAuthenticated = principal?.Identity?.IsAuthenticated;
            if (isAuthenticated == null || isAuthenticated == false)
            {
                throw new InfrastructureException("Unauthenticated");
            }

            var user = await userManager.GetUserAsync(principal);
            if (user == null) throw new InfrastructureException("Unauthenticated");

            await signInManager.SignOutAsync();

            return user.Id;
        }
    }
}
