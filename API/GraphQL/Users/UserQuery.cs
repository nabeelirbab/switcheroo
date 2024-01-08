using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Users;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Infrastructure.Items;
using Microsoft.AspNetCore.Http;

namespace API.GraphQL
{
    public partial class Query
    {
        [Authorize]
        public async Task<Users.Models.User> GetMe(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService
        )
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);

            if (user == null) throw new ApiException("Not logged in");
            if (!user.Id.HasValue) throw new ApiException("Fatal. Db entity doesn't have a primary key...or you fucked up");

            return Users.Models.User.FromDomain(user);
        }

        [Authorize]
        public async Task<Paginated<Users.Models.User>> GetUsers(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository,
            int limit,
            string? cursor
        )
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);

            if (user == null) throw new ApiException("Not logged in");
            if (!user.Id.HasValue) throw new ApiException("Fatal. Db entity doesn't have a primary key...or you fucked up");

            var paginatedUser = await userRepository.GetAllUsers(limit, cursor);

            var users = new Paginated<Users.Models.User>(
                paginatedUser.Data.Select(Users.Models.User.FromDomain).ToList(),
                paginatedUser.Cursor,
                paginatedUser.TotalCount,
                paginatedUser.HasNextPage);

            return users;
        }

        [Authorize]
        public async Task<bool> NotifyMe(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserRepository userRepository
        )
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);

            if (user == null) throw new ApiException("Not logged in");
            if (!user.Id.HasValue) throw new ApiException("Fatal. Db entity doesn't have a primary key...or you fucked up");

            var notified = await userRepository.NotifyMe(user.Id);

            return notified;
        }
    }
}
