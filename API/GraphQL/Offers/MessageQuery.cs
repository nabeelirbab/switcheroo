using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
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
            try
            {
                var claimsPrinciple = httpContextAccessor.HttpContext.User;
                var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
                if (!user.Id.HasValue) throw new ApiException("Fatal.");

                if (user == null) throw new ApiException("Not logged in");

                var chats = Message.FromDomain(await messageRepository.GetChat((Guid)user.Id));

                return chats;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Api Exception {ex.Message}");
            }
        }

        [Authorize]
        public async Task<int> GetChatCount(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IMessageRepository messageRepository)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");

            var chatCount = messageRepository.GetChatCount((Guid)user.Id);

            return await chatCount;
        }

        [Authorize]
        public async Task<int> GetMessagesCount(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IMessageRepository messageRepository)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");

            var chatCount = messageRepository.GetMessagesCount((Guid)user.Id);

            return await chatCount;
        }
    }
}