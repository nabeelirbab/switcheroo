using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.OfferAnalytics
{
    public interface IOfferAnalyticsRepository
    {
        Task<List<DailyOfferCreationTrend>> GetDailyTrend();
        Task<List<WeeklyOfferCreationTrend>> GetWeeklyTrend();
        Task<List<MonthlyOfferCreationTrend>> GetMonthlyTrend();
        Task<OfferCountByType> GetCountByType();

        Task<int> GetTotalRightSwipes();
        Task<List<DailyOfferCreationTrend>> GetDailyRightSwipes();
        Task<List<WeeklyOfferCreationTrend>> GetWeeklyRightSwipes();
        Task<List<MonthlyOfferCreationTrend>> GetMonthlyRightSwipes();

        Task<int> GetTotalLeftSwipes();
        Task<List<DailyOfferCreationTrend>> GetDailyLeftSwipes();
        Task<List<WeeklyOfferCreationTrend>> GetWeeklyLeftSwipes();
        Task<List<MonthlyOfferCreationTrend>> GetMonthlyLeftSwipes();

    }
}
