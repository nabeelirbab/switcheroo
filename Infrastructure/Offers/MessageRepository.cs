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
using Domain.Users;
using Domain.Items;
using Infrastructure.UserManagement;
using Amazon.S3.Model;

namespace Infrastructure.Offers
{
    public class MessageRepository : IMessageRepository
    {
        private readonly SwitcherooContext db;
        private readonly Func<Database.Schema.Message, DateTime> _messageOrderingExpression = z => z.CreatedAt.Date;
        private readonly ILoggerManager _loggerManager;
        private readonly IUserRepository _userRepository;

        public MessageRepository(SwitcherooContext db, ILoggerManager loggerManager, IUserRepository userRepository)
        {
            this.db = db;
            _loggerManager = loggerManager;
            _userRepository = userRepository;
        }

        public async Task<List<Domain.Offers.Message>> GetMessagesByOfferId(Guid offerId)
        {
            var messages = await db.Messages
                .Where(z => z.OfferId == offerId)
                .ToListAsync();

            var msg = messages
                .OrderByDescending(_messageOrderingExpression)
                .Select(message => new Domain.Offers.Message(message.Id, message.CreatedByUserId, message.OfferId, message.Cash, message.CreatedByUserId, message.MessageText, message.MessageReadAt, message.CreatedAt, message.IsRead))
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

                var offers = await db.Offers.
                    Where(o => o.CreatedByUserId == userId || myItems.Contains(o.TargetItemId))
                    .Where(o => o.SourceStatus == o.TargetStatus)
                    .ToListAsync();


                //var offers = await db.Offers
                //        .Where(z => myItems.Contains(z.TargetItemId) || myItems.Contains(z.SourceItemId))
                //        .Where(z => z.SourceStatus == z.TargetStatus)
                //        .ToListAsync();

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

                var offersWithNoMessages = offers.Where(o => offerIdsWithNoMessages.Contains(o.Id)).ToList();
                var itemIds = offersWithNoMessages.SelectMany(offer => new[] { offer.SourceItemId, offer.TargetItemId }).ToList();
                var items = await db.Items
                    .Where(item => itemIds.Contains(item.Id))
                    .ToListAsync();
                //var UsersIds = items
                //    .Where(i => i.CreatedByUserId != userId)
                //    .Select(i => i.CreatedByUserId)
                //    .ToList();

                string message = "";
                var dummyMessages = new List<Domain.Offers.Message>();
                foreach (var offerId in offerIdsWithNoMessages)
                {
                    Domain.Offers.Message newDummyMessage;
                    var offerByOfferId = offers.Where(o => o.Id == offerId).FirstOrDefault();
                    //var targetUserId = items.Where(i => i.Id == offerByOfferId.TargetItemId).Select(i => i.CreatedByUserId).FirstOrDefault();
                    Guid targetUserId;
                    if (offerByOfferId.CreatedByUserId == userId)
                    {
                        targetUserId = items.Where(i => i.Id == offerByOfferId.TargetItemId).Select(i => i.CreatedByUserId).FirstOrDefault();
                    }
                    else
                    {
                        targetUserId = offerByOfferId.CreatedByUserId;
                    }
                    if (offerByOfferId.Cash != null && offerByOfferId.Cash > 0)
                    {
                        newDummyMessage = new Domain.Offers.Message(
                        Guid.NewGuid(),
                            offerByOfferId.CreatedByUserId,
                            offerId,
                            offerByOfferId.Cash,
                            targetUserId,
                            message,
                            null,
                            offerByOfferId.CreatedAt,
                            false
                        );
                        dummyMessages.Add(newDummyMessage);
                    }
                    else
                    {
                        newDummyMessage = new Domain.Offers.Message(
                        Guid.NewGuid(),
                            offerByOfferId.CreatedByUserId,
                            offerId,
                            offerByOfferId.Cash,
                            targetUserId,
                            message,
                            null,
                            offerByOfferId.CreatedAt,
                            false
                        );
                        dummyMessages.Add(newDummyMessage);
                    }

                }
                var offerIdsInLastMessages = lastMessages.Select(l => l.OfferId).ToList();
                var itemIdsInLastMessages = offers.Where(o => offerIdsInLastMessages.Contains(o.Id)).SelectMany(o => new[] { o.SourceItemId, o.TargetItemId }).ToList();
                var itemsInLastMessages = await db.Items
                    .Where(item => itemIdsInLastMessages.Contains(item.Id))
                    .ToListAsync();
                foreach (var _message in lastMessages)
                {
                    var offerInMessage = offers.Where(o => o.Id == _message.OfferId).FirstOrDefault();
                    if (offerInMessage.CreatedByUserId == userId)
                    {
                        _message.UserId = itemsInLastMessages.Where(i => i.Id == offerInMessage.TargetItemId).Select(i => i.CreatedByUserId).FirstOrDefault();
                    }
                    else
                    {
                        _message.UserId = offerInMessage.CreatedByUserId;
                    }
                }
                // Merge lastMessages and dummyMessages
                var mergedMessages = lastMessages
                    .Select(message => new Domain.Offers.Message(
                        message.Id,
                        message.CreatedByUserId,
                        message.OfferId,
                        message.Cash,
                        message.UserId,
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
                            message.UserId,
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
                return mergedMessages.OrderByDescending(m => m.CreatedAt).ThenBy(m => m.IsRead).ToList();
            }
            catch (Exception ex)
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

            return new Domain.Offers.Message(dbMessage.Id, dbMessage.CreatedByUserId, dbMessage.OfferId, dbMessage.Cash, dbMessage.CreatedByUserId, dbMessage.MessageText,
                dbMessage.MessageReadAt, dbMessage.CreatedAt, dbMessage.IsRead);
        }

        public async Task<Domain.Offers.Message> CreateMessageAsync(Domain.Offers.Message message)
        {
            var now = DateTime.UtcNow;

            var newDbItem = new Database.Schema.Message(message.OfferId, message.MessageText, message.MessageReadAt)
            {
                CreatedByUserId = message.CreatedByUserId,
                CreatedAt = now,
                Cash = message.Cash,
                UpdatedAt = now,
                UpdatedByUserId = message.CreatedByUserId,
                IsRead = false
            };

            await db.Messages.AddAsync(newDbItem);

            var senderId = message.CreatedByUserId;
            var offerId = db.Offers.Where(o => o.Id.Equals(message.OfferId)).FirstOrDefault();
            var sourceItem = db.Items.Where(i => i.Id.Equals(offerId.SourceItemId)).FirstOrDefault();
            var targetItem = db.Items.Where(i => i.Id.Equals(offerId.TargetItemId)).FirstOrDefault();
            var sourceItemCreatedUser = db.Users.Where(u => u.Id.Equals(sourceItem.CreatedByUserId)).FirstOrDefault();
            var targetItemCreatedUser = db.Users.Where(u => u.Id.Equals(targetItem.CreatedByUserId)).FirstOrDefault();

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
                try
                {
                    var app = FirebaseApp.DefaultInstance;
                    var messaging = FirebaseMessaging.GetMessaging(app);
                    string targetUserFcm = await _userRepository.GetTargetUserForMessage(message.CreatedByUserId, message.OfferId, true);
                    var notification = new FirebaseAdmin.Messaging.Message()
                    {
                        Token = targetUserFcm,
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
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