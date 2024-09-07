using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.ItemAnalytics;

namespace Infrastructure.Admin
{
    public class ItemAnalyticsRepository : IItemAnalyticsRepository
    {
        private readonly SwitcherooContext db;
        private readonly ILogger<ItemAnalyticsRepository> _logger;

        public ItemAnalyticsRepository(SwitcherooContext db, ILogger<ItemAnalyticsRepository> logger)
        {
            this.db = db;
            _logger = logger;
            _logger.LogDebug("Nlog is integrated to Item analytics repository");
        }

        public async Task<List<ItemDailyRegistrationTrend>> GetDailyRegistrationTrend()
        {
            var dailyTrends = await this.db.Items
            .AsNoTracking()
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month, Day = u.CreatedAt.Day })
            .Select(group => new ItemDailyRegistrationTrend
            {
                Date = new DateTime(group.Key.Year, group.Key.Month, group.Key.Day),
                Count = group.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
            return dailyTrends;
        }
        public async Task<List<ItemWeeklyRegistrationTrend>> GetWeeklyRegistrationTrend()
        {
            List<ItemWeeklyRegistrationTrend> weeklyTrends = new List<ItemWeeklyRegistrationTrend>();
            using (var command = this.db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = @"
                        SELECT
                        EXTRACT(YEAR FROM ""CreatedAt"") AS Year,
                        EXTRACT(WEEK FROM ""CreatedAt"") AS Week,
                        COUNT(*) AS Count
                        FROM ""Items""
                        GROUP BY EXTRACT(YEAR FROM ""CreatedAt""), EXTRACT(WEEK FROM ""CreatedAt"")
                        ORDER BY Year, Week";
                this.db.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        weeklyTrends.Add(new ItemWeeklyRegistrationTrend
                        {
                            Year = Convert.ToInt32(result["Year"]),
                            Week = Convert.ToInt32(result["Week"]),
                            Count = Convert.ToInt32(result["Count"])
                        });
                    }
                }
            }
            return weeklyTrends;
        }
        public async Task<List<ItemMonthlyRegistrationTrend>> GetMonthlyRegistrationTrend()
        {
            var monthlyTrends = await this.db.Items
            .AsNoTracking()
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month })
            .Select(group => new ItemMonthlyRegistrationTrend
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                Count = group.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();
            return monthlyTrends;
        }

        public async Task<decimal> GetAveragePriceOfAllItems()
        {
            var items = await db.Items.ToListAsync();
            if (!items.Any())
                return 0;
            var averagePrice = Math.Round(items.Average(item => item.AskingPrice),2);

            return averagePrice;
        }
    }
}
