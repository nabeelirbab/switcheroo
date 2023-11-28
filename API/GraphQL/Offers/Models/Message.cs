using Domain.Items;
using Domain.Users;
using HotChocolate;
using Infrastructure.Items;
using Infrastructure.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.GraphQL.Models
{
    public class Message
    {
        public Message(Guid id, Guid createdByUserId, Guid offerId, string messageText, DateTime? messageReadAt, DateTimeOffset? createdAt)
        {
            Id = id;
            OfferId = offerId;
            CreatedByUserId = createdByUserId;
            MessageText = messageText;
            MessageReadAt = messageReadAt;
            CreatedAt = createdAt;
        }

        public Guid Id { get; private set; }

        public Guid OfferId { get; private set; }
        
        public Guid CreatedByUserId { get; private set; }

        public string MessageText { get; private set; }
        
        public DateTime? MessageReadAt { get; private set; }

        public DateTimeOffset? CreatedAt { get; set; }

        [GraphQLNonNullType]
        public async Task<List<Users.Models.User>> GetTargetUser(
            [Service] IUserRepository userRepository
        )
        {
            return (await userRepository.GetUserByUserId(CreatedByUserId))
                .Select(Users.Models.User.FromDomain)
                .ToList();
        }

        public static Message FromDomain(Domain.Offers.Message domMessage) {
            if (!domMessage.Id.HasValue) throw new ApiException("Mapping error. Invalid message");

            return new Message(domMessage.Id.Value, domMessage.CreatedByUserId, domMessage.OfferId, domMessage.MessageText, domMessage.MessageReadAt, domMessage.CreatedAt);
        }

        public static List<Message> FromDomain(List<Domain.Offers.Message> domMessages)
        {
            if (domMessages == null || domMessages.Count == 0)
            {
                // Handle the case where there are no messages if needed.
                return new List<Message>();
            }

            return domMessages.Select(domMessage => new Message(
                domMessage.Id.Value,
                domMessage.CreatedByUserId,
                domMessage.OfferId,
                domMessage.MessageText,
                domMessage.MessageReadAt,
                domMessage.CreatedAt))
                .ToList();
        }

    }
}