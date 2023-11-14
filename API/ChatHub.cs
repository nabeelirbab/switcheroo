using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace API
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(Guid userId, object message)
        {
            await Clients.User(userId.ToString()).SendAsync("ReceiveMessage", message);
        }
    }
}
