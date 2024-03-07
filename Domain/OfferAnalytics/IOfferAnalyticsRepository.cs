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

    }
}
