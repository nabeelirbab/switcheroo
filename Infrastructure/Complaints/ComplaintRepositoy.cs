using Domain.Categories;
using Domain.Complaints;
using Domain.Services;
using Infrastructure.Database;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Complaints
{
    public class ComplaintRepositoy: IComplaintRepository
    {
        private readonly SwitcherooContext db;
        private readonly ILoggerManager _loggerManager;

        public ComplaintRepositoy(SwitcherooContext db, ILoggerManager loggerManager)
        {
            this.db = db;
            _loggerManager = loggerManager;
        }

        public async Task<Complaint> CreateComplaintAsync(Complaint complaint)
        {
            try
            {
                var now = DateTime.UtcNow;
                if (!complaint.CreatedByUserId.HasValue)
                    throw new InfrastructureException("No createdByUserId provided");

                var newDbComplaints = new Database.Schema.Complaint(
                    complaint.Title,
                    complaint.Description,
                    complaint.IsSolved,
                    complaint.TargetUserId,
                    complaint.TargetItemId
                )
                {
                    CreatedByUserId = complaint.CreatedByUserId.Value,
                    UpdatedByUserId = complaint.CreatedByUserId.Value,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _loggerManager.LogError($"{newDbComplaints.Description}");
                await db.Complaints.AddAsync(newDbComplaints);
                await db.SaveChangesAsync();

                return await GetComplaintById(newDbComplaints.Id);
            }
            catch (Exception ex)
            {
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }

        public async Task<Complaint> GetComplaintById(Guid complaintId)
        {
            var item = await db.Complaints
                .Where(z => z.Id == complaintId)
                .Select(Database.Schema.Complaint.ToDomain)
                .SingleOrDefaultAsync();

            if (item == null) throw new InfrastructureException($"Unable to locate item {complaintId}");

            return item;
        }

        public async Task<List<Complaint>> GetComplaints()
        {
            return await db.Complaints
                .Select(Database.Schema.Complaint.ToDomain)
                .ToListAsync();
        }
    }
}
