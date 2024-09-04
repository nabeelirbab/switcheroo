using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Domain.Users;
using HotChocolate;

namespace API.GraphQL.Items.Models
{
    public class Item
    {
        private Item(Guid id, string title, string description,
            decimal askingPrice, int? flexibilityRange,
            bool isFlexible, bool isHidden, bool isSwapOnly,
            List<string> categories, List<string> imageUrls, string mainImageUrl,
            Guid createdByUserId, Guid updatedByUserId, decimal? latitude, decimal? longitude)
        {
            Id = id;
            Title = title;
            Description = description;
            FlexibilityRange = flexibilityRange;
            AskingPrice = askingPrice;
            IsFlexible = isFlexible;
            IsHidden = isHidden;
            IsSwapOnly = isSwapOnly;
            Categories = categories;
            ImageUrls = imageUrls;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
            Latitude = latitude;
            Longitude = longitude;
            MainImageUrl = mainImageUrl;
        }
        public Guid Id { get; private set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public decimal AskingPrice { get; private set; }
        public decimal? Latitude { get; private set; }
        public decimal? Longitude { get; private set; }

        public int? FlexibilityRange { get; private set; }

        public bool IsFlexible { get; private set; }

        public bool IsHidden { get; private set; }

        public bool IsSwapOnly { get; private set; }

        //[GraphQLNonNullType]
        public List<string> Categories { get; private set; }

        [GraphQLNonNullType]
        public List<string> ImageUrls { get; private set; }

        public string? MainImageUrl { get; set; } //=> ImageUrls?.Count == 0 ? null : ImageUrls[0];

        public Guid CreatedByUserId { get; private set; }



        public bool? HasMatchingOffer { get; set; }
        public bool? HasCashOffer { get; set; }
        public int? CashOfferValue { get; set; }


        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public Guid? DeletedByUserId { get; set; }



        public async Task<Users.Models.User?> GetDeletedByUser([Service] IUserRepository userRepository)
        {
            if (DeletedByUserId == null) return null;
            return await GetUserByUserId(userRepository, DeletedByUserId.Value);
        }
        public async Task<Users.Models.User> GetCreatedByUser([Service] IUserRepository userRepository)
        {
            return await GetUserByUserId(userRepository, CreatedByUserId);
        }

        public Guid UpdatedByUserId { get; private set; }

        public async Task<Users.Models.User> GetUpdatedByUser([Service] IUserRepository userRepository)
        {
            return await GetUserByUserId(userRepository, UpdatedByUserId);
        }

        private async Task<Users.Models.User> GetUserByUserId(IUserRepository userRepository, Guid userId)
        {
            var domUser = await userRepository.GetById(userId);

            if (domUser == null) throw new ApiException($"Invalid UserId {userId}");

            return Users.Models.User.FromDomain(domUser);
        }

        // Mappers
        public static Item FromDomain(Domain.Items.Item domItem)
        {
            if (!domItem.Id.HasValue) throw new ApiException("Mapping error. Id missing");
            if (!domItem.CreatedByUserId.HasValue) throw new ApiException("Mapping error. CreatedByUserId missing");
            if (!domItem.UpdatedByUserId.HasValue) throw new ApiException("Mapping error. UpdatedByUserId missing");

            return new Item(
                domItem.Id.Value,
                domItem.Title,
                domItem.Description,
                domItem.AskingPrice,
                domItem.FlexibilityRange,
                domItem.IsFlexible,
                domItem.IsHidden,
                domItem.IsSwapOnly,
                domItem.Categories,
                domItem.ImageUrls,
                domItem.MainImageUrl,
                domItem.CreatedByUserId.Value,
                domItem.UpdatedByUserId.Value,
                domItem.Latitude,
                domItem.Longitude
                )
            {
                HasMatchingOffer = domItem.HasMatchingOffer,
                HasCashOffer = domItem.HasCashOffer,
                CashOfferValue = domItem.CashOfferValue,
                IsDeleted = domItem.IsDeleted,
                DeletedAt = domItem.DeletedAt,
                DeletedByUserId = domItem.DeletedByUserId,
            };
        }
    }
}
