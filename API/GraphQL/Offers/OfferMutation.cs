using System;
using System.Threading.Tasks;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Offer = API.GraphQL.Models.Offer;

namespace API.GraphQL
{
    public partial class Mutation
    {
        public async Task<Offer> MarkMessagesAsRead(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository,
            string offerId
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var domainOffer = await offerRepository.MarkMessagesAsRead(user.Id.Value, Guid.Parse(offerId));
            return Offer.FromDomain(domainOffer);
        }

        public async Task<Offer> CreateOffer(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository,
            Guid sourceItemId,
            Guid targetItemId,
            int sourceStatus,
            int? cash,
            int? targeteStatus
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var domainOffer = await offerRepository.CreateOffer(Domain.Offers.Offer.CreateNewOffer(sourceItemId, targetItemId, cash, user.Id.Value, sourceStatus, targeteStatus));
            
            return Offer.FromDomain(domainOffer);
        }

        public async Task<bool> DeleteOffer(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository,
            Guid id
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            await offerRepository.DeleteOffer(id,(Guid)user.Id);

            return true;
        }

        public async Task<bool> AcceptOffer(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository,
            Guid offerId
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            await offerRepository.AcceptOffer(offerId);

            return true;
        }
    }
}
