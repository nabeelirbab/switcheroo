using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.ItemAnalytics
{
    public interface IItemAnalyticsRepository
    {
        Task<List<ItemDailyRegistrationTrend>> GetDailyRegistrationTrend();
        Task<List<ItemWeeklyRegistrationTrend>> GetWeeklyRegistrationTrend();
        Task<List<ItemMonthlyRegistrationTrend>> GetMonthlyRegistrationTrend();

    }
}
