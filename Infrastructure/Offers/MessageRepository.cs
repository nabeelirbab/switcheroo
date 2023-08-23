using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Offers;
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
        
        public async Task<List<Message>> GetMessagesByOfferId(Guid offerId)
        {
            var messages = await db.Messages
                .Where(z => z.OfferId == offerId)
                .ToListAsync();

            return messages
                .OrderByDescending(_messageOrderingExpression)
                .Select(message => new Message(message.Id, message.CreatedByUserId, message.OfferId, message.MessageText, message.MessageReadAt))
                .ToList();
        }

        public async Task<Message> GetMessageById(Guid messageId)
        {
            var dbMessage = await db.Messages
                .SingleOrDefaultAsync(z => z.Id == messageId);
            
            return new Message(dbMessage.Id, dbMessage.CreatedByUserId, dbMessage.OfferId, dbMessage.MessageText,
                dbMessage.MessageReadAt);
        }

        public async Task<Message> CreateMessageAsync(Message message)
        {
            var now = DateTime.UtcNow;

            var newDbItem = new Database.Schema.Message(message.OfferId, message.MessageText, message.MessageReadAt)
            {
                CreatedByUserId = message.CreatedByUserId,
                CreatedAt = now,
                UpdatedAt = now,
                UpdatedByUserId = message.CreatedByUserId
            };

            await db.Messages.AddAsync(newDbItem);

            await db.SaveChangesAsync();

            return await GetMessageById(newDbItem.Id);
        }
    }
}