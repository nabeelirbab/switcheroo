using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Offers;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Infrastructure.Database.Schema;
using System.Threading;
using Domain.Services;

namespace Infrastructure.Offers
{
    public class MessageRepository : IMessageRepository
    {
        private readonly SwitcherooContext db;
        private readonly Func<Database.Schema.Message, DateTime> _messageOrderingExpression = z => z.CreatedAt.Date;
        private readonly ILoggerManager _loggerManager;

        public MessageRepository(SwitcherooContext db, ILoggerManager loggerManager)
        {
            this.db = db;
            _loggerManager = loggerManager;
        }

        public async Task<List<Domain.Offers.Message>> GetMessagesByOfferId(Guid offerId)
        {
            var messages = await db.Messages
                .Where(z => z.OfferId == offerId)
                .ToListAsync();

            var msg = messages
                .OrderByDescending(_messageOrderingExpression)
                .Select(message => new Domain.Offers.Message(message.Id, message.CreatedByUserId, message.OfferId,message.Cash, message.CreatedByUserId, message.MessageText, message.MessageReadAt, message.CreatedAt, message.IsRead))
                .ToList();
            return msg;
        }

        public async Task<List<Domain.Offers.Message>> GetChat(Guid userId)
        {
            try
            {
                var now = DateTime.Now;
                var myItems = await db.Items
                   .Where(z => z.CreatedByUserId == userId)
                   .Select(z => z.Id)
                   .ToArrayAsync();
                var simpleOffers = await db.Offers
                        .Where(z => myItems.Contains(z.TargetItemId) || myItems.Contains(z.SourceItemId))
                        .Where(z => z.SourceStatus == z.TargetStatus)
                        .ToListAsync();

                //get created cashed offer
                var createdCashffer = await db.Offers
                    .Where(o => o.SourceItemId == Guid.Empty && o.CreatedByUserId == userId)
                    .Where(z => z.SourceStatus == z.TargetStatus)
                    .ToListAsync();

                //get received cashed offer
                var receivedCashffer = await db.Offers
                    .Where(o => myItems.Contains(o.TargetItemId))
                    .Where(z => z.SourceStatus == z.TargetStatus)
                    .ToListAsync();

                //merge all offers
                var offers = simpleOffers
                    .Union(createdCashffer)
                    .Union(receivedCashffer)
                    .GroupBy(o => o.Id)
                    .Select(g => g.First())
                    .ToList();


                // Offer IDs with messages
                var lastMessages = await db.Messages
                        .Where(message => offers.Select(o => o.Id).Contains(message.OfferId))
                        .GroupBy(message => message.OfferId)
                        .Select(group => group.OrderByDescending(m => m.CreatedAt).FirstOrDefault())
                        .ToListAsync();

                // Offer IDs without any messages
                var offerIdsWithNoMessages = offers
                    .Where(offer => !db.Messages.Any(message => message.OfferId == offer.Id))
                    .Select(offer => offer.Id)
                    .ToList();

                // Offer without any messages
                var offersWithNoMessages = offers
                    .Where(offer => offerIdsWithNoMessages.Contains(offer.Id))
                    .ToList();

                var itemIds = offersWithNoMessages.SelectMany(offer => new[] { offer.SourceItemId, offer.TargetItemId }).ToList();

                var validItemIds = itemIds.Where(id => id != null && id != Guid.Empty).ToList();

                var items = await db.Items
                    .Where(item => validItemIds.Contains(item.Id))
                    .ToListAsync();

                // target user here userId is source User
                var usersIds = items
                    .Where(i => i.CreatedByUserId != userId)
                    .Select(i => i.CreatedByUserId)
                    .ToList();

                // get target user of cashed offer
                var targetUserIds = offers.Select(c => c.CreatedByUserId).ToList();
                Console.WriteLine(targetUserIds.Count);

                // merge both users
                // Merge both lists of user IDs without duplicates
                var mergedUserIds = usersIds
                        .Concat(targetUserIds.Except(usersIds))
                        .ToList();


                // Create dummy messages for offerIds without associated messages
                string message = "";
                var dummyMessages = new List<Domain.Offers.Message>();
                foreach (var offerId in offerIdsWithNoMessages)
                {
                    var cash = db.Offers.Where(x=>x.Id==offerId).Select(x=>x.Cash).FirstOrDefault();
                    foreach (var user in mergedUserIds)
                    {
                        var newDummyMessage = new Domain.Offers.Message(
                            Guid.NewGuid(),
                            user,
                            offerId,
                            cash,
                            userId,
                            message,
                            null,
                            null,
                            false
                        );
                        dummyMessages.Add(newDummyMessage);
                    }
                }


                // Merge lastMessages and dummyMessages
                var mergedMessages = lastMessages
                    .Select(message => new Domain.Offers.Message(
                        message.Id,
                        message.CreatedByUserId,
                        message.OfferId,
                        message.Cash,
                        message.UserId = userId,
                        message.MessageText,
                        message.MessageReadAt,
                        message.CreatedAt,
                        message.IsRead
                    ))
                    .Concat(dummyMessages
                        .Select(message => new Domain.Offers.Message(
                            message.Id,
                            message.CreatedByUserId,
                            message.OfferId,
                            message.Cash,
                            message.UserId = userId,
                            message.MessageText,
                            message.MessageReadAt,
                            message.CreatedAt,
                            message.IsRead
                        ))
                    )
                    .GroupBy(message => message.OfferId)
                    .Select(group => group.First())
                    .ToList();

                foreach (var readeMessage in mergedMessages.Where(m => m.CreatedByUserId == userId))
                {
                    if (readeMessage.MessageReadAt == null)
                    {
                        readeMessage.MessageReadAt = now;
                    }
                }
                return mergedMessages;
            }
            catch( Exception ex )
            {
                throw new InfrastructureException($"Infrastructure Exception {ex.Message}");
            }
        }

        public async Task<int> GetChatCount(Guid userId)
        {
            var itemIds = await db.Items
               .Where(z => z.CreatedByUserId == userId)
               .Select(z => z.Id)
               .ToArrayAsync();

            // Retrieve received offers using myItems
            var offerIds = await db.Offers
                .Where(z => itemIds.Contains(z.TargetItemId))
                .Where(offer => (int)offer.SourceStatus == (int)offer.TargetStatus)
                .Select(offer => offer.Id)
                .ToArrayAsync();

            // Retrieve received offers using myItems
            var messages = await db.Messages
                .Where(message => offerIds.Contains(message.OfferId) && message.MessageReadAt == null)
                .ToListAsync();

            return messages.Count;
        }

        public async Task<bool> MarkmessageCountZero(Guid userId)
        {
            var itemIds = await db.Items
               .Where(z => z.CreatedByUserId == userId)
               .Select(z => z.Id)
               .ToArrayAsync();

            // Retrieve received offers using myItems
            var offerIds = await db.Offers
                .Where(z => itemIds.Contains(z.TargetItemId))
                .Select(offer => offer.Id)
                .ToArrayAsync();

            // Retrieve received offers using myItems
            var messages = await db.Messages
                .Where(message => offerIds.Contains(message.OfferId) && message.IsRead == false)
            .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await db.SaveChangesAsync(); 

            return true;
        }

        public async Task<Domain.Offers.Message> GetMessageById(Guid messageId)
        {
            var dbMessage = await db.Messages
                .SingleOrDefaultAsync(z => z.Id == messageId);

            return new Domain.Offers.Message(dbMessage.Id, dbMessage.CreatedByUserId, dbMessage.OfferId,dbMessage.Cash, dbMessage.CreatedByUserId, dbMessage.MessageText,
                dbMessage.MessageReadAt, dbMessage.CreatedAt, dbMessage.IsRead);
        }

        public async Task<Domain.Offers.Message> CreateMessageAsync(Domain.Offers.Message message)
        {
            var now = DateTime.UtcNow;

            var newDbItem = new Database.Schema.Message(message.OfferId, message.MessageText, message.MessageReadAt)
            {
                CreatedByUserId = message.CreatedByUserId,
                CreatedAt = now,
                Cash=message.Cash,
                UpdatedAt = now,
                UpdatedByUserId = message.CreatedByUserId,
                IsRead=false
            };

            await db.Messages.AddAsync(newDbItem);

            var senderId = message.CreatedByUserId;
            var offerId = db.Offers.Where(o=>o.Id.Equals(message.OfferId)).FirstOrDefault(); 
            var sourceItem = db.Items.Where(i=>i.Id.Equals(offerId.SourceItemId)).FirstOrDefault();
            var targetItem = db.Items.Where(i=>i.Id.Equals(offerId.TargetItemId)).FirstOrDefault();
            var sourceItemCreatedUser = db.Users.Where(u=>u.Id.Equals(sourceItem.CreatedByUserId)).FirstOrDefault();
            var targetItemCreatedUser = db.Users.Where(u=>u.Id.Equals(targetItem.CreatedByUserId)).FirstOrDefault();

            var userFCM = "";
            if (senderId != sourceItemCreatedUser.Id)
            {
                userFCM = sourceItemCreatedUser.FCMToken;
            }
            else
            {
                userFCM = targetItemCreatedUser.FCMToken;
            }

            if (!string.IsNullOrEmpty(userFCM))
            {
                var app = FirebaseApp.DefaultInstance;
                var messaging = FirebaseMessaging.GetMessaging(app);

                var notification = new FirebaseAdmin.Messaging.Message()
                {
                    Token = userFCM,
                    Notification = new Notification
                    {
                        Title = "Message",
                        Body = message.MessageText
                        // Other notification parameters can be added here
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "datetime", DateTime.Now.ToString() }
                        // You can add other data fields as needed
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

        public async Task<int> GetMessagesCount(Guid userId)
        {
            var itemIds = await db.Items
               .Where(z => z.CreatedByUserId == userId)
               .Select(z => z.Id)
               .ToArrayAsync();

            // Retrieve received offers using myItems
            var offerIds = await db.Offers
                .Where(z => itemIds.Contains(z.TargetItemId))
                .Select(offer => offer.Id)
                .ToArrayAsync();

            // Retrieve received offers using myItems
            var messages = await db.Messages
                .Where(message => offerIds.Contains(message.OfferId) && message.IsRead == false)
            .ToListAsync();

            return messages.Count;
        }
    }
}