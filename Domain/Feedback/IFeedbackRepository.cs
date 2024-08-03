using Domain.Complaints;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Feedback
{
    public interface IFeedbackRepository
    {
        Task<Feedback> CreateFeedbackAsync(Feedback feedback, Guid userId);
        Task<string> UpdateFeedbackStatusAsync(Guid id,Guid userId ,FeedbackStatus status);
        Task<Feedback> GetFeedbackById(Guid feedbackId);
        Task<List<Feedback>> GetFeedbacks();
        Task<List<Feedback>> GetFeedbacksByStatus(FeedbackStatus status);

    }
}
