using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.GraphQL.CommonServices;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Offer = API.GraphQL.Models.Offer;

namespace API.GraphQL
{
    public partial class Query
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Offer> GetOffer(
            [Service] UserContextService userContextService,
            [Service] IOfferRepository offerRepository,
            string offerId)
        {
            return Offer.FromDomain(await offerRepository.GetOfferById(userContextService.GetCurrentUserId(), Guid.Parse(offerId)));
        }

        [Authorize]
        public async Task<IEnumerable<Offer>> GetAllOffersByItemId(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository,
            Guid itemId)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");

            var offers = await offerRepository.GetAllOffersByItemId(itemId);

            return offers.Select(Offer.FromDomain).ToList();
        }

        [Authorize]
        public async Task<IEnumerable<Offer>> GetCreatedOffers(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");

            var offers = await offerRepository.GetCreatedOffers(user.Id.Value);

            return offers.Select(Offer.FromDomain).ToList();
        }

        [Authorize]
        public async Task<IEnumerable<Offer>> GetReceivedOffers(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");

            var offers = await offerRepository.GetReceivedOffers(user.Id.Value);

            return offers.Select(Offer.FromDomain).ToList();
        }

        [Authorize]
        public async Task<int> GetNotificationCount(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");

            var chatCount = offerRepository.GetNotificationCount((Guid)user.Id);

            return await chatCount;
        }
    }
}