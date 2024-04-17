using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Offers
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetMessagesByOfferId(Guid offerId);
        Task<List<Message>> GetChat(Guid userId);
        Task<List<Message>> GetAllChat();
        Task<List<Message>> GetAllChatByUser(Guid userId);
        Task<int> GetChatCount(Guid userId);

        Task<int> GetMessagesCount(Guid userId);
        Task<bool> MarkmessageCountZero(Guid userId);

        Task<Message> CreateMessageAsync(Message item);
    }
}