using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Complaints
{
    public interface IComplaintRepository
    {
        Task<Complaint> CreateComplaintAsync(Complaint complaint);
        Task<Complaint> GetComplaintById(Guid complaintId);

        Task<List<Complaint>> GetComplaints();

        Task<List<Complaint>> GetRestrictedItems();

        Task<List<Complaint>> GetRestrictedUsers();
    }
}
