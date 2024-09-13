using Domain.Notifications;
using Domain.Services;
using Domain.Users;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Version;

namespace Infrastructure.Version
{
    public class AppVersionRepository : IAppVersionRepository
    {
        private readonly SwitcherooContext db;
        private readonly IUserRepository userRepository;
        public AppVersionRepository(SwitcherooContext db, IUserRepository userRepository)
        {
            this.db = db;
            this.userRepository = userRepository;
        }

        public async Task<Domain.Version.AppVersion> CreateAsync(Domain.Version.AppVersion version, Guid userId)
        {
            try
            {
                var now = DateTime.UtcNow;

                var newDbEntity = new Database.Schema.AppVersion(
                    version.AndroidVersion,
                    version.IOSVersion
                )
                {
                    CreatedByUserId = userId,
                    UpdatedByUserId = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await db.AppVersion.AddAsync(newDbEntity);
                await db.SaveChangesAsync();

                return await GetById(newDbEntity.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }

        public async Task<List<Domain.Version.AppVersion>> GetAll()
        {
            return await db.AppVersion
                .Select(Database.Schema.AppVersion.ToDomain)
                .ToListAsync();
        }

        public async Task<Domain.Version.AppVersion> GetById(Guid Id)
        {
            var item = await db.AppVersion
                .Where(z => z.Id == Id)
                .Select(Database.Schema.AppVersion.ToDomain)
                .FirstOrDefaultAsync();

            if (item == null) throw new InfrastructureException($"Unable to locate Id");

            return item;
        }
        public async Task<Domain.Version.AppVersion> GetLatest()
        {
            var item = await db.AppVersion
                .OrderByDescending(z => z.CreatedAt)
                .Select(Database.Schema.AppVersion.ToDomain)
                .FirstOrDefaultAsync();
            return item;
        }
    }
}
