using System;

namespace API.GraphQL.Offers.Models
{
    public class MessageInput
    {
        public Guid OfferId { get; set; }
        
        public string MessageText { get; set; }
    }
}