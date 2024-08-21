using Domain.Users;
using Infrastructure.UserManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.GraphQL.CommonServices
{
    public class UserContextService : IUserRoleProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public UserContextService(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        public Guid GetCurrentUserId()
        {
            var userCp = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = userCp?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                throw new ApiException("Couldn't find user ID claim.");
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new ApiException("User ID claim is not a valid GUID.");
            }

            return userId;
        }

        public string GetCurrentUserEmail()
        {
            var userCp = _httpContextAccessor.HttpContext?.User;
            var emailClaim = userCp?.FindFirst(ClaimTypes.Email)?.Value;

            if (emailClaim == null)
            {
                throw new ApiException("Couldn't find email claim.");
            }

            return emailClaim;
        }

        public async Task<Domain.Users.User> GetCurrentUser()
        {
            var userCp = _httpContextAccessor.HttpContext?.User;
            if (userCp == null) throw new ApiException("Not authenticated");

            // Resolve IUserAuthenticationService using IServiceProvider
            var userAuthenticationService = _serviceProvider.GetRequiredService<IUserAuthenticationService>();
            return await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
        }

        public HttpContext GetHttpRequestContext()
        {
            return _httpContextAccessor.HttpContext;
        }

        public List<string> GetCurrentUserRoles()
        {
            var userCp = _httpContextAccessor.HttpContext?.User;
            if (userCp == null) throw new ApiException("Not authenticated");

            return userCp.FindAll(ClaimTypes.Role).Select(roleClaim => roleClaim.Value).ToList();
        }

        public bool IsAdminOrSuperAdmin
        {
            get
            {
                var userCp = _httpContextAccessor.HttpContext?.User;
                if (userCp == null) return false;

                return userCp.IsInRole("SuperAdmin") || userCp.IsInRole("Admin");
            }
        }
    }
}
