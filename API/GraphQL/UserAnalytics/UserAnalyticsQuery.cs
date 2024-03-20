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

namespace API.GraphQL
{
    public partial class Query
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<UserAnalytics.Models.UserEnagement> GetUserEngagement([Service] IUserAnalyticsRepository userAnalyticsRepository)
        {
            return new UserAnalytics.Models.UserEnagement();
        }
    }
}
