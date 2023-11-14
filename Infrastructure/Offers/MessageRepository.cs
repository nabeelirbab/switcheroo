﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Offers;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Offers
{
    public class MessageRepository: IMessageRepository
    {
        private readonly SwitcherooContext db;
        private readonly Func<Database.Schema.Message, DateTime> _messageOrderingExpression = z => z.CreatedAt.Date;

        public MessageRepository(SwitcherooContext db)
        {
            this.db = db;
        }
        
        public async Task<List<Domain.Offers.Message>> GetMessagesByOfferId(Guid offerId)
        {
            var messages = await db.Messages
                .Where(z => z.OfferId == offerId)
                .ToListAsync();

            var msg =  messages
                .OrderByDescending(_messageOrderingExpression)
                .Select(message => new Domain.Offers.Message(message.Id, message.CreatedByUserId, message.OfferId, message.MessageText, message.MessageReadAt))
                .ToList();
            return msg;
        }

        public async Task<Domain.Offers.Message> GetMessageById(Guid messageId)
        {
            var dbMessage = await db.Messages
                .SingleOrDefaultAsync(z => z.Id == messageId);
            
            return new Domain.Offers.Message(dbMessage.Id, dbMessage.CreatedByUserId, dbMessage.OfferId, dbMessage.MessageText,
                dbMessage.MessageReadAt);
        }

        public async Task<Domain.Offers.Message> CreateMessageAsync(Domain.Offers.Message message)
        {
            var now = DateTime.UtcNow;

            var newDbItem = new Database.Schema.Message(message.OfferId, message.MessageText, message.MessageReadAt)
            {
                CreatedByUserId = message.CreatedByUserId,
                CreatedAt = now,
                UpdatedAt = now,
                UpdatedByUserId = message.CreatedByUserId
            };

            var offer = db.Offers.FirstOrDefault(x => x.Id.Equals(message.OfferId));
            if (offer==null) throw new InfrastructureException("offer is null");

            var itemId = db.Items.FirstOrDefault(x => x.Id.Equals(offer.TargetItemId));
            if (itemId == null) throw new InfrastructureException("offer is null");

            var userFCMToken = db.Users
                .Where(x => x.Id == itemId.CreatedByUserId)
                .Select(x => x.FCMToken).FirstOrDefault();

            await db.Messages.AddAsync(newDbItem);
            if (!string.IsNullOrEmpty(userFCMToken))
            {
                var app = FirebaseApp.DefaultInstance;
                var messaging = FirebaseMessaging.GetMessaging(app);

                var notification = new FirebaseAdmin.Messaging.Message()
                {
                    Token = userFCMToken,
                    Notification = new Notification
                    {
                        Title = "Message",
                        Body = message.MessageText
                        // Other notification parameters can be added here
                    }
                };
                string response = await messaging.SendAsync(notification);
            }
            else
            {
                throw new InfrastructureException($"No FCM Token exists for this user");
            }
            await db.SaveChangesAsync();

            return await GetMessageById(newDbItem.Id);
        }
    }
}