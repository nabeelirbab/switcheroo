using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Items;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Offer = API.GraphQL.Models.Offer;

namespace API.GraphQL.Users.Models
{
    public class User
    {

        private User(Guid id, string username, string firstName, string lastName, string email,
            string? mobile, string? gender, DateTime? dateOfBirth, int? distance, int? itemCount, int? matchedItemCount, int? unMatchedItemCount, decimal? latitude, decimal? longitude,
            string? fcmToken, string? blurb, string? avatarUrl, bool isMatchNotification, bool isChatNotificationsEnabled, DateTimeOffset? createdAt)
        {
            Id = id;
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Mobile = mobile;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            Distance = distance;
            ItemCount = itemCount;
            MatchedItemCount = matchedItemCount;
            UnMatchedItemCount = unMatchedItemCount;
            Latitude = latitude;
            Longitude = longitude;
            FCMToken = fcmToken;
            Blurb = blurb;
            AvatarUrl = avatarUrl;
            IsMatchNotification = isMatchNotification;
            IsChatNotificationsEnabled = isChatNotificationsEnabled;
            CreatedAt = createdAt;
        }

        public Guid Id { get; private set; }

        public string Username { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Email { get; set; }

        public string? Mobile { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public int? Distance { get; set; }

        public int? ItemCount { get; set; }

        public int? MatchedItemCount { get; set; }

        public int? UnMatchedItemCount { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? FCMToken { get; set; }

        public string? Blurb { get; private set; }

        public string? AvatarUrl { get; private set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public bool IsMatchNotification { get; private set; }

        public bool IsChatNotificationsEnabled { get; private set; }

        public bool InitiateSignUpProcess { get; set; }

        [GraphQLNonNullType]
        public async Task<IEnumerable<Offer>> GetOffers(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");
            return (await offerRepository.GetAllOffers(user.Id.Value)).Select(Offer.FromDomain);
        }

        [GraphQLNonNullType]
        public async Task<List<Items.Models.Item>> Items(
            [Service] IItemRepository itemRepository
        )
        {
            var retVal = await itemRepository.GetItemsByUserId(Id);

            return retVal
                .Select(GraphQL.Items.Models.Item.FromDomain)
                .ToList();
        }

        // Mappers
        public static User FromDomain(Domain.Users.User domUser)
        {
            if (!domUser.Id.HasValue) throw new ApiException("Mapping error. Invalid user");

            return new User(
                domUser.Id.Value,
                domUser.Username,
                domUser.FirstName,
                domUser.LastName,
                domUser.Email,
                domUser.Mobile,
                domUser.Gender,
                domUser.DateOfBirth,
                domUser.Distance,
                domUser.ItemCount,
                domUser.MatchedItemCount,
                domUser.UnMatchedItemCount,
                domUser.Latitude,
                domUser.Longitude,
                domUser.FCMToken,
                domUser.Blurb,
                domUser.AvatarUrl,
                domUser.IsMatchNotificationsEnabled,
                domUser.IsChatNotificationsEnabled,
                domUser.CreatedAt
            );
        }
    }
}
