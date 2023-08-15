using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Offers
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetMessagesByOfferId(Guid offerId);
        
        Task<Message> CreateMessageAsync(Message item);
    }
}