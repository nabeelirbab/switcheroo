using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace API
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(object message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
