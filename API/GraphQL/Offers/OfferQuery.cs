using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        [Authorize]
        public async Task<Offer> GetOffer(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferRepository offerRepository,
            string offerId)
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);
            if (!user.Id.HasValue) throw new ApiException("Fatal.");

            if (user == null) throw new ApiException("Not logged in");
            
            return Offer.FromDomain(await offerRepository.GetOfferById(user.Id.Value, Guid.Parse(offerId)));
        }
    }
}