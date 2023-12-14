using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Offers
{
    public class Message
    {
        public Message(Guid? id, Guid createdByUserId, Guid offerId, Guid? userId, string messageText, DateTime? messageReadAt, DateTimeOffset? createdAt, bool? isRead)
        {
            Id = id;
            CreatedByUserId = createdByUserId;
            OfferId = offerId;
            UserId = userId;
            MessageText = messageText;
            MessageReadAt = messageReadAt;
            CreatedAt = createdAt;
            IsRead = isRead;
        }

        [Required]
        public Guid? Id { get; private set; }

        [Required]
        public Guid OfferId { get; private set; }

        public Guid? UserId { get; set; }

        [Required]
        public string MessageText { get; private set; }
        
        [Required]
        public Guid CreatedByUserId { get; private set; }
        
        public DateTime? MessageReadAt { get; private set; }
        public DateTimeOffset? CreatedAt { get; set; }

        public bool? IsRead { get; set; }

        public static Message CreateMessage(
            Guid offerId,
            Guid? userId,
            Guid currentUserId,
            string messageText
        )
        {
            return new Message(
                null,
                currentUserId,
                offerId,
                userId,
                messageText,
                null,
                null,
                false
            );
        }
    }
}
