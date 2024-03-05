using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.ItemAnalytics;
using Domain.Items;
using Domain.Offers;
using Domain.UserAnalytics;
using Domain.Users;
using HotChocolate;
using Infrastructure.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace API.GraphQL.ItemAnalytics.Models
{
    public class ItemEnagement
    {

        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.ItemAnalytics.ItemDailyRegistrationTrend>> DailyRegistrationTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemAnalyticsRepository itemAnalyticsRepository)
        {
            return await itemAnalyticsRepository.GetDailyRegistrationTrend();
        }


        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.ItemAnalytics.ItemWeeklyRegistrationTrend>> WeeklyRegistrationTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemAnalyticsRepository itemAnalyticsRepository)
        {
            return await itemAnalyticsRepository.GetWeeklyRegistrationTrend();
        }


        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.ItemAnalytics.ItemMonthlyRegistrationTrend>> MonthlyRegistrationTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemAnalyticsRepository itemAnalyticsRepository)
        {
            return await itemAnalyticsRepository.GetMonthlyRegistrationTrend();
        }

    }
}
