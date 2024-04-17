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

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<IEnumerable<Offer>> GetAllOffersByItemId(
            [Service] IOfferRepository offerRepository,
            Guid itemId)
        {
            var offers = await offerRepository.GetAllOffersByItemId(itemId);
            return offers.Select(Offer.FromDomain).ToList();
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<IEnumerable<Offer>> GetCreatedOffers(
            [Service] UserContextService userContextService,
            [Service] IOfferRepository offerRepository)
        {
            var requestedUserId = userContextService.GetCurrentUserId();
            var offers = await offerRepository.GetCreatedOffers(requestedUserId);
            return offers.Select(Offer.FromDomain).ToList();
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<IEnumerable<Offer>> GetReceivedOffers(
            [Service] UserContextService userContextService,
            [Service] IOfferRepository offerRepository)
        {
            var requestedUserId = userContextService.GetCurrentUserId();
            var offers = await offerRepository.GetReceivedOffers(requestedUserId);
            return offers.Select(Offer.FromDomain).ToList();
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<int> GetNotificationCount(
            [Service] UserContextService userContextService,
            [Service] IOfferRepository offerRepository)
        {
            var requestUserId = userContextService.GetCurrentUserId();
            var chatCount = offerRepository.GetNotificationCount(requestUserId);
            return await chatCount;
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<Paginated<Offer>> GetAllMatchedOffers(
            [Service] IOfferRepository offerRepository, int limit, string? cursor)
        {
            var pageinatedOffers = await offerRepository.GetAllMatchedOffers(limit, cursor);
            return new Paginated<Offer>(
                pageinatedOffers.Data
                    .Select(Offer.FromDomain)
                    .ToList(),
                pageinatedOffers.Cursor,
                pageinatedOffers.TotalCount,
                pageinatedOffers.HasNextPage);
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<Paginated<Offer>> GetAllPendingMatchingOffers(
            [Service] IOfferRepository offerRepository, int limit, string? cursor)
        {
            var pageinatedOffers = await offerRepository.GetAllPendingMatchingOffers(limit, cursor);
            return new Paginated<Offer>(
                pageinatedOffers.Data
                    .Select(Offer.FromDomain)
                    .ToList(),
                pageinatedOffers.Cursor,
                pageinatedOffers.TotalCount,
                pageinatedOffers.HasNextPage);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<Paginated<Offer>> GetAllAcceptedCashOffers(
            [Service] IOfferRepository offerRepository, int limit, string? cursor)
        {
            var pageinatedOffers = await offerRepository.GetAllAcceptedCashOffers(limit, cursor);
            return new Paginated<Offer>(
                pageinatedOffers.Data
                    .Select(Offer.FromDomain)
                    .ToList(),
                pageinatedOffers.Cursor,
                pageinatedOffers.TotalCount,
                pageinatedOffers.HasNextPage);
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<Paginated<Offer>> GetAllPendingCashOffers(
            [Service] IOfferRepository offerRepository, int limit, string? cursor)
        {
            var pageinatedOffers = await offerRepository.GetAllPendingCashOffers(limit, cursor);
            return new Paginated<Offer>(
                pageinatedOffers.Data
                    .Select(Offer.FromDomain)
                    .ToList(),
                pageinatedOffers.Cursor,
                pageinatedOffers.TotalCount,
                pageinatedOffers.HasNextPage);
        }


    }
}