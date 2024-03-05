using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.UserAnalytics
{
    public interface IUserAnalyticsRepository
    {
        Task<List<UserDailyRegistrationTrend>> GetDailyRegistrationTrend();
        Task<List<UserWeeklyRegistrationTrend>> GetWeeklyRegistrationTrend();
        Task<List<UserMonthlyRegistrationTrend>> GetMonthlyRegistrationTrend();

    }
}
