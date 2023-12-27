using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Items;
using Domain.Offers;
using HotChocolate;
using Item = API.GraphQL.Items.Models.Item;

namespace API.GraphQL.Models
{
    public class Offer
    {
        public Offer(Guid id, Guid sourceItemId, Guid targetItemId, int? cash, DateTime createdAt,int sourceStatus,int? targeteStatus)
        {
            Id = id;
            SourceItemId = sourceItemId;
            TargetItemId = targetItemId;
            Cash = cash;
            CreatedAt = createdAt;
            SourceStatus = sourceStatus;
            TargeteStatus = targeteStatus;
        }
        
        public Guid Id { get; private set; }
        public Guid SourceItemId { get; private set; }
        public Guid TargetItemId { get; private set; }

        public int? Cash { get; private set; }
        
        public DateTime CreatedAt { get; set; }
        public int SourceStatus { get; set; }
        public int? TargeteStatus { get; set; }

        public async Task<Item?> GetSourceItem(
            [Service] IItemRepository itemRepository
        )
        {
            if (Cash == null)
            {
                var retVal = await itemRepository.GetItemByItemId(SourceItemId);
                return Item.FromDomain(retVal);
            }
            else
            {
                return null;
            }
        }
        
        [GraphQLNonNullType]
        public async Task<Item> GetTargetItem(
            [Service] IItemRepository itemRepository
        )
        {
            var retVal = await itemRepository.GetItemByItemId(TargetItemId);
            return Item.FromDomain(retVal);
        }
        
        [GraphQLNonNullType]
        public async Task<List<Message>> GetMessages(
            [Service] IMessageRepository messageRepository
        )
        {
            return (await messageRepository.GetMessagesByOfferId(Id))
                .Select(Message.FromDomain)
                .ToList();
        }
        
        public static Offer FromDomain(Domain.Offers.Offer domOffer) {
            if (!domOffer.Id.HasValue) throw new ApiException("Mapping error. Invalid offer");

            return new Offer(domOffer.Id.Value, domOffer.SourceItemId, domOffer.TargetItemId, domOffer.Cash, domOffer.CreatedAt,domOffer.SourceStatus,domOffer.TargeteStatus);
        }
    }
}