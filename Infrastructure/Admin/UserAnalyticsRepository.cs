using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Users;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.UserAnalytics;

namespace Infrastructure.Admin
{
    public class UserAnalyticsRepository : IUserAnalyticsRepository
    {
        private readonly SwitcherooContext db;
        private readonly ILogger<UserAnalyticsRepository> _logger;

        public UserAnalyticsRepository(SwitcherooContext db, ILogger<UserAnalyticsRepository> logger)
        {
            this.db = db;
            _logger = logger;
            _logger.LogDebug("Nlog is integrated to User analytics repository");
        }

        public async Task<List<UserDailyRegistrationTrend>> GetDailyRegistrationTrend()
        {
            var dailyTrends = await this.db.Users
            .AsNoTracking()
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month, Day = u.CreatedAt.Day })
            .Select(group => new UserDailyRegistrationTrend
            {
                Date = new DateTime(group.Key.Year, group.Key.Month, group.Key.Day),
                Count = group.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
            return dailyTrends;
        }
        public async Task<List<UserWeeklyRegistrationTrend>> GetWeeklyRegistrationTrend()
        {
            List<UserWeeklyRegistrationTrend> weeklyTrends = new List<UserWeeklyRegistrationTrend>();
            using (var command = this.db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = @"
                        SELECT
                        EXTRACT(YEAR FROM ""CreatedAt"") AS Year,
                        EXTRACT(WEEK FROM ""CreatedAt"") AS Week,
                        COUNT(*) AS Count
                        FROM ""Users""
                        GROUP BY EXTRACT(YEAR FROM ""CreatedAt""), EXTRACT(WEEK FROM ""CreatedAt"")
                        ORDER BY Year, Week";
                this.db.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        weeklyTrends.Add(new UserWeeklyRegistrationTrend
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
        public async Task<List<UserMonthlyRegistrationTrend>> GetMonthlyRegistrationTrend()
        {
            var monthlyTrends = await this.db.Users
            .AsNoTracking()
            .GroupBy(u => new { Year = u.CreatedAt.Year, Month = u.CreatedAt.Month })
            .Select(group => new UserMonthlyRegistrationTrend
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
