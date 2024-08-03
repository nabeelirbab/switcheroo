using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Users;
using Domain.UserAnalytics;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using API.GraphQL.CommonServices;

namespace API.GraphQL
{
    public partial class Query
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Users.Models.User> GetMe(
            [Service] UserContextService userContextService
        )
        {
            var user = await userContextService.GetCurrentUser();
            return Users.Models.User.FromDomain(user);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<Paginated<Users.Models.User>> GetUsers(
            [Service] IUserRepository userRepository,
            int limit,
            string? cursor
        )
        {
            var paginatedUser = await userRepository.GetAllUsers(limit, cursor);

            var users = new Paginated<Users.Models.User>(
                paginatedUser.Data.Select(Users.Models.User.FromDomain).ToList(),
                paginatedUser.Cursor,
                paginatedUser.TotalCount,
                paginatedUser.HasNextPage);

            return users;
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<KeyValue>> GetUsersGenderCount(
            [Service] IUserRepository userRepository
        )
        {
            var usersCount = await userRepository.GetUsersGenderCount();
            return usersCount;
        }

        //[HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> NotifyMe(
            [Service] UserContextService userContextService,
            [Service] IUserRepository userRepository,
            string device_token
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            //var notified = await userRepository.NotifyMe(requestUserId, newMatchingNotification, newCashOfferNotification, cashOfferAcceptedNotification);
            await userRepository.SendFireBaseNotification("Test", "This is test notification", [device_token], "", "");
            return true;
        }


    }
}
