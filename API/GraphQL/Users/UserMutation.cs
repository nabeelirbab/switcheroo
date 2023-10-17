using System;
using System.Threading.Tasks;
using API.GraphQL.Users.Models;
using API.HtmlTemplates;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore.Storage;

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

        public async Task<Users.Models.User> UpdateUserProfile(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string? blurb,
            string? avatarUrl
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            return Users.Models.User.FromDomain(await userRepository.UpdateUserProfileDetails(user.Id.Value, blurb, avatarUrl));
        }

        public async Task<Users.Models.User> UpdateUserName(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string firstName,
            string lastName
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            return Users.Models.User.FromDomain(await userRepository.UpdateUserName(user.Id.Value, firstName, lastName));
        }

        public async Task<Users.Models.User> UpdateUserEmail(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string email
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            return Users.Models.User.FromDomain(await userRepository.UpdateUserEmail(user.Id.Value, email));
        }

        public async Task<Users.Models.User> UpdateUserMobile(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string? mobile
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            return Users.Models.User.FromDomain(await userRepository.UpdateUserMobile(user.Id.Value, mobile));
        }

        public async Task<Users.Models.User> UpdateUserGender(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string? gender
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            return Users.Models.User.FromDomain(await userRepository.UpdateUserGender(user.Id.Value, gender));
        }

        public async Task<Users.Models.User> UpdateUserDateOfBirth(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string? dateOfBirth
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            DateTime.TryParse(dateOfBirth, out var dateOfBirthAsDate);
            var updatedUser = await userRepository.UpdateUserDateOfBirth(user.Id.Value, dateOfBirth == null ? null : (DateTime?)dateOfBirthAsDate);
            return Users.Models.User.FromDomain(updatedUser);
        }

        public async Task<Users.Models.User> UpdateUserDistance(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            int? distance
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            return Users.Models.User.FromDomain(await userRepository.UpdateUserDistance(user.Id.Value, distance));
        }

        public async Task<Users.Models.User> SignIn(
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            string email,
            string password
        )
        {
            var userId = await userAuthenticationService.SignInAsync(email, password);
            var user = await userRepository.GetById(userId);

            if (!user.Id.HasValue)
            {
                throw new ApiException("No primary key. Database cooked?");
            }

            return Users.Models.User.FromDomain(user);
        }

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

        public async Task<bool> DeleteUser(
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IHttpContextAccessor httpContextAccessor
        )
        {
            var user = httpContextAccessor?.HttpContext?.User;

            if (user == null) return false;
            Console.WriteLine($"user in mutation {user}");
            await userAuthenticationService.DeleteUserAsync(user);

            return true;
        }
    }
}
