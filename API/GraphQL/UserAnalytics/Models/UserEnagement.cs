using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Items;
using Domain.Offers;
using Domain.UserAnalytics;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Offer = API.GraphQL.Models.Offer;

namespace API.GraphQL.UserAnalytics.Models
{
    public class UserEnagement
    {

        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.UserAnalytics.UserDailyRegistrationTrend>> DailyRegistrationTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserAnalyticsRepository userAnalyticsRepository)
        {
            return await userAnalyticsRepository.GetDailyRegistrationTrend();
        }


        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.UserAnalytics.UserWeeklyRegistrationTrend>> WeeklyRegistrationTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserAnalyticsRepository userAnalyticsRepository)
        {
            return await userAnalyticsRepository.GetWeeklyRegistrationTrend();
        }


        [GraphQLNonNullType]
        public async Task<IEnumerable<Domain.UserAnalytics.UserMonthlyRegistrationTrend>> MonthlyRegistrationTrend(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IUserAnalyticsRepository userAnalyticsRepository)
        {
            return await userAnalyticsRepository.GetMonthlyRegistrationTrend();
        }

    }
}
