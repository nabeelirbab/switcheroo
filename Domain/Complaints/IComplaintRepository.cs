using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Complaints
{
    public interface IComplaintRepository
    {
        Task<Complaint> CreateComplaintAsync(Complaint complaint);
        Task<bool> CheckReportedUserAgainstRequestedUser(Guid requestUserId,Guid reportedUserId);
        Task<bool> CheckReportedItemAgainstRequestedUser(Guid requestUserId,Guid reportedItemId);
        Task<Complaint> GetComplaintById(Guid complaintId);

        Task<List<Complaint>> GetComplaints();

        Task<List<Complaint>> GetRestrictedItems();

        Task<List<Complaint>> GetRestrictedUsers();
    }
}
