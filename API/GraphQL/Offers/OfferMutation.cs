using System;
using System.Threading.Tasks;
using API.GraphQL.CommonServices;
using Domain.Offers;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Offer = API.GraphQL.Models.Offer;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Offer> MarkMessagesAsRead(
            [Service] UserContextService userContextService,
            [Service] IOfferRepository offerRepository,
            string offerId
        )
        {
            var domainOffer = await offerRepository.MarkMessagesAsRead(userContextService.GetCurrentUserId(), Guid.Parse(offerId));
            return Offer.FromDomain(domainOffer);
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> MarkNotificationAsRead(
            [Service] UserContextService userContextService,
            [Service] IOfferRepository offerRepository
        )
        {
            await offerRepository.MarkNotificationRead(userContextService.GetCurrentUserId());
            return true;
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Offer> CreateOffer(
            [Service] UserContextService userContextService,
            [Service] IOfferRepository offerRepository,
            Guid? sourceItemId,
            Guid targetItemId,
            int? sourceStatus,
            int? cash,
            int? targeteStatus
        )
        {
            bool isRead = false;
            if (cash != null && cash > 0) sourceItemId = targetItemId;
            var domainOffer = await offerRepository.CreateOffer(Domain.Offers.Offer.CreateNewOffer(sourceItemId ?? targetItemId, targetItemId, cash, userContextService.GetCurrentUserId(), sourceStatus ?? 1, targeteStatus, isRead));
            return Offer.FromDomain(domainOffer);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> DeleteOffer(
            [Service] UserContextService userContextService,
            [Service] IOfferRepository offerRepository,
            Guid id
        )
        {
            await offerRepository.DeleteOffer(id, userContextService.GetCurrentUserId());
            return true;
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> AcceptOffer(
            [Service] IOfferRepository offerRepository,
            Guid offerId
        )
        {
            await offerRepository.AcceptOffer(offerId);
            return true;
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> UnmatchOffer(
            [Service] IOfferRepository offerRepository,
            Guid offerId
        )
        {
            await offerRepository.UnmatchOffer(offerId);
            return true;
        }
    }
}
