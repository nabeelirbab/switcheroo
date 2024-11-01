﻿using System;
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
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<ItemAnalytics.Models.ItemEnagement> GetItemEngagement([Service] IItemAnalyticsRepository itemAnalyticsRepository)
        {
            return new ItemAnalytics.Models.ItemEnagement();
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<decimal> GetAllItemsAveragePrice([Service] IItemAnalyticsRepository itemAnalyticsRepository)
        {
            return await itemAnalyticsRepository.GetAveragePriceOfAllItems();
        }

    }
}
