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
                var successUser = await signInManager.UserManager.FindByEmailAsync(email);

                return successUser.Id;
            }

            var user = await signInManager.UserManager.FindByEmailAsync(email);
            
            // if email is invalid
            if(user == null) throw new InfrastructureException("Invalid credentials");
            else
            {
                // if email is valid but not confirmed
                if (!user.EmailConfirmed)
                {
                    throw new InfrastructureException("Account not Actived");
                }
                // if email is valid but password id wrong
                else
                {
                    throw new InfrastructureException("Invalid credentials");
                }
            }
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
