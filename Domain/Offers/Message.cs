﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Offers
{
    public class Message
    {
        public Message(Guid? id, Guid createdByUserId, Guid offerId, string messageText, DateTime? messageReadAt)
        {
            Id = id;
            CreatedByUserId = createdByUserId;
            OfferId = offerId;
            MessageText = messageText;
            MessageReadAt = messageReadAt;
        }

        [Required]
        public Guid? Id { get; private set; }

        [Required]
        public Guid OfferId { get; private set; }

        [Required]
        public string MessageText { get; private set; }
        
        [Required]
        public Guid CreatedByUserId { get; private set; }
        
        public DateTime? MessageReadAt { get; private set; }
        
        
        public static Message CreateMessage(
            Guid offerId,
            Guid currentUserId,
            string messageText
        )
        {
            return new Message(
                null,
                currentUserId,
                offerId,
                messageText,
                null
            );
        }
    }
}
