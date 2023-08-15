using System;

namespace API.GraphQL.Models
{
    public class Message
    {
        public Message(Guid id, Guid createdByUserId, Guid offerId, string messageText, DateTime? messageReadAt)
        {
            Id = id;
            OfferId = offerId;
            CreatedByUserId = createdByUserId;
            MessageText = messageText;
            MessageReadAt = messageReadAt;
        }

        public Guid Id { get; private set; }

        public Guid OfferId { get; private set; }
        
        public Guid CreatedByUserId { get; private set; }

        public string MessageText { get; private set; }
        
        public DateTime? MessageReadAt { get; private set; }
        
        public static Message FromDomain(Domain.Offers.Message domMessage) {
            if (!domMessage.Id.HasValue) throw new ApiException("Mapping error. Invalid message");

            return new Message(domMessage.Id.Value, domMessage.CreatedByUserId, domMessage.OfferId, domMessage.MessageText, domMessage.MessageReadAt);
        }
    }
}