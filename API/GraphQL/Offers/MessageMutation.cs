using System;
using System.Threading.Tasks;
using API.GraphQL.Items.Models;
using API.GraphQL.Offers.Models;
using Domain.Items;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Message = API.GraphQL.Models.Message;

namespace API.GraphQL
{
    public partial class Mutation
    {
        public async Task<Message> CreateMessage(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IMessageRepository messageRepository,
            MessageInput message
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

            return Message.FromDomain(newDomainMessage);
        }
    }
}