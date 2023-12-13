using System;
using System.Threading.Tasks;

namespace Domain.Complaints
{
    public interface IComplaintRepository
    {
        Task<Complaint> CreateComplaintAsync(Complaint complaint);
        Task<Complaint> GetComplaintById(Guid complaintId);
    }
}
