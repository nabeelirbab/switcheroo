using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.ItemAnalytics;
using Domain.OfferAnalytics;
using Domain.Users;
using HotChocolate;
using Infrastructure.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace API.GraphQL.OfferAnalytics.Models
{
    public class OfferEnagement
    {

        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.OfferAnalytics.DailyOfferCreationTrend>> DailyTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetDailyTrend();
        }


        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.OfferAnalytics.WeeklyOfferCreationTrend>> WeeklyTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetWeeklyTrend();
        }


        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.OfferAnalytics.MonthlyOfferCreationTrend>> MonthlyTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetMonthlyTrend();
        }

        [GraphQLNonNullType]
        public async Task<Domain.OfferAnalytics.OfferCountByType> CountByType(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetCountByType();
        }

    }
}
