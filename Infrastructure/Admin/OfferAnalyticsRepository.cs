using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.OfferAnalytics;
using Infrastructure.Database.Schema;

namespace Infrastructure.Admin
{
    public class OfferAnalyticsRepository : IOfferAnalyticsRepository
    {
        private readonly SwitcherooContext db;
        private readonly ILogger<OfferAnalyticsRepository> _logger;

        public OfferAnalyticsRepository(SwitcherooContext db, ILogger<OfferAnalyticsRepository> logger)
        {
            this.db = db;
            _logger = logger;
            _logger.LogDebug("Nlog is integrated to Item analytics repository");
        }

        public async Task<List<DailyOfferCreationTrend>> GetDailyTrend()
        {
            var dailyTrends = await this.db.Offers
            .AsNoTracking()
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month, Day = u.CreatedAt.Day })
            .Select(group => new DailyOfferCreationTrend
            {
                Date = new DateTime(group.Key.Year, group.Key.Month, group.Key.Day),
                Count = group.Count(),
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
            return dailyTrends;
        }
        public async Task<List<WeeklyOfferCreationTrend>> GetWeeklyTrend()
        {
            List<WeeklyOfferCreationTrend> weeklyTrends = new List<WeeklyOfferCreationTrend>();
            using (var command = this.db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = @"
                        SELECT
                        EXTRACT(YEAR FROM ""CreatedAt"") AS Year,
                        EXTRACT(WEEK FROM ""CreatedAt"") AS Week,
                        COUNT(*) AS Count
                        FROM ""Offers""
                        GROUP BY EXTRACT(YEAR FROM ""CreatedAt""), EXTRACT(WEEK FROM ""CreatedAt"")
                        ORDER BY Year, Week";
                this.db.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        weeklyTrends.Add(new WeeklyOfferCreationTrend
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
        public async Task<List<MonthlyOfferCreationTrend>> GetMonthlyTrend()
        {
            var monthlyTrends = await this.db.Offers
            .AsNoTracking()
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month })
            .Select(group => new MonthlyOfferCreationTrend
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

        public async Task<OfferCountByType> GetCountByType()
        {
            var counts = await this.db.Offers
        .GroupBy(o => 1) // Arbitrary grouping to perform aggregate functions
        .Select(group => new
        {
            CashOffers = group.Count(o => o.Cash > 0),
            AcceptedCashOffers = group.Count(o => o.Cash > 0 && o.SourceStatus == o.TargetStatus),
            UnMatchedOffers = group.Count(o => (o.Cash > 0) == false || o.Cash == null),
            MatchedOffers = group.Count(o => ((o.Cash > 0) == false || o.Cash == null) && o.SourceStatus == o.TargetStatus)
        })
        .FirstOrDefaultAsync();

            if (counts == null) // In case there are no offers at all
            {
                return new OfferCountByType { CashOffers = 0, AcceptedCashOffers = 0, UnMatchedOffers = 0, MatchedOffers = 0 };
            }

            return new OfferCountByType
            {
                CashOffers = counts.CashOffers,
                AcceptedCashOffers = counts.AcceptedCashOffers,
                UnMatchedOffers = counts.UnMatchedOffers,
                MatchedOffers = counts.MatchedOffers
            };
        }

        public async Task<int> GetTotalRightSwipes()
        {
            var count = await this.db.Offers
            .AsNoTracking()
            .Where(o => o.Cash <= 0 || o.Cash == null)
            .CountAsync();
            return count;
        }
        public async Task<List<DailyOfferCreationTrend>> GetDailyRightSwipes()
        {
            var dailyTrends = await this.db.Offers
            .AsNoTracking()
            .Where(o => o.Cash <= 0 || o.Cash == null)
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month, Day = u.CreatedAt.Day })
            .Select(group => new DailyOfferCreationTrend
            {
                Date = new DateTime(group.Key.Year, group.Key.Month, group.Key.Day),
                Count = group.Count(),
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
            return dailyTrends;
        }

        public async Task<List<WeeklyOfferCreationTrend>> GetWeeklyRightSwipes()
        {
            List<WeeklyOfferCreationTrend> weeklyTrends = new List<WeeklyOfferCreationTrend>();
            using (var command = this.db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = @"
                        SELECT
                        EXTRACT(YEAR FROM ""CreatedAt"") AS Year,
                        EXTRACT(WEEK FROM ""CreatedAt"") AS Week,
                        COUNT(*) AS Count
                        FROM ""Offers""
                        WHERE ""Cash"" is NULL
                        GROUP BY EXTRACT(YEAR FROM ""CreatedAt""), EXTRACT(WEEK FROM ""CreatedAt"")
                        ORDER BY Year, Week";
                this.db.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        weeklyTrends.Add(new WeeklyOfferCreationTrend
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

        public async Task<List<MonthlyOfferCreationTrend>> GetMonthlyRightSwipes()
        {
            var monthlyTrends = await this.db.Offers
            .AsNoTracking()
            .Where(o => o.Cash <= 0 || o.Cash == null)
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month })
            .Select(group => new MonthlyOfferCreationTrend
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

        public async Task<int> GetTotalLeftSwipes()
        {
            var count = await this.db.DismissedItem
            .AsNoTracking()
            .CountAsync();
            return count;
        }

        public async Task<List<DailyOfferCreationTrend>> GetDailyLeftSwipes()
        {
            var dailyTrends = await this.db.DismissedItem
            .AsNoTracking()
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month, Day = u.CreatedAt.Day })
            .Select(group => new DailyOfferCreationTrend
            {
                Date = new DateTime(group.Key.Year, group.Key.Month, group.Key.Day),
                Count = group.Count(),
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
            return dailyTrends;
        }
        public async Task<List<WeeklyOfferCreationTrend>> GetWeeklyLeftSwipes()
        {
            List<WeeklyOfferCreationTrend> weeklyTrends = new List<WeeklyOfferCreationTrend>();
            using (var command = this.db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = @"
                        SELECT
                        EXTRACT(YEAR FROM ""CreatedAt"") AS Year,
                        EXTRACT(WEEK FROM ""CreatedAt"") AS Week,
                        COUNT(*) AS Count
                        FROM ""DismissedItem""
                        GROUP BY EXTRACT(YEAR FROM ""CreatedAt""), EXTRACT(WEEK FROM ""CreatedAt"")
                        ORDER BY Year, Week";
                this.db.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        weeklyTrends.Add(new WeeklyOfferCreationTrend
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

        public async Task<List<MonthlyOfferCreationTrend>> GetMonthlyLeftSwipes()
        {
            var monthlyTrends = await this.db.DismissedItem
            .AsNoTracking()
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month })
            .Select(group => new MonthlyOfferCreationTrend
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
    }
}
