using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Message = API.GraphQL.Models.Message;

namespace API.GraphQL
{
    public partial class Query
    {
        [Authorize]
        public async Task<List<Message>> GetMessages(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IMessageRepository messageRepository,
            Guid offerId)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");

            return Message.FromDomain(await messageRepository.GetMessagesByOfferId(offerId));
        }

        [Authorize]
        public async Task<List<Message>> GetChat(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IMessageRepository messageRepository)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");

            return Message.FromDomain(await messageRepository.GetChat((Guid)user.Id));
        }
    }
}