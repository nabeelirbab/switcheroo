using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Users;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Flows;
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
            if (user == null) throw new InfrastructureException("Invalid credentials");
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

        public async Task<Guid> SignInByEmailAsync(string email)
        {
            var domainUser = await userRepository.GetByEmail(email);
            var infraUser = Infrastructure.Database.Schema.User.FromDomain(domainUser);
            await signInManager.SignInAsync(infraUser, true);
            var user = await signInManager.UserManager.FindByEmailAsync(email);
            if (user == null) throw new InfrastructureException("Invalid credentials");
            //else if (!user.EmailConfirmed)
            //{
            //    throw new InfrastructureException("Account not Actived");
            //}
            return user.Id;
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

        public async Task<Tuple<bool, string, string>> AuthenticateGoogleAsync(string idToken)
        {

            //var settings = new GoogleJsonWebSignature.ValidationSettings()
            //{
            //    Audience = new List<string>() {
            //        "752477583659-dsi1seh5cbboe1qhs4pm10vp00d4i4l1.apps.googleusercontent.com",
            //        "752477583659-ehabr9atvt9991kma60bmv3m5k2h1dqq.apps.googleusercontent.com"
            //    }
            //};
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            bool userStatus = await userRepository.CheckIfUserByEmail(payload.Email);
            return new Tuple<bool, string, string>(userStatus, payload.Name, payload.Email);
            //return new Tuple<bool, string, string>(false, "Hamza Muhammad Farooqi", idToken);
        }
    }
}
