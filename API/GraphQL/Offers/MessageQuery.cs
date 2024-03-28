using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.GraphQL.CommonServices;
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
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<List<Message>> GetMessages(
            [Service] IMessageRepository messageRepository,
            Guid offerId)
        {
            return Message.FromDomain(await messageRepository.GetMessagesByOfferId(offerId));
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<List<Message>> GetChat(
            [Service] UserContextService userContextService,
            [Service] IMessageRepository messageRepository)
        {
            try
            {
                var chats = Message.FromDomain(await messageRepository.GetChat(userContextService.GetCurrentUserId()));
                return chats;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Api Exception {ex.Message}");
            }
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<int> GetChatCount(
            [Service] UserContextService userContextService,
            [Service] IMessageRepository messageRepository)
        {
            var chatCount = messageRepository.GetChatCount(userContextService.GetCurrentUserId());
            return await chatCount;
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<int> GetMessagesCount(
            [Service] UserContextService userContextService,
            [Service] IMessageRepository messageRepository)
        {
            var chatCount = messageRepository.GetMessagesCount(userContextService.GetCurrentUserId());
            return await chatCount;
        }
    }
}