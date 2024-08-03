using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Feedback;

namespace Infrastructure.Notifications
{
    public class FeedbackRepositoy : IFeedbackRepository
    {
        private readonly SwitcherooContext db;
        private readonly IUserRepository userRepository;
        public FeedbackRepositoy(SwitcherooContext db, IUserRepository userRepository)
        {
            this.db = db;
            this.userRepository = userRepository;
        }

        public async Task<Domain.Feedback.Feedback> CreateFeedbackAsync(Domain.Feedback.Feedback feedback, Guid userId)
        {
            try
            {
                var now = DateTime.UtcNow;

                var newDbFeedback = new Database.Schema.Feedback(
                    feedback.Title,
                    feedback.Description,
                    feedback.Status
                )
                {
                    CreatedByUserId = userId,
                    UpdatedByUserId = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await db.Feedback.AddAsync(newDbFeedback);
                await db.SaveChangesAsync();
                return await GetFeedbackById(newDbFeedback.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }
        public async Task<string> UpdateFeedbackStatusAsync(Guid id, Guid userId, FeedbackStatus status)
        {
            try
            {
                var now = DateTime.UtcNow;

                var dbFeedback = await db.Feedback.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (dbFeedback == null)
                    return "No feedback found for the specified id";
                dbFeedback.Status = status;
                dbFeedback.UpdatedAt = now;
                dbFeedback.UpdatedByUserId = userId;
                await db.SaveChangesAsync();
                return "Feedback status updated successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }

        public async Task<List<Domain.Feedback.Feedback>> GetFeedbacks()
        {
            return await db.Feedback
                .Select(Database.Schema.Feedback.ToDomain)
                .ToListAsync();
        }
        public async Task<List<Domain.Feedback.Feedback>> GetFeedbacksByStatus(FeedbackStatus status)
        {
            return await db.Feedback
                .Where(x => x.Status == status)
                .Select(Database.Schema.Feedback.ToDomain)
                .ToListAsync();
        }

        public async Task<Domain.Feedback.Feedback> GetFeedbackById(Guid feedbackId)
        {
            var item = await db.Feedback
                .Where(z => z.Id == feedbackId)
                .Select(Database.Schema.Feedback.ToDomain)
                .FirstOrDefaultAsync();

            if (item == null) throw new InfrastructureException($"Unable to locate notificaitonId");

            return item;
        }
    }
}
