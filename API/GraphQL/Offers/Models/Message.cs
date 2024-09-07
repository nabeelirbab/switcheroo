using Domain.Items;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using Infrastructure.Items;
using Infrastructure.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.GraphQL.Models
{
    public class Message
    {
        public Message(Guid id, Guid createdByUserId, Guid offerId, int? cash, Guid? userId, string messageText, DateTime? messageReadAt, DateTimeOffset? createdAt)
        {
            Id = id;
            OfferId = offerId;
            Cash = cash;
            UserId = userId;
            CreatedByUserId = createdByUserId;
            MessageText = messageText;
            MessageReadAt = messageReadAt;
            CreatedAt = createdAt;
        }

        public Guid Id { get; private set; }

        public Guid OfferId { get; private set; }
        public int? Cash { get; private set; }


        public Guid? UserId { get; set; }

        public Guid CreatedByUserId { get; private set; }

        public string MessageText { get; private set; }

        public DateTime? MessageReadAt { get; private set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }

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

        [GraphQLNonNullType]
        public async Task<List<Users.Models.User>> GetTargetUser(
            [Service] IUserRepository userRepository
        )
        {
            List<Users.Models.User> users_list = new List<Users.Models.User>();
            var target_user = Users.Models.User.FromDomain(await userRepository.GetById(UserId));
            users_list.Add(target_user);
            return users_list;
            //return (await userRepository.GetTargetUser(UserId, OfferId))
            //    .Select(Users.Models.User.FromDomain)
            //    .ToList();
        }

        public async Task<List<Items.Models.Item>> GetTargetItem(
            [Service] IItemRepository itemRepository
        )
        {
            var domainItems = await itemRepository.GetTargetItem(OfferId, UserId);

            if (domainItems == null)
            {
                return new List<Items.Models.Item>();
            }

            return domainItems
                .Select(Items.Models.Item.FromDomain)
                .ToList();
        }
        public async Task<Offer> GetOffer(
            [Service] IOfferRepository offerRepository
        )
        {
            var domainOffer = await offerRepository.GetOfferByOfferId(OfferId);
            return Offer.FromDomain(domainOffer);

        }

        public async Task<List<Items.Models.Item>> GetSourceItem(
            [Service] IItemRepository itemRepository
)
        {
            var domainItems = await itemRepository.GetSourceItem(OfferId, UserId);

            if (domainItems == null)
            {
                return new List<Items.Models.Item>();
            }

            return domainItems
                .Select(Items.Models.Item.FromDomain)
                .ToList();
        }


        public static Message FromDomain(Domain.Offers.Message domMessage)
        {
            if (!domMessage.Id.HasValue) throw new ApiException("Mapping error. Invalid message");

            return new Message(domMessage.Id.Value, domMessage.CreatedByUserId, domMessage.OfferId, domMessage.Cash, domMessage.UserId, domMessage.MessageText, domMessage.MessageReadAt, domMessage.CreatedAt)
            {
                IsDeleted = domMessage.IsDeleted,
                DeletedAt = domMessage.DeletedAt,
                DeletedByUserId = domMessage.DeletedByUserId,
            };
        }

        public static List<Message> FromDomain(List<Domain.Offers.Message> domMessages)
        {
            if (domMessages == null || domMessages.Count == 0)
            {
                // Handle the case where there are no messages if needed.
                return new List<Message>();
            }

            return domMessages.Select(domMessage => new Message(
                domMessage.Id.Value,
                domMessage.CreatedByUserId,
                domMessage.OfferId,
                domMessage.Cash,
                domMessage.UserId,
                domMessage.MessageText,
                domMessage.MessageReadAt,
                domMessage.CreatedAt)
            {
                IsDeleted = domMessage.IsDeleted,
                DeletedAt = domMessage.DeletedAt,
                DeletedByUserId = domMessage.DeletedByUserId,
            }).ToList();
        }

    }
}