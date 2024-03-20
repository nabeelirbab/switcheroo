using System;
using System.Threading.Tasks;
using API.GraphQL.CommonServices;
using API.GraphQL.Offers.Models;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Message = API.GraphQL.Models.Message;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Message> CreateMessage(
            [Service] UserContextService userContextService,
            [Service] IMessageRepository messageRepository,
            [Service] IUserRepository userRepository,
            MessageInput message
        )
        {
            try
            {
                var requestUserId = userContextService.GetCurrentUserId();
                var newDomainMessage = await messageRepository.CreateMessageAsync(
                    Domain.Offers.Message.CreateMessage(
                        message.OfferId,
                        message.Cash,
                        requestUserId,
                        requestUserId,
                        message.MessageText
                     )
                 );

                var returnmessage = Message.FromDomain(newDomainMessage);
                var targetUserId = await userRepository.GetTargetUserForMessage(requestUserId, message.OfferId, false);
                if (targetUserId != null)
                    await _chatHubContext.Clients.User(targetUserId).SendAsync("ReceiveMessage", returnmessage);
                return returnmessage;
            }
            catch (Exception ex)
            {
                throw new ApiException($"API Exception {ex.Message}");
            }
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> MarkmessageCountZero(
            [Service] UserContextService userContextService,
            [Service] IMessageRepository messageRepository
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            await messageRepository.MarkmessageCountZero(requestUserId);
            return true;
        }
    }
}