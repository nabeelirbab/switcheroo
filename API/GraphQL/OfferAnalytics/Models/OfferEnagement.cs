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

        [GraphQLNonNullType]
        public async Task<int> GetTotalRightSwipes(
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetTotalRightSwipes();
        }
        [GraphQLNonNullType]
        public async Task<int> GetTotalLeftSwipes(
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetTotalLeftSwipes();
        }

        [GraphQLNonNullType]
        public async Task<List<DailyOfferCreationTrend>> GetDailyRightSwipes(
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetDailyRightSwipes();
        }
        [GraphQLNonNullType]
        public async Task<List<WeeklyOfferCreationTrend>> GetWeeklyRightSwipes(
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetWeeklyRightSwipes();
        }
        [GraphQLNonNullType]
        public async Task<List<MonthlyOfferCreationTrend>> GetMonthlyRightSwipes(
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetMonthlyRightSwipes();
        }
        [GraphQLNonNullType]
        public async Task<List<DailyOfferCreationTrend>> GetDailyLeftSwipes(
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetDailyLeftSwipes();
        }
        [GraphQLNonNullType]
        public async Task<List<WeeklyOfferCreationTrend>> GetWeeklyLeftSwipes(
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetWeeklyLeftSwipes();
        }
        [GraphQLNonNullType]
        public async Task<List<MonthlyOfferCreationTrend>> GetMonthlyLeftSwipes(
            [Service] IOfferAnalyticsRepository offerAnalyticsRepository)
        {
            return await offerAnalyticsRepository.GetMonthlyLeftSwipes();
        }

    }
}
