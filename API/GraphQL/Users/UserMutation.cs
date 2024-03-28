using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using API.GraphQL.CommonServices;
using API.GraphQL.Users.Models;
using API.HtmlTemplates;
using Domain.Users;
using GraphQL;
using HotChocolate;
using Infrastructure.UserManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace API.GraphQL
{
    public partial class Mutation
    {
        public async Task<Guid> RegisterUser(
            [Service] IUserRegistrationService userRegistrationService,
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IEmailSender emailSender,
            RegisterUser user,
            string password
        )
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) throw new ApiException("No httpcontext. Well isn't this just awkward?");

            var request = httpContext.Request;
            var userToCreate = Domain.Users.User.CreateNewUser(user.FirstName, user.LastName, user.Email);

            var userId = await userRegistrationService.CreateUserAsync(userToCreate, password);

            var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";

            var emailConfirmCode = await userRegistrationService.GenerateConfirmationCodeAsync(userId);
            var email = new SignUpConfirmationEmail(basePath, emailConfirmCode, userToCreate.Email, $"{userToCreate.FirstName} {userToCreate.LastName}");

            await emailSender.SendEmailAsync(userToCreate.Email, "Switcheroo Email Confirmation", email.GetHtmlString());

            return userId;
        }

        public async Task<bool> VerifyUser(
            [Service] IUserRegistrationService userRegistrationService,
            string email,
            string verificationCode
        )
        {
            var result = await userRegistrationService.VerifyUserAsync(email, verificationCode);

            return result;
        }


        public async Task<bool> ResetPasswordInitiate(
            [Service] IUserRegistrationService userRegistrationService,
            [Service] IUserRepository userRepository,
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IEmailSender emailSender,
            string email
            )
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) throw new ApiException("No httpcontext. Well isn't this just awkward?");

            var user = await userRepository.GetByEmail(email);
            if (user == null) throw new ApiException("Invalid email address");

            var request = httpContext.Request;
            var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";

            var emailConfirmationCode = await userRegistrationService.GeneratePasswordResetConfirmationCodeAsync(email);
            var resetEmail = new PasswordResetConfirmationEmail(basePath, emailConfirmationCode, email, $"{user.FirstName} {user.LastName}");

            await emailSender.SendEmailAsync(user.Email, "Switcheroo Password Reset", resetEmail.GetHtmlString());

            return true;
        }


        public async Task<bool> ResendPassword(
            [Service] IUserRegistrationService userRegistrationService,
            [Service] IUserRepository userRepository,
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IEmailSender emailSender,
            Guid userId,
            string email
            )
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null) throw new ApiException("No httpcontext. Well isn't this just awkward?");

            var user = await userRepository.GetByEmail(email);
            if (user == null) throw new ApiException("Invalid email address");

            var request = httpContext.Request;
            var basePath = $"{request.Scheme}://{request.Host.ToUriComponent()}";

            var emailConfirmationCode = await userRegistrationService.GetSixDigitCodeByUserIdAsync(userId);
            var resetEmail = new PasswordResetConfirmationEmail(basePath, emailConfirmationCode, email, $"{user.FirstName} {user.LastName}");

            await emailSender.SendEmailAsync(user.Email, "Switcheroo Password Reset", resetEmail.GetHtmlString());

            return true;
        }

        public async Task<string?> RetrieveResetPasswordToken(
            [Service] IUserRegistrationService userRegistrationService,
            string email,
            string verificationCode
        )
        {
            return await userRegistrationService.RetrieveResetPasswordTokenAsync(email, verificationCode);
        }

        public async Task<bool> ResetPassword(
            [Service] IUserRegistrationService userRegistrationService,
            string email,
            string newPassword,
            string resetPasswordToken
        )
        {
            return await userRegistrationService.ResetPasswordAsync(email, newPassword, resetPasswordToken);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserProfile(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            string? blurb,
            string? avatarUrl
        )
        {

            var requestUserId = userContextService.GetCurrentUserId();

            return Users.Models.User.FromDomain(await userRepository.UpdateUserProfileDetails(requestUserId, blurb, avatarUrl));
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserName(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            string firstName,
            string lastName
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            return Users.Models.User.FromDomain(await userRepository.UpdateUserName(requestUserId, firstName, lastName));
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserEmail(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            string email
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            return Users.Models.User.FromDomain(await userRepository.UpdateUserEmail(requestUserId, email));
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserMobile(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            string? mobile
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            return Users.Models.User.FromDomain(await userRepository.UpdateUserMobile(requestUserId, mobile));
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserGender(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            string? gender
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            return Users.Models.User.FromDomain(await userRepository.UpdateUserGender(requestUserId, gender));
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserDateOfBirth(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            string? dateOfBirth
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            DateTime.TryParse(dateOfBirth, out var dateOfBirthAsDate);
            var updatedUser = await userRepository.UpdateUserDateOfBirth(requestUserId, dateOfBirth == null ? null : (DateTime?)dateOfBirthAsDate);
            return Users.Models.User.FromDomain(updatedUser);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserDistance(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            int? distance
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            return Users.Models.User.FromDomain(await userRepository.UpdateUserDistance(requestUserId, distance));
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserLocation(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            decimal? latitude,
            decimal? longitude
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            return Users.Models.User.FromDomain(await userRepository.UpdateUserLocation(requestUserId, latitude, longitude));
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> UpdateUserFCMToken(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            string? fcmtoken
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            return Users.Models.User.FromDomain(await userRepository.UpdateUserFCMToken(requestUserId, fcmtoken));
        }


        public async Task<Users.Models.User> SignIn(
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string email,
            string password
        )
        {
            try
            {
                var userId = await userAuthenticationService.SignInAsync(email, password);
                var user = await userRepository.GetById(userId);

                if (!user.Id.HasValue)
                {
                    throw new ApiException("No primary key. Database cooked?");
                }

                return Users.Models.User.FromDomain(user);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        public async Task<Users.Models.User> SignInGoogle(
            [Service] IUserRegistrationService userRegistrationService,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string idToken
        )
        {
            try
            {
                var (existFlag, name, userEmail) = await userAuthenticationService.AuthenticateGoogleAsync(idToken);
                if (!existFlag)
                {
                    var userToCreate = Domain.Users.User.CreateNewUser(name, "", userEmail);
                    var createdUserId = await userRegistrationService.CreateUserAsync(userToCreate, "Abc123##", true);
                }
                var userId = await userAuthenticationService.SignInByEmailAsync(userEmail);
                var user = await userRepository.GetById(userId);

                if (!user.Id.HasValue)
                {
                    throw new ApiException("No primary key. Database cooked?");
                }

                Users.Models.User userInstance = Users.Models.User.FromDomain(user);
                userInstance.InitiateSignUpProcess = !existFlag;
                return userInstance;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        public async Task<Users.Models.User> SignInFacebook(
            [Service] IUserRegistrationService userRegistrationService,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string accessToken
        )
        {
            try
            {
                var (existFlag, name, userEmail) = await userAuthenticationService.AuthenticateFacebookAsync(accessToken);
                if (!existFlag)
                {
                    var userToCreate = Domain.Users.User.CreateNewUser(name, "", userEmail);
                    var createdUserId = await userRegistrationService.CreateUserAsync(userToCreate, "Abc123##", true);
                }
                var userId = await userAuthenticationService.SignInByEmailAsync(userEmail);
                var user = await userRepository.GetById(userId);

                if (!user.Id.HasValue)
                {
                    throw new ApiException("No primary key. Database cooked?");
                }

                Users.Models.User userInstance = Users.Models.User.FromDomain(user);
                userInstance.InitiateSignUpProcess = !existFlag;
                return userInstance;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> SignOut(
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IHttpContextAccessor httpContextAccessor
        )
        {
            var user = httpContextAccessor?.HttpContext?.User;

            if (user == null) return false;
            await userAuthenticationService.SignOutAsync(user);

            return true;
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<bool> DeleteUser(
            [Service] IUserRepository userRepository,
            List<Guid> userIds
        )
        {
            var user = await userRepository.GetUserByUserId(userIds);

            if (user.Count == 0) return false;

            await userRepository.DeleteUser(userIds);
            return true;

        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<bool> CreateRole([Service] IServiceProvider serviceProvider, string roleName)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
            return true;
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        //[HotChocolate.AspNetCore.Authorization.Authorize(Policy = "SuperAdminOnly")]
        public async Task<bool> UpdateUserRole(
            [Service] UserContextService userContextService,
            [Service] IUserRegistrationService userRegistrationService,
            string userId,
            string roleName)
        {
            Guid requestUserId = userContextService.GetCurrentUserId();
            return await userRegistrationService.UpdateUserRoleAsync(userId, roleName);
        }
    }
}
