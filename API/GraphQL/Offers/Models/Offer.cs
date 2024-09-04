using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.GraphQL.Offers.Models;
using Domain.Items;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using Item = API.GraphQL.Items.Models.Item;

namespace API.GraphQL.Models
{
    public class Offer
    {
        public Offer(Guid id, Guid sourceItemId, Guid targetItemId, int? cash, DateTime createdAt, int sourceStatus, int? targeteStatus)
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

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }

        public SwipesInfo SwipesInfo { get; set; }

        public async Task<Users.Models.User?> GetDeletedByUser([Service] IUserRepository userRepository)
        {
            if (DeletedByUserId == null) return null;
            return await GetUserByUserId(userRepository, DeletedByUserId.Value);
        }

        private async Task<Users.Models.User> GetUserByUserId(IUserRepository userRepository, Guid userId)
        {
            var domUser = await userRepository.GetById(userId);

            if (domUser == null) throw new ApiException($"Invalid UserId {userId}");

            return Users.Models.User.FromDomain(domUser);
        }
        public async Task<Item?> GetSourceItem(
            [Service] IItemRepository itemRepository
        )
        {
            var retVal = await itemRepository.GetItemByItemId(SourceItemId);
            if (retVal == null)
                return null;
            return Item.FromDomain(retVal);
        }

        [GraphQLNonNullType]
        public async Task<Item> GetTargetItem(
            [Service] IItemRepository itemRepository
        )
        {
            var retVal = await itemRepository.GetItemByItemId(TargetItemId);
            if (retVal == null)
                return null;
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

        public static Offer FromDomain(Domain.Offers.Offer domOffer)
        {
            if (!domOffer.Id.HasValue) throw new ApiException("Mapping error. Invalid offer");

            return new Offer(domOffer.Id.Value, domOffer.SourceItemId, domOffer.TargetItemId, domOffer.Cash, domOffer.CreatedAt, domOffer.SourceStatus, domOffer.TargeteStatus) { IsDeleted = domOffer.IsDeleted, DeletedAt = domOffer.DeletedAt, DeletedByUserId = domOffer.DeletedByUserId };
        }
        public static List<Offer> FromDomains(List<Domain.Offers.Offer> domOffers)
        {
            var offersList = new List<Offer>();
            if (domOffers == null || domOffers.Count == 0)
            {
                // Handle the case where there are no messages if needed.
                return offersList;
            }
            foreach (var domOffer in domOffers)
            {
                var offer = new Offer(domOffer.Id.Value, domOffer.SourceItemId, domOffer.TargetItemId, domOffer.Cash, domOffer.CreatedAt, domOffer.SourceStatus, domOffer.TargeteStatus) { IsDeleted = domOffer.IsDeleted, DeletedAt = domOffer.DeletedAt, DeletedByUserId = domOffer.DeletedByUserId };
                offersList.Add(offer);
            }
            return offersList;

        }
    }
}