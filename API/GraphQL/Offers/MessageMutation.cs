using System;
using System.Threading.Tasks;
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
        private readonly IHubContext<ChatHub> _chatHubContext;

        public Mutation(IHubContext<ChatHub> chatHubContext)
        {
            _chatHubContext = chatHubContext;
        }
        public async Task<Message> CreateMessage(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IMessageRepository messageRepository,
            MessageInput message,
            Guid? receiverId
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var newDomainMessage = await messageRepository.CreateMessageAsync(
                Domain.Offers.Message.CreateMessage(
                    message.OfferId,
                    user.Id.Value,
                    message.MessageText
                 )
             );

            var returnmessage = Message.FromDomain(newDomainMessage);

            await _chatHubContext.Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", returnmessage.MessageText);

            return returnmessage;
        }

        public async Task<bool> MarkmessageCountZero(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IMessageRepository messageRepository
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            await messageRepository.MarkmessageCountZero(user.Id.Value);

            return true;
        }
    }
}