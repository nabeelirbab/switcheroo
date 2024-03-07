using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Users;
using Domain.UserAnalytics;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Domain.ItemAnalytics;

namespace API.GraphQL
{
    public partial class Query
    {
        public async Task<OfferAnalytics.Models.OfferEnagement> GetOfferEngagement([Service] IItemAnalyticsRepository itemAnalyticsRepository)
        {
            return new OfferAnalytics.Models.OfferEnagement();
        }
    }
}
